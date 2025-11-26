# GitHub Release Upload Script for XyaRat v1.0.8
param([string]$GithubToken = $env:GITHUB_TOKEN)

$owner = "wsxyanua"
$repo = "XyaRat"
$tag = "v1.0.8"
$releaseName = "XyaRat v1.0.8 - VS2022 Compatible Build"

$releaseBody = @"
# XyaRat v1.0.8 - Visual Studio 2022 Compatible Release

## What's New
- Full Visual Studio 2022 build compatibility
- Upgraded all projects to .NET Framework 4.8
- Fixed 39 files with build issues
- Enhanced Logger system with unified API
- 18 functional plugin DLLs included

## Release Contents
- **XyaRat.exe** (12.3 MB) - Server/Control Panel
- **Client.exe** (446 KB) - Client Stub/Payload
- **Plugins.zip** - 18 plugin DLLs

## Requirements
- Windows 7+ (x64)
- .NET Framework 4.8 Runtime

## WARNING - Educational Use Only
This is a Remote Access Tool for educational purposes only.
- Antivirus WILL detect these files
- DO NOT use on systems you don't own
- Test ONLY in isolated environments

Full details: https://github.com/wsxyanua/XyaRat/blob/main/RELEASE_NOTES_v1.0.8.md
"@

Write-Host "`n=== XyaRat v1.0.8 Release Upload ===" -ForegroundColor Cyan

if ([string]::IsNullOrEmpty($GithubToken)) {
    Write-Host "ERROR: GitHub token not found!" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "token $GithubToken"
    "Accept" = "application/vnd.github.v3+json"
}

Write-Host "Creating release..." -ForegroundColor Yellow
$releaseData = @{
    tag_name = $tag
    name = $releaseName
    body = $releaseBody
    draft = $false
    prerelease = $false
} | ConvertTo-Json

try {
    $release = Invoke-RestMethod -Uri "https://api.github.com/repos/$owner/$repo/releases" -Method Post -Headers $headers -Body $releaseData -ContentType "application/json"
    Write-Host "Release created! ID: $($release.id)" -ForegroundColor Green
    
    Write-Host "Creating Plugins.zip..." -ForegroundColor Yellow
    $pluginsZip = "d:\XyaRat\Plugins.zip"
    if (Test-Path $pluginsZip) { Remove-Item $pluginsZip -Force }
    Compress-Archive -Path "d:\XyaRat\ReleasePackage\Plugins\*" -DestinationPath $pluginsZip
    
    $files = @(
        @{Path="d:\XyaRat\ReleasePackage\XyaRat.exe"; Name="XyaRat.exe"},
        @{Path="d:\XyaRat\ReleasePackage\Client.exe"; Name="Client.exe"},
        @{Path="d:\XyaRat\ReleasePackage\README.txt"; Name="README.txt"},
        @{Path=$pluginsZip; Name="Plugins.zip"}
    )
    
    foreach ($file in $files) {
        if (Test-Path $file.Path) {
            $sizeMB = [math]::Round((Get-Item $file.Path).Length / 1MB, 2)
            Write-Host "Uploading $($file.Name) ($sizeMB MB)..." -ForegroundColor Cyan
            
            $uploadUrl = $release.upload_url -replace '\{\?name,label\}', "?name=$($file.Name)"
            $uploadHeaders = @{
                "Authorization" = "token $GithubToken"
                "Content-Type" = "application/octet-stream"
            }
            
            $fileBytes = [System.IO.File]::ReadAllBytes($file.Path)
            Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $uploadHeaders -Body $fileBytes | Out-Null
            Write-Host "  SUCCESS!" -ForegroundColor Green
        }
    }
    
    Write-Host "`nRELEASE PUBLISHED!" -ForegroundColor Green
    Write-Host $release.html_url -ForegroundColor Cyan
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
