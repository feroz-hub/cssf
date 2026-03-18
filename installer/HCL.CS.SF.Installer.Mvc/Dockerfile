FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Installer/HCL.CS.SFInstallerMVC/HCL.CS.SFInstallerMVC.csproj
RUN dotnet publish Installer/HCL.CS.SFInstallerMVC/HCL.CS.SFInstallerMVC.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HCL.CS.SFInstallerMVC.dll"]
