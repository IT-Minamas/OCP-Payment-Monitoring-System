param(
    [string]$TaskName = "OCP CFO Email Alert",
    [string]$StartTime = "10:00AM"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$exePath = Join-Path $projectRoot "publish\OCP.CFOEmailAlert.exe"

if (-not (Test-Path $exePath)) {
    throw "Published executable not found: $exePath. Run publish first."
}

$action = New-ScheduledTaskAction -Execute $exePath -WorkingDirectory (Split-Path $exePath)
$trigger = New-ScheduledTaskTrigger -Weekly -DaysOfWeek Monday -At $StartTime
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -ExecutionTimeLimit (New-TimeSpan -Minutes 10)

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Settings $settings -Description "Sends weekly OCP memo progress alert for CFO approval." -Force
Write-Host "Task '$TaskName' registered. Configure its Security Options in Task Scheduler to run under the approved service account, including 'Run whether user is logged on or not'."
