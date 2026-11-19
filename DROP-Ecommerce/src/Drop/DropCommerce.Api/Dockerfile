# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS runtime
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /home/app/.aspnet
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_ENVIRONMENT=Production
USER app
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD curl --fail --silent --show-error http://127.0.0.1:8080/health || exit 1

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
ARG TARGETARCH
WORKDIR /src
COPY DROP-Ecommerce/ DROP-Ecommerce/
RUN dotnet publish DROP-Ecommerce/src/Drop/DropCommerce.Api/DropCommerce.Api.csproj \
    --configuration Release --arch $TARGETARCH --self-contained false \
    -p:UseAppHost=false -p:InstallGitHooks=false --output /app/publish

FROM runtime AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DropCommerce.Api.dll"]
