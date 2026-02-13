# Build stage - compile and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Joja.Api/Joja.Api.csproj", "Joja.Api/"]
RUN dotnet restore "Joja.Api/Joja.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/Joja.Api"
RUN dotnet build "Joja.Api.csproj" -c Release --no-restore

# Publish stage - package the application
FROM build AS publish
RUN dotnet publish "Joja.Api.csproj" -c Release --no-build -o /app/publish

# Runtime stage - create production image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Set environment to production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Copy published application
COPY --from=publish /app/publish .

# Expose port for Render.com
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD dotnet Joja.Api.dll --version || exit 1

# Set user to non-root for security
RUN addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app

USER appuser

# Run the application
ENTRYPOINT ["dotnet", "Joja.Api.dll"]
