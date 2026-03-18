# Uses the demo server host as the current identity runtime entrypoint.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish demos/HCL.CS.SF.Demo.Server/HCL.CS.SF.DemoServerApp.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends bash curl openssl postgresql-client \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
COPY scripts /app/scripts
COPY docker/entrypoint.identity.sh /app/docker/entrypoint.identity.sh
RUN chmod +x /app/docker/entrypoint.identity.sh
ENV ASPNETCORE_URLS=https://+:8443
EXPOSE 8443
ENTRYPOINT ["/app/docker/entrypoint.identity.sh"]
