using SharedCommerce.Api.Configuration;
using DropCommerce.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCommerceSecurity();

app.MapHealthChecks("/health")
    .AllowAnonymous();

app.MapControllers();

app.Run();