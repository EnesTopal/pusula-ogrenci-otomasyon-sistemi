# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Pusula.UI/Pusula.UI.csproj", "Pusula.UI/"]
COPY ["Pusula.Api/Pusula.Api.csproj", "Pusula.Api/"]
RUN dotnet restore "Pusula.UI/Pusula.UI.csproj"


COPY . .

# Remove conflicting appsettings files from API project to avoid conflicts
RUN rm -f Pusula.Api/appsettings*.json

WORKDIR /src/Pusula.UI
RUN dotnet build "Pusula.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pusula.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pusula.UI.dll"]
