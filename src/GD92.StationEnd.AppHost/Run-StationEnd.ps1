$ErrorActionPreference = "Stop"

$appHost = Join-Path $PSScriptRoot "GD92.StationEnd.AppHost.csproj"
aspire run --apphost $appHost --detach --format Json --non-interactive | Out-Null

if ($LASTEXITCODE -ne 0) {
    throw "Aspire failed to start the StationEnd AppHost."
}

$deadline = [DateTimeOffset]::UtcNow.AddMinutes(2)
$nodeManagerStarted = $false
do {
    $description = aspire describe Node-Manager-UA `
        --apphost $appHost `
        --format Json `
        --non-interactive 2>$null

    if ($LASTEXITCODE -eq 0) {
        $resource = ($description | ConvertFrom-Json).resources |
            Select-Object -First 1
        $nodeManagerUrl = $resource.urls |
            Where-Object { $_.name -eq "https" } |
            Select-Object -ExpandProperty url -First 1
        $dashboardUrl = $resource.dashboardUrl

        if ($resource.state -eq "Running" -and $nodeManagerUrl -and $dashboardUrl) {
            Write-Host "NodeManager: $nodeManagerUrl"
            Write-Host "Aspire dashboard: $dashboardUrl"
            $nodeManagerStarted = $true
            break
        }
    }

    Start-Sleep -Seconds 1
} while ([DateTimeOffset]::UtcNow -lt $deadline)

if (-not $nodeManagerStarted) {
    throw "NodeManager did not bind an HTTPS endpoint within two minutes."
}

Write-Host "Press any key to stop StationEnd."
[Console]::ReadKey($true) | Out-Null

aspire stop --apphost $appHost --non-interactive

if ($LASTEXITCODE -ne 0) {
    throw "Aspire failed to stop the StationEnd AppHost."
}
