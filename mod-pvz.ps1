# Prompt the user to enter the destination folder path
$destination = Read-Host "Enter the destination folder path"

# Build the ZIP file URL
$url = "https://github.com/CG8516/PvZA11y/releases/download/beta.1.15.4/PvZA11y_Beta1.15.4.zip"

# Download and extract the ZIP
Invoke-WebRequest -Uri $url -OutFile "$destination\PvZA11y_.zip"
Expand-Archive -Path "$destination\PvZA11y_.zip" -DestinationPath $destination -Force

# Display a message indicating that the mod has been updated
Write-Host "The mod has been updated successfully!"
