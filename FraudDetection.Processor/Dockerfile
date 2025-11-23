# Build Stage - Uses .NET 9 SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["FraudDetection.Worker/FraudDetection.Worker.csproj", "FraudDetection.Worker/"]
RUN dotnet restore "FraudDetection.Worker/FraudDetection.Worker.csproj"

# Copy the rest of the source code
COPY . .
WORKDIR "/src/FraudDetection.Worker"
RUN dotnet publish -c Release -o /app/publish

# Run Stage - Uses .NET 9 Runtime
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FraudDetection.Worker.dll"]