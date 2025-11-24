# 1. Build Stage - USING .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the csproj file and restore
COPY ["FraudDetection.API/FraudDetection.API.csproj", "FraudDetection.API/"]
RUN dotnet restore "FraudDetection.API/FraudDetection.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/FraudDetection.API"
RUN dotnet publish -c Release -o /app/publish

# 2. Run Stage - USING .NET 9 RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "FraudDetection.API.dll"]