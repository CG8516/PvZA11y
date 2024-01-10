# Verificar se está executando como administrador
if (-not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Running as a non-administrator. Requesting elevation..."

    # Solicitar elevação para administrador
    Start-Process powershell -ArgumentList "-NoProfile -ExecutionPolicy Bypass -File `"$($MyInvocation.MyCommand.Path)`"" -Verb RunAs
    exit
}

# Fetch the releases API URL
$releasesUrl = "https://api.github.com/repos/CG8516/PvZA11y/releases"
$releasesInfo = Invoke-RestMethod -Uri $releasesUrl

# Get the first browser_download_url from the assets
$browserDownloadUrl = $releasesInfo[0].assets[0].browser_download_url

# Prompt the user to enter the destination folder path
$destination = Read-Host "Enter the destination folder path"

# Create the full path for the downloaded file
$fileDownloadPath = Join-Path $destination "PvZA11y_.zip"

# Download the file directly
Invoke-WebRequest -Uri $browserDownloadUrl -OutFile $fileDownloadPath

# Check if the file was downloaded successfully
if (Test-Path $fileDownloadPath) {
    # Extract the ZIP file
    Expand-Archive -Path $fileDownloadPath -DestinationPath $destination -Force

    # Display a message indicating that the mod has been updated
    Write-Host "The mod has been updated successfully!"

    # Pause and prompt user to press Enter to exit
    Write-Host "Press Enter to exit..."
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
} else {
    Write-Host "Failed to download the file. Please check your internet connection and try again."

    # Pause and prompt user to press Enter to exit
    Write-Host "Press Enter to exit..."
    $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
}
