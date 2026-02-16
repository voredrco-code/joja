# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Joja.Api/Joja.Api.csproj", "Joja.Api/"]
RUN dotnet restore "Joja.Api/Joja.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/Joja.Api"
RUN dotnet build "Joja.Api.csproj" -c Release --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish "Joja.Api.csproj" -c Release --no-build -o /app/publish

# 2. Final Stage (Production)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# (مهم جداً) تنزيل مكاتب Globalization و SQLite عشان تشتغل على Alpine
RUN apk add --no-cache icu-libs sqlite-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Copy the published app
COPY --from=publish /app/publish .

# (الحل السحري) إنشاء فولدر للداتابيز وإعطاء صلاحيات الكتابة لليوزر
RUN mkdir -p /app/data && \
    addgroup -g 1000 appuser && \
    adduser -u 1000 -G appuser -s /bin/sh -D appuser && \
    chown -R appuser:appuser /app && \
    chmod -R 777 /app/data

# فتح البوت
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# تبديل لليوزر العادي للأمان
USER appuser

# تشغيل التطبيق
ENTRYPOINT ["dotnet", "Joja.Api.dll"]
