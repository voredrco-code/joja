# Safe FTP Upload Script for SmarterASP (Handles Locked Files)
Write-Host "=== Joja Safe Upload to SmarterASP ===" -ForegroundColor Green

# FTP Configuration
$ftpServer = "win8213.site4now.net"
$ftpUsername = "kordy7-001"
$ftpPassword = "kokololo2323"
$baseUri = "ftp://$ftpServer/site1"

# Paths
$localPath = "c:\Users\selko\.vscode\joja\Joja.Api\publish"

# Create WebClient
$webclient = New-Object System.Net.WebClient
$webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)

# 1. Upload app_offline.htm to stop the site
Write-Host "1. Taking site offline..." -ForegroundColor Yellow
$offlineContent = "<html><body><h1>Site is updating...</h1></body></html>"
$offlineFile = "app_offline.htm"
Set-Content $offlineFile $offlineContent
try {
    $uri = New-Object System.Uri("$baseUri/$offlineFile")
    $webclient.UploadFile($uri, (Get-Item $offlineFile).FullName)
    Write-Host "Site is offline. Waiting 10 seconds for locks to release..." -ForegroundColor Green
    Start-Sleep -Seconds 10
}
catch {
    Write-Host "Failed to upload app_offline.htm: $_" -ForegroundColor Red
    # Continue anyway, maybe it's not locked
}

# 2. Upload Files
Write-Host "2. Uploading files..." -ForegroundColor Yellow
$fileCount = 0
$successCount = 0
$errorCount = 0

Get-ChildItem -Path $localPath -Recurse -File | ForEach-Object {
    $fileCount++
    $relativePath = $_.FullName.Substring($localPath.Length + 1)
    $remoteFile = "$baseUri/$($relativePath.Replace('\', '/'))"
    
    Write-Host "[$fileCount] Uploading: $($_.Name)..." -NoNewline
    
    $retry = 0
    $uploaded = $false
    while (-not $uploaded -and $retry -lt 3) {
        try {
            $uri = New-Object System.Uri($remoteFile)
            $webclient.UploadFile($uri, $_.FullName)
            Write-Host " OK" -ForegroundColor Green
            $successCount++
            $uploaded = $true
        }
        catch {
            $retry++
            Write-Host " RETRY $retry..." -NoNewline -ForegroundColor Yellow
            Start-Sleep -Seconds 2
        }
    }
    
    if (-not $uploaded) {
        Write-Host " FAILED" -ForegroundColor Red
        $errorCount++
    }
}

# 3. Remove app_offline.htm to start the site
Write-Host "3. Bringing site back online..." -ForegroundColor Yellow
try {
    $request = [System.Net.WebRequest]::Create("$baseUri/$offlineFile")
    $request.Method = [System.Net.WebRequestMethods+Ftp]::DeleteFile
    $request.Credentials = $webclient.Credentials
    $response = $request.GetResponse()
    Write-Host "Site is online!" -ForegroundColor Green
}
catch {
    Write-Host "Failed to delete app_offline.htm: $_" -ForegroundColor Red
    Write-Host "You may need to delete it manually via File Manager." -ForegroundColor Yellow
}

# Cleanup local file
Remove-Item $offlineFile

Write-Host ""
Write-Host "=== Upload Complete ===" -ForegroundColor Green
Write-Host "Total: $fileCount | Success: $successCount | Errors: $errorCount"
