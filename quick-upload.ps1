# Quick FTP Upload Script for SmarterASP
Write-Host "=== Joja Auto-Upload to SmarterASP ===" -ForegroundColor Green
Write-Host ""

# FTP Configuration
$ftpServer = "win8213.site4now.net"
$ftpUsername = "kordy7-001"
$ftpPassword = "kokololo2323"
Write-Host "FTP Server: ftp://$ftpServer" -ForegroundColor Yellow
Write-Host "Username: $ftpUsername" -ForegroundColor Yellow

# Paths
$localPath = "c:\Users\selko\.vscode\joja\Joja.Api\publish"
$remotePath = "ftp://$ftpServer/site1"

Write-Host ""
Write-Host "Uploading from: $localPath" -ForegroundColor Cyan
Write-Host "To: $remotePath" -ForegroundColor Cyan
Write-Host ""

# Create WebClient
$webclient = New-Object System.Net.WebClient
$webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)

# Upload files
$fileCount = 0
$successCount = 0
$errorCount = 0

Get-ChildItem -Path $localPath -Recurse -File | ForEach-Object {
    $fileCount++
    $relativePath = $_.FullName.Substring($localPath.Length + 1)
    $remoteFile = "$remotePath/$($relativePath.Replace('\', '/'))"
    
    Write-Host "[$fileCount] Uploading: $($_.Name)..." -NoNewline
    
    try {
        $uri = New-Object System.Uri($remoteFile)
        $webclient.UploadFile($uri, $_.FullName)
        Write-Host " OK" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host " ERROR: $_" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "=== Upload Complete ===" -ForegroundColor Green
Write-Host "Total Files: $fileCount" -ForegroundColor Cyan
Write-Host "Success: $successCount" -ForegroundColor Green
Write-Host "Errors: $errorCount" -ForegroundColor Red
Write-Host ""
Write-Host "Open site at: http://kordy7-001-site1.qtempurl.com" -ForegroundColor Cyan
Write-Host ""
