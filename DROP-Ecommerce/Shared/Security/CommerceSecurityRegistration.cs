using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Commerce.Security;

/// <summary>
/// Centraliza a configuração HTTP de segurança para manter Store e Drop com as mesmas proteções.
/// </summary>
public static class CommerceSecurityRegistration
{
    /// <summary>
    /// Configura JWT, autorização, CSRF e limites de requisições para aplicar a mesma proteção nas duas APIs.
    /// </summary>
    public static IServiceCollection AddCommerceSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SecurityOptions>().Bind(configuration.GetSection(SecurityOptions.Section))
            .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32, "Security:SigningKey requer no mínimo 32 bytes, configurados fora do código.")
            .Validate(options => options.Issuer.Length > 0 && options.AccessTokenMinutes is > 0 and <= 60 && options.RefreshFamilyDays is > 0 and <= 90, "Prazos de token inválidos.")
            .Validate(options => options.FailedLoginThreshold > 0 && options.InitialLockoutSeconds > 0 && options.MaximumLockoutSeconds >= options.InitialLockoutSeconds && options.FailureResetMinutes > 0, "Bloqueio progressivo inválido.")
            .Validate(options => options.AuthRequestsPerMinute > 0 && options.RefreshRequestsPerMinute > 0 && options.ApiRequestsPerMinute > 0 && options.TokenRetentionDays >= 0, "Limites/retencão inválidos.")
            .Validate(options => options.InvitationHours > 0 && options.OwnershipTransferMinutes > 0 && options.ActionTokenMinutes > 0, "Prazos de ação inválidos.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "CommerceJwt";
            options.DefaultChallengeScheme = "CommerceJwt";
        }).AddPolicyScheme("CommerceJwt", null, options =>
        {
            // Escolhe o esquema pelo público do token; a assinatura ainda será validada pelo JWT.
            options.ForwardDefaultSelector = context =>
            {
                var header = context.Request.Headers.Authorization.ToString();

                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (header.Length > 32775)
                            return SecurityNames.Backoffice;

                        var token = new JwtSecurityTokenHandler().ReadJwtToken(header[7..]);

                        if (token.Audiences.Contains(SecurityNames.Storefront))
                            return SecurityNames.Storefront;
                    }
                    catch (ArgumentException)
                    {
                    }
                }

                return SecurityNames.Backoffice;
            };
        }).AddJwtBearer(SecurityNames.Backoffice, _ =>
{
}).AddJwtBearer(SecurityNames.Storefront, _ =>
{
});

        foreach (var audience in new[] { SecurityNames.Backoffice, SecurityNames.Storefront })
        {
            // Exige assinatura, prazo e público corretos para separar administração e clientes.
            services.AddOptions<JwtBearerOptions>(audience).Configure<IOptions<SecurityOptions>>((options, settings) =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Value.Issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    RequireAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.SigningKey)),
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(15),
                    NameClaimType = "sub"
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Distingue indisponibilidade de credenciais inválidas e não libera acesso sem validação.
                        if (context.Exception is SecurityFailure { Status: 503 })
                            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(context.Exception).Throw();

                        return Task.CompletedTask;
                    },
                    // Confere o estado da sessão para que um JWT assinado também respeite revogações.
                    OnTokenValidated = async context =>
                    {
                        var validator = context.HttpContext.RequestServices.GetRequiredService<ISessionValidator>();

                        if (context.Principal is null || !await validator.ValidateAsync(context.Principal, context.Request.Host.Host, context.HttpContext.RequestAborted))
                            context.Fail("Sessão inválida ou revogada.");
                    }
                };
            });
        }

        // Separa tipos de acesso e exige uma loja selecionada nas operações administrativas comuns.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(SecurityNames.GlobalUser, new AuthorizationPolicyBuilder(SecurityNames.Backoffice)
                .RequireAuthenticatedUser().RequireClaim(SecurityNames.Actor, "user").Build());
            options.AddPolicy(SecurityNames.Backoffice, new AuthorizationPolicyBuilder(SecurityNames.Backoffice)
                .RequireAuthenticatedUser().RequireClaim(SecurityNames.Actor, "user")
                .RequireClaim(SecurityNames.Enterprise).RequireClaim(SecurityNames.Membership).Build());
            options.AddPolicy(SecurityNames.Storefront, new AuthorizationPolicyBuilder(SecurityNames.Storefront)
                .RequireAuthenticatedUser().RequireClaim(SecurityNames.Actor, "customer")
                .RequireClaim(SecurityNames.Enterprise).RequireClaim(SecurityNames.Customer).Build());
            options.AddPolicy(SecurityNames.PrimaryOwner, new AuthorizationPolicyBuilder(SecurityNames.Backoffice)
                .RequireAuthenticatedUser().RequireClaim(SecurityNames.Actor, "user")
                .RequireClaim(SecurityNames.Enterprise).RequireClaim(SecurityNames.Membership)
                .RequireClaim(SecurityNames.Kind, "primary_owner").Build());
            options.FallbackPolicy = options.GetPolicy(SecurityNames.Backoffice);
        });
        // Aceita domínio e IP encaminhados somente por proxies explicitamente confiáveis.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            var security = configuration.GetSection(SecurityOptions.Section).Get<SecurityOptions>() ?? new();
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var proxy in security.TrustedProxies)
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            // Sem proxy configurado, o pipeline não usa os headers encaminhados.
        });
        // Permite cookies entre origens somente para os sites autorizados na configuração.
        services.AddCors(options => options.AddPolicy("CommerceBrowser", policy =>
        {
            var origins = configuration.GetSection("Security:AllowedOrigins").Get<string[]>() ?? [];

            if (origins.Length > 0)
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }));
        // Vincula o header CSRF a um cookie protegido para rejeitar ações forjadas pelo navegador.
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-Commerce-Csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
        services.AddRateLimiter(options =>
        {
            var settings = configuration.GetSection(SecurityOptions.Section).Get<SecurityOptions>() ?? new();
            options.RejectionStatusCode = 429;
            // Informa o excesso de requisições e o prazo de espera sem revelar detalhes internos.
            options.OnRejected = async (context, ct) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
                    context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retry.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    code = "Security.RateLimited",
                    message = "Muitas requisições. Tente novamente depois."
                }, ct);
            };
            // Distribui os limites por IP e operação usando as requisições da janela deslizante.
            foreach (var (name, permits) in new[] { ("auth", settings.AuthRequestsPerMinute), ("refresh", settings.RefreshRequestsPerMinute), ("api", settings.ApiRequestsPerMinute) })
                options.AddPolicy(name, context => RateLimitPartition.GetSlidingWindowLimiter(
                    $"{context.Connection.RemoteIpAddress}:{name}", _ => new()
                    {
                        PermitLimit = permits,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return services;
    }

    /// <summary>
    /// Organiza os middlewares para identificar a loja antes de autenticar, limitar requisições e autorizar o acesso.
    /// </summary>
    public static IApplicationBuilder UseCommerceSecurity(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<SecurityOptions>>().Value;

        if (options.TrustedProxies.Length > 0)
            app.UseForwardedHeaders();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("CommerceBrowser");
        // Resolve a loja antes da autenticação para manter a validação no contexto correto.
        app.Use(async (context, next) =>
        {
            var resolver = context.RequestServices.GetService<IRequestTenantResolver>();

            if (resolver is not null)
                await resolver.ResolveAsync(context);

            await next(context);
        });
        app.UseAuthentication();
        // Evita que respostas autenticadas sejam reutilizadas pelo cache de outra sessão.
        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
                context.Response.Headers.CacheControl = "no-store";

            await next(context);
        });
        app.UseRateLimiter();
        app.UseAuthorization();

        return app;
    }
}
