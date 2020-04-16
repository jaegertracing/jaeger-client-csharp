[CmdletBinding(PositionalBinding = $false)]
param(
    [string] $ArtifactsPath = (Join-Path $PWD "artifacts"),
    [string] $BuildConfiguration = "Release",

    [bool] $RunDeploy = $env:APPVEYOR_REPO_BRANCH -eq "master"
)

$ErrorActionPreference = "Stop"
$Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

function Task {
    [CmdletBinding()] param (
        [Parameter(Mandatory = $true)] [string] $name,
        [Parameter(Mandatory = $false)] [bool] $runTask,
        [Parameter(Mandatory = $false)] [scriptblock] $cmd
    )

    if ($cmd -eq $null) {
        throw "Command is missing for task '$name'. Make sure the starting '{' is on the same line as the term 'Task'. E.g. 'Task `"$name`" `$Run$name {'"
    }

    if ($runTask -eq $true) {
        Write-Host "`n------------------------- [$name] -------------------------`n" -ForegroundColor Cyan
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        & $cmd
        Write-Host "`nTask '$name' finished in $($sw.Elapsed.TotalSeconds) sec."
    }
    else {
        Write-Host "`n------------------ Skipping task '$name' ------------------" -ForegroundColor Yellow
    }
}

Task "Deploy" $RunDeploy {

	docker login -u "$env:DOCKER_USER" -p "$env:DOCKER_PASS"
	docker push jaegertracing/xdock-csharp

    if ($LASTEXITCODE -ne 0) { throw "Deploy docker failed." }
}

Write-Host "`nBuild finished in $($Stopwatch.Elapsed.TotalSeconds) sec." -ForegroundColor Green
