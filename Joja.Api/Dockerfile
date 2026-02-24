# Stage 1: Build
FROM dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["Joja.Api.csproj", "./"]
RUN dotnet restore "./Joja.Api.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "Joja.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "Joja.Api.csproj" -c Release -o /app/publish

# Stage 3: Final Runtime
FROM dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Joja.Api.dll"]
