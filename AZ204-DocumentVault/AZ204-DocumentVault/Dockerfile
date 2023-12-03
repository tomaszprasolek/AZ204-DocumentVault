FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["AZ204-DocumentVault/AZ204-DocumentVault.csproj", "AZ204-DocumentVault/"]
RUN dotnet restore "AZ204-DocumentVault/AZ204-DocumentVault.csproj"
COPY . .
WORKDIR "/src/AZ204-DocumentVault"
RUN dotnet build "AZ204-DocumentVault.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AZ204-DocumentVault.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AZ204-DocumentVault.dll"]
