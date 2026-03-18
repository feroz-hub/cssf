# Placeholder image: currently builds the gateway library package.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build src/Gateway/HCL.CS.SF.Gateway/HCL.CS.SF.ProxyService.csproj -c Release

CMD ["dotnet", "--info"]
