# Joja API - Render.com Deployment Guide

## Overview
This guide explains how to deploy the Joja API to Render.com as a production web service.

## Prerequisites
- GitHub account with the jojo repository
- Render.com account (free tier available)
- Docker knowledge (Render.com handles Docker builds)

## Deployment Steps

### 1. Push to GitHub
Ensure your code is pushed to your GitHub repository:
```bash
git add .
git commit -m "Prepare for Render.com deployment"
git push origin main
```

### 2. Connect Render.com to GitHub
1. Log in to [Render.com](https://render.com)
2. Click "New +" → "Web Service"
3. Select "Deploy an existing repository"
4. Connect your GitHub account if not already connected
5. Select the `jojo` repository

### 3. Configure Web Service
Render.com will auto-detect the `render.yaml` file. Verify these settings:

**Build Settings:**
- **Build Command:** (leave empty - Docker will handle it)
- **Dockerfile Path:** `./Joja.Api/Dockerfile`
- **Docker Context:** `.` (root directory)

**Environment:**
- `ASPNETCORE_ENVIRONMENT`: `Production` (auto-set in render.yaml)
- Add any additional secrets in Render.com dashboard:
  - Database connection string (if using external DB)
  - API keys, credentials, etc.

**Instance:**
- **Region:** Choose based on your users' location
- **Plan:** Start with Free tier or Starter tier

### 4. Database Setup - IMPORTANT

#### Option A: SQLite with Persistent Disk (Recommended for small apps)
1. Uncomment the disk configuration in `render.yaml`:
   ```yaml
   diskSize: 1
   diskMountPath: /app/data
   ```
2. Render.com will create a persistent 1GB disk mounted at `/app/data`
3. The database file will persist across deployments

#### Option B: PostgreSQL (Recommended for production)
1. Create a PostgreSQL database on Render.com:
   - Dashboard → Databases → New Database → PostgreSQL
2. Update connection string in appsettings.Production.json or set as environment variable:
   ```
   DefaultConnection=Host=your-db.render.com;Port=5432;Database=joja;Username=username;Password=password;
   ```
3. Add NuGet package for PostgreSQL:
   ```bash
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```
4. Update Program.cs to use PostgreSQL:
   ```csharp
   options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
   ```

#### Option C: External Database (AWS RDS, Azure, etc.)
Set the connection string as an environment variable in Render.com dashboard.

### 5. Deploy
1. Click "Create Web Service"
2. Render.com will:
   - Build the Docker image
   - Run the container
   - Start your service
3. Your app will be available at: `https://joja-api-xxx.render.com`

### 6. Post-Deployment Checks
- Test the health endpoint: `https://joja-api-xxx.render.com/`
- Check logs in Render.com dashboard for any errors
- Verify database connectivity if applicable

## Troubleshooting

### Service won't start
- Check build logs in Render.com dashboard
- Verify Docker builds locally: `docker build -t joja-api -f Joja.Api/Dockerfile .`

### Database not persisting
- Ensure persistent disk is enabled if using SQLite
- Check database path in appsettings.Production.json

### Port binding errors
- Verify port 5000 is exposed in Dockerfile (already set)
- Check environment variable handling in Program.cs

### Health check failing
- Ensure the app responds at root path (/)
- Verify port is correct (5000)

## Local Testing

Before deploying, test the Docker build locally:
```bash
# Build the image
docker build -t joja-api -f Joja.Api/Dockerfile .

# Run the container
docker run -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  joja-api

# Test the service
curl http://localhost:5000/
```

## Monitoring & Logs
- Access logs in Render.com dashboard under "Logs"
- Set up email alerts for deployment events
- Monitor resource usage (CPU, Memory, Disk)

## Updates & Redeployment
- Any push to the main branch will auto-trigger a new deployment
- Current deployment will be replaced automatically
- No manual steps needed if using `autoDeploy: true` in render.yaml

## Support Resources
- [Render.com Documentation](https://render.com/docs)
- [.NET on Docker](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Entity Framework Core - Database Providers](https://learn.microsoft.com/en-us/ef/core/providers/)
