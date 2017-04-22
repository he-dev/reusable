#Get-Content config.sql | .\sqlite3.exe config.db

.\sqlite3 config.db ".read config.sql"

Write-Host
if ($lastexitcode -eq 0)
{
    Write-Host "*** Configuration successfuly updated ;-)" -foregroundcolor "green"
}
else
{
    Write-Host "*** Error updating configuration ;-(" -foregroundcolor "red"
}