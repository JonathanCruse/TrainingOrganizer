# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY TrainingOrganizer.slnx ./
COPY src/TrainingOrganizer.Domain/TrainingOrganizer.Domain.csproj src/TrainingOrganizer.Domain/
COPY src/TrainingOrganizer.Application/TrainingOrganizer.Application.csproj src/TrainingOrganizer.Application/
COPY src/TrainingOrganizer.Infrastructure/TrainingOrganizer.Infrastructure.csproj src/TrainingOrganizer.Infrastructure/
COPY src/TrainingOrganizer.Api/TrainingOrganizer.Api.csproj src/TrainingOrganizer.Api/

# Restore only the Api project (pulls all transitive dependencies)
RUN dotnet restore src/TrainingOrganizer.Api/TrainingOrganizer.Api.csproj

# Copy all source code
COPY . .

# Publish Api project in Release mode
RUN dotnet publish src/TrainingOrganizer.Api/TrainingOrganizer.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd --gid 1000 appuser \
    && useradd --uid 1000 --gid 1000 --shell /bin/bash --create-home appuser

# Install curl for healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published output
COPY --from=build /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Switch to non-root user
USER appuser

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TrainingOrganizer.Api.dll"]
