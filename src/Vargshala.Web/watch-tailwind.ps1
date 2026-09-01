# Run Tailwind in Watch Mode for Live Reloading during UI development
Write-Host "🚀 Starting Tailwind CSS Live Watcher for Vargshala.Web..." -ForegroundColor Cyan
& "$PSScriptRoot\tailwindcss.exe" -i "$PSScriptRoot\Styles\app.css" -o "$PSScriptRoot\wwwroot\app.css" --watch
