# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY NexusBakeryTech.slnx ./
COPY src/NexusBakery.Domain/NexusBakery.Domain.csproj src/NexusBakery.Domain/
COPY src/NexusBakery.Application/NexusBakery.Application.csproj src/NexusBakery.Application/
COPY src/NexusBakery.Infrastructure/NexusBakery.Infrastructure.csproj src/NexusBakery.Infrastructure/
COPY src/NexusBakery.Agents/NexusBakery.Agents.csproj src/NexusBakery.Agents/
COPY src/NexusBakery.Api/NexusBakery.Api.csproj src/NexusBakery.Api/
COPY tests/NexusBakery.UnitTests/NexusBakery.UnitTests.csproj tests/NexusBakery.UnitTests/

# Restore dependencies
RUN dotnet restore src/NexusBakery.Api/NexusBakery.Api.csproj

# Copy all source files
COPY src/ src/

# Publish the API in Release mode
WORKDIR /src/src/NexusBakery.Api
RUN dotnet publish NexusBakery.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render exposes the PORT environment variable (default 10000)
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "NexusBakery.Api.dll"]
