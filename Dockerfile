# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/SampleApi/SampleApi.csproj", "src/SampleApi/"]
RUN dotnet restore "src/SampleApi/SampleApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/SampleApi"
RUN dotnet build "SampleApi.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "SampleApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleApi.dll"]
