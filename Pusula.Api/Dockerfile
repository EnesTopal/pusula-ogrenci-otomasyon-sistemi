# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Pusula.Api.csproj", "."]
RUN dotnet restore "./Pusula.Api.csproj"
COPY . .
RUN dotnet build "./Pusula.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Pusula.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pusula.Api.dll"]
