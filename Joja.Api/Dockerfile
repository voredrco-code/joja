# Use the official ASP.NET Core SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["Joja.Api/Joja.Api.csproj", "Joja.Api/"]
RUN dotnet restore "Joja.Api/Joja.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Joja.Api"
RUN dotnet build "Joja.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Joja.Api.csproj" -c Release -o /app/publish

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Joja.Api.dll"]
