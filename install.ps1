# Skrypt instalacyjny dla Windows (PowerShell)
$Repo = "Xeno178531/ccommit"
$BinaryName = "ccommit.exe"
$InstallDir = "$env:ProgramFiles\ccommit"

Write-Host "Pobieranie najnowszej wersji $BinaryName..." -ForegroundColor Cyan

# Pobranie bezpośredniego linku do najnowszego wydań (Releases) z GitHub API
$ReleaseUrl = "https://api.github.com/repos/$Repo/releases/latest"
$DownloadUrl = (Invoke-RestMethod -Uri $ReleaseUrl).assets | Where-Object { $_.name -eq $BinaryName } | Select-Object -ExpandProperty browser_download_url

if (-not $DownloadUrl) {
    Write-Error "Nie znaleziono pliku $BinaryName w najnowszym wydaniu GitHub!"
    exit 1
}

# Utworzenie katalogu docelowego w Program Files
if (-not (Test-Path $InstallDir)) {
    New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
}

$Destination = "$InstallDir\$BinaryName"
Write-Host "Pobieranie pliku..."
Invoke-WebRequest -Uri $DownloadUrl -OutFile $Destination

# Dodanie katalogu do zmiennej środowiskowej PATH (dla całej maszyny)
$CurrentPath = [Environment]::GetEnvironmentVariable("Path", [EnvironmentVariableTarget]::Machine)
if ($CurrentPath -notlike "*$InstallDir*") {
    Write-Host "Dodawanie katalogu do systemowej zmiennej PATH..." -ForegroundColor Yellow
    [Environment]::SetEnvironmentVariable("Path", "$CurrentPath;$InstallDir", [EnvironmentVariableTarget]::Machine)
}

Write-Host "Gotowe! Uruchom ponownie terminal i wpisz: MojaKomenda" -ForegroundColor Green