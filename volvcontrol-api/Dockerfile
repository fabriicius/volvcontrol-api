#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["volvcontrol-api/volvcontrol-api.csproj", "volvcontrol-api/"]
RUN dotnet restore "volvcontrol-api/volvcontrol-api.csproj"
COPY . .
WORKDIR "/src/volvcontrol-api"
RUN dotnet build "volvcontrol-api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "volvcontrol-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Firebase credentials inside container
RUN mkdir -p /app/token
COPY ["volvcontrol-api.service/FirebaseCredentials/volvcontrol-ebe86-b99e62524b19.json", "/app/token/volvcontrol-ebe86-b99e62524b19.json"]
ENV Firebase__CredentialsPath=/app/token/volvcontrol-ebe86-b99e62524b19.json

ENTRYPOINT ["dotnet", "volvcontrol-api.dll"]