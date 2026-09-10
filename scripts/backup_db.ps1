# Vargshala Database Backup Script (Schema + Full Data)
# Runs pg_dump from PostgreSQL 18 installation with Neon connection

$ErrorActionPreference = "Stop"

$pgDumpPath = "C:\Program Files\PostgreSQL\18\bin\pg_dump.exe"
if (-not (Test-Path $pgDumpPath)) {
    # Fallback search
    $found = Get-ChildItem "C:\Program Files\PostgreSQL\*\bin\pg_dump.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) {
        $pgDumpPath = $found.FullName
    } else {
        Write-Error "pg_dump.exe not found under C:\Program Files\PostgreSQL\"
        exit 1
    }
}

$backupDir = Join-Path $PSScriptRoot "..\backups"
if (-not (Test-Path $backupDir)) {
    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
}

$timestamp = Get-Date -Format 'yyyyMMdd_HHmmss'
$backupFile = Join-Path $backupDir "vargshala_backup_$timestamp.sql"
$latestFile = Join-Path $backupDir "vargshala_latest_backup.sql"

$connString = "postgresql://neondb_owner:npg_2AxfCDt0jgJO@ep-wispy-sound-b3seblw4-pooler.c-4.ap-southeast-1.aws.neon.tech/Vargshala?sslmode=require&hostaddr=52.76.212.156"

Write-Host "Starting Vargshala database backup..." -ForegroundColor Cyan

& $pgDumpPath `
    --dbname=$connString `
    --clean `
    --if-exists `
    --no-owner `
    --no-privileges `
    --column-inserts `
    --file=$backupFile

if ($LASTEXITCODE -eq 0 -and (Test-Path $backupFile)) {
    # Post-process: comment out psql-only meta-commands (\restrict, \unrestrict)
    # so the script can execute cleanly in pgAdmin Query Tool, DBeaver, etc.
    $rawContent = Get-Content $backupFile
    $sanitized = $rawContent | ForEach-Object {
        if ($_ -match '^\\[a-zA-Z]') {
            "-- $_  (commented out for pgAdmin/GUI SQL editor compatibility)"
        } else {
            $_
        }
    }
    Set-Content -Path $backupFile -Value $sanitized -Encoding UTF8

    Copy-Item $backupFile -Destination $latestFile -Force
    $fileInfo = Get-Item $backupFile
    Write-Host "Backup completed successfully!" -ForegroundColor Green
    Write-Host "File: $($fileInfo.FullName)" -ForegroundColor Yellow
    Write-Host "Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Yellow
} else {
    Write-Error "Backup failed with exit code $LASTEXITCODE"
}
