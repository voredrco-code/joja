
# FTP Upload Script for Joja.Api.dll ONLY
Write-Host "=== Uploading Joja.Api.dll ===" -ForegroundColor Green

# Configuration
$ftpServer = "ftp://208.98.35.213"
$ftpUsername = "kordy7-001"
$ftpPassword = "kokololo2323"
$localPath = "c:\Users\selko\.vscode\joja\Joja.Api\publish\Joja.Api.dll"
$remotePath = "joja/Joja.Api.dll"

Write-Host "Source: $localPath"
Write-Host "Target: $ftpServer/$remotePath"

try {
    $ftpRequest = [System.Net.FtpWebRequest]::Create("$ftpServer/$remotePath")
    $ftpRequest.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)
    $ftpRequest.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
    $ftpRequest.UseBinary = $true
    $ftpRequest.UsePassive = $true
    $ftpRequest.KeepAlive = $false
    $ftpRequest.Timeout = 60000 # 60 seconds timeout
    
    $fileBytes = [System.IO.File]::ReadAllBytes($localPath)
    $ftpRequest.ContentLength = $fileBytes.Length
    
    $requestStream = $ftpRequest.GetRequestStream()
    $requestStream.Write($fileBytes, 0, $fileBytes.Length)
    $requestStream.Close()
    $requestStream.Dispose()
    
    $response = $ftpRequest.GetResponse()
    $response.Close()
    
    Write-Host "SUCCESS: Joja.Api.dll uploaded!" -ForegroundColor Green
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.InnerException) {
        Write-Host "Inner: $($_.Exception.InnerException.Message)" -ForegroundColor Red
    }
}
