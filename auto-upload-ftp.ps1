# 🚀 Script تلقائي للرفع على SmarterASP

# الخطوة 1: خذ FTP Credentials من Control Panel
Write-Host "=== Joja Auto-Upload to SmarterASP ===" -ForegroundColor Green
Write-Host ""

# املأ هذه البيانات من SmarterASP Control Panel:
$ftpServer = Read-Host "FTP Server (Default: ftp://ftp.jojaskincare.com)"
if ([string]::IsNullOrWhiteSpace($ftpServer)) { $ftpServer = "ftp://ftp.jojaskincare.com" }

$ftpUsername = Read-Host "Username (من لوحة تحكم الاستضافة)"
$ftpPassword = Read-Host "Password" -AsSecureString
$ftpPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($ftpPassword))

# المسارات
$localPath = "c:\Users\selko\.vscode\joja\Joja.Api\publish"
$remotePath = "/" # Root directory for main domain

Write-Host "جاري الرفع من: $localPath" -ForegroundColor Yellow
Write-Host "إلى: $remotePath" -ForegroundColor Yellow
Write-Host ""

# إنشاء FTP request
$webclient = New-Object System.Net.WebClient
$webclient.Credentials = New-Object System.Net.NetworkCredential($ftpUsername, $ftpPassword)

# رفع الملفات
$fileCount = 0
Get-ChildItem -Path $localPath -Recurse -File | ForEach-Object {
    $fileCount++
    $relativePath = $_.FullName.Substring($localPath.Length + 1)
    $remoteFile = "$remotePath/$($relativePath.Replace('\', '/'))"
    
    Write-Host "[$fileCount] Uploading: $($_.Name)..." -NoNewline
    
    try {
        $uri = New-Object System.Uri($remoteFile)
        $webclient.UploadFile($uri, $_.FullName)
        Write-Host " ✓" -ForegroundColor Green
    }
    catch {
        Write-Host " ✗ Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Done! ===" -ForegroundColor Green
Write-Host "افتح: http://kordy7-001.site" -ForegroundColor Cyan
