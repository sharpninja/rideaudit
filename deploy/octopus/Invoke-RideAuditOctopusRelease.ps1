# From PAYTON-LEGION2: ensure RideAudit Octopus project, sync tree, release, deploy.
# FR-RIDE-063 / UC-RIDE-032 / TR-RIDE-DEPLOY-001 / TR-RIDE-DEPLOY-002 / TEST-RIDE-038.
# No GHCR. Secrets stay in ~/.creds. SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$ProjectName = "RideAudit",
    [string]$EnvironmentName = "Development",
    [string]$TargetName = "PAYTON-DESKTOP-LINUX",
    [string]$TargetRole = "rideaudit-host",
    [string]$SshHost = "PAYTON-DESKTOP",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [string]$SshConnectHost = "172.19.0.1",
    [string]$AdmissionUrl = "http://192.168.0.149:28080/",
    [string]$ApiBase = "http://192.168.0.149:8066",
    [string]$Version = "",
    [switch]$SkipSync,
    [switch]$WithPublish
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OctopusApi.ps1")
. (Join-Path $here "..\omarchy\OmarchySsh.ps1")

$scriptBody = @"
set -eu
ROOT="$RemoteAbs"
test -f "`$ROOT/deploy/octopus/remote-build-and-run.sh"
/usr/bin/bash --noprofile --norc "`$ROOT/deploy/octopus/remote-build-and-run.sh"
"@

function Get-OctopusItems {
    param($Connection, [string]$Path)
    $result = Get-RideAuditOctopusJson -Connection $Connection -Path $Path
    if ($null -eq $result) { return @() }
    if ($result -is [System.Array]) { return @($result) }
    if ($result.PSObject.Properties.Name -contains "Items") { return @($result.Items) }
    return @($result)
}

function Wait-OctopusTask {
    param($Connection, [string]$TaskId, [int]$TimeoutSec = 3600)
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    do {
        $task = Get-RideAuditOctopusJson -Connection $Connection -Path "/api/tasks/$TaskId"
        Write-Host ("task {0} state={1} desc={2}" -f $task.Id, $task.State, $task.Description)
        if ($task.State -in @("Success", "Failed", "Canceled", "TimedOut")) {
            return $task
        }
        Start-Sleep -Seconds 5
    } while ((Get-Date) -lt $deadline)
    throw "Octopus task $TaskId timed out after $TimeoutSec seconds"
}

function Ensure-SshAccount {
    param($Connection)
    $accounts = Get-OctopusItems -Connection $Connection -Path "/api/Spaces-1/accounts/all"
    if ($accounts.Count -eq 0) {
        $accounts = Get-OctopusItems -Connection $Connection -Path "/api/accounts/all"
    }
    $existing = $accounts | Where-Object { $_.Name -eq "RideAudit PAYTON-DESKTOP SSH" } | Select-Object -First 1
    if ($existing) {
        Write-Host ("using account {0}" -f $existing.Id)
        return $existing
    }

    $keyPath = Join-Path $env:USERPROFILE ".ssh\id_ed25519_payton_desktop"
    $yamlPath = Join-Path $env:USERPROFILE ".creds\payton-omarchy.yaml"
    $body = $null
    if (Test-Path -LiteralPath $keyPath) {
        $pem = [IO.File]::ReadAllText($keyPath)
        $body = @{
            AccountType                   = "SshKeyPair"
            Name                          = "RideAudit PAYTON-DESKTOP SSH"
            Description                   = "FR-RIDE-063 Linux Docker host SSH. Created from LEGION2 key file, not git."
            Username                      = "sharpninja"
            PrivateKeyFile                = @{ HasValue = $true; NewValue = $pem }
            TenantedDeploymentParticipation = "Untenanted"
        }
        Write-Host "creating SSH key-pair account from local key file (not printed)"
    }
    elseif (Test-Path -LiteralPath $yamlPath) {
        $yaml = Get-RideAuditYamlMap -Path $yamlPath
        $body = @{
            AccountType                   = "UsernamePassword"
            Name                          = "RideAudit PAYTON-DESKTOP SSH"
            Description                   = "FR-RIDE-063 Linux Docker host SSH. Password from ~/.creds, not git."
            Username                      = $yaml.username
            Password                      = @{ HasValue = $true; NewValue = $yaml.password }
            TenantedDeploymentParticipation = "Untenanted"
        }
        Write-Host "creating username/password account from cred yaml (not printed)"
    }
    else {
        throw "no SSH key or payton-omarchy.yaml available to create an Octopus account"
    }

    $created = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/accounts" -Body $body
    Write-Host ("created account {0}" -f $created.Id)
    return $created
}

function Ensure-LinuxTarget {
    param($Connection, $Account, [string]$Fingerprint)
    $machines = Get-OctopusItems -Connection $Connection -Path "/api/machines/all"
    $existing = $machines | Where-Object { $_.Name -eq $TargetName } | Select-Object -First 1
    if ($existing) {
        Write-Host ("using machine {0} health={1}" -f $existing.Id, $existing.HealthStatus)
        return $existing
    }

    $body = @{
        Name                            = $TargetName
        IsDisabled                      = $false
        HealthStatus                    = "Unknown"
        IsInProcess                     = $true
        Endpoint                        = @{
            CommunicationStyle = "Ssh"
            Uri                = "ssh://${SshConnectHost}:22/"
            Host               = $SshConnectHost
            Port               = 22
            Fingerprint        = $Fingerprint
            DotNetCorePlatform = "linux-x64"
            HostKeyAlgorithm   = "ssh-ed25519"
            AccountId          = $Account.Id
        }
        TenantedDeploymentParticipation = "Untenanted"
        EnvironmentIds                  = @("Environments-1", "Environments-3")
        Roles                           = @($TargetRole)
        TenantIds                       = @()
    }
    $created = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/machines" -Body $body
    Write-Host ("created machine {0}" -f $created.Id)
    return $created
}

function Start-MachineHealth {
    param($Connection, [string]$MachineId)
    $body = @{
        Name        = "Health"
        Description = "Health check $TargetName for FR-RIDE-063"
        SpaceId     = "Spaces-1"
        Arguments   = @{
            MachineIds        = @($MachineId)
            Timeout           = "00:05:00"
            MachineTimeout    = "00:02:00"
            OnlyTestConnection = $false
        }
    }
    try {
        return Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/tasks" -Body $body
    }
    catch {
        Write-Host ("health task create failed: {0}" -f $_.Exception.Message)
        return $null
    }
}

function Ensure-ProjectAndProcess {
    param($Connection)
    $projects = Get-OctopusItems -Connection $Connection -Path "/api/projects/all"
    $project = $projects | Where-Object { $_.Name -eq $ProjectName } | Select-Object -First 1
    if (-not $project) {
        $body = @{
            Name           = $ProjectName
            Description    = "FR-RIDE-063: Octopus builds RideAudit containers and deploys them to PAYTON-DESKTOP. No GHCR."
            ProjectGroupId = "ProjectGroups-1"
            LifecycleId    = "Lifecycles-1"
        }
        try {
            $project = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/projects" -Body $body
            Write-Host ("created project {0}" -f $project.Id)
        }
        catch {
            $msg = $_.Exception.Message
            if ($msg -match "license|limit|quota") {
                throw "DEFAULT_OCTOPUS_LICENSE_BLOCKED: $msg"
            }
            throw
        }
    }
    else {
        Write-Host ("using project {0}" -f $project.Id)
    }

    $process = Get-RideAuditOctopusJson -Connection $Connection -Path "/api/Spaces-1/deploymentprocesses/$($project.DeploymentProcessId)"
    $stepName = "Build and run RideAudit containers"
    $step = @{
        Name               = $stepName
        Type               = "Step"
        PackageRequirement = "LetOctopusDecide"
        Properties         = @{ "Octopus.Action.TargetRoles" = $TargetRole }
        Condition          = "Success"
        StartTrigger       = "StartAfterPrevious"
        Actions            = @(
            @{
                ActionType                   = "Octopus.Script"
                Name                         = $stepName
                IsDisabled                   = $false
                IsRequired                   = $true
                CanBeUsedForProjectVersioning = $false
                WorkerPoolId                 = $null
                Container                    = @{ Image = $null; FeedId = $null }
                Environments                 = @()
                ExcludedEnvironments         = @()
                Channels                     = @()
                TenantTags                   = @()
                Packages                     = @()
                Condition                    = "Success"
                Properties                   = @{
                    "Octopus.Action.RunOnServer"        = "false"
                    "Octopus.Action.EnabledFeatures"    = ""
                    "Octopus.Action.Script.ScriptSource" = "Inline"
                    "Octopus.Action.Script.Syntax"      = "Bash"
                    "Octopus.Action.Script.ScriptBody"  = $scriptBody
                }
            }
        )
    }
    $process.Steps = @($step)
    $updated = Invoke-RideAuditOctopusApi -Connection $Connection -Method PUT -Path "/api/Spaces-1/deploymentprocesses/$($process.Id)" -Body $process
    Write-Host ("updated process {0} version={1}" -f $updated.Id, $updated.Version)
    return $project
}

function New-RideAuditReleaseAndDeploy {
    param($Connection, $Project, [string]$ReleaseVersion)
    $channels = Get-OctopusItems -Connection $Connection -Path "/api/Spaces-1/projects/$($Project.Id)/channels"
    $channel = $channels | Select-Object -First 1
    if (-not $channel) { throw "project $($Project.Id) has no channel" }
    $releaseBody = @{
        ProjectId     = $Project.Id
        ChannelId     = $channel.Id
        Version       = $ReleaseVersion
        ReleaseNotes  = "FR-RIDE-063 RideAudit containers to PAYTON-DESKTOP. Local Docker only. No GHCR."
    }
    $release = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/releases" -Body $releaseBody
    Write-Host ("created release {0} version={1}" -f $release.Id, $release.Version)

    $envs = Get-OctopusItems -Connection $Connection -Path "/api/environments/all"
    $env = $envs | Where-Object { $_.Name -eq $EnvironmentName } | Select-Object -First 1
    if (-not $env) { throw "environment $EnvironmentName not found" }

    $deployBody = @{
        ReleaseId     = $release.Id
        EnvironmentId = $env.Id
        Comments      = "FR-RIDE-063 / UC-RIDE-032"
    }
    $deployment = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/deployments" -Body $deployBody
    Write-Host ("created deployment {0} task={1}" -f $deployment.Id, $deployment.TaskId)
    $task = Wait-OctopusTask -Connection $Connection -TaskId $deployment.TaskId -TimeoutSec 5400
    return [pscustomobject]@{
        Release    = $release
        Deployment = $deployment
        Task       = $task
        Channel    = $channel
        Environment = $env
    }
}

Write-Host "FR-RIDE-063 RideAudit Octopus release from PAYTON-LEGION2. No GHCR."
$cx = Get-RideAuditOctopusConnection -ApiBase $ApiBase
Write-Host ("octopus api={0} portal={1} stored-url={2}" -f $cx.ApiBase, $cx.PortalBase, $cx.ServerUrl)

$root = Get-RideAuditOctopusJson -Connection $cx -Path "/api"
Write-Host ("octopus version={0} installation={1}" -f $root.Version, $root.InstallationId)

if (-not $SkipSync) {
    Write-Host "syncing tree to $SshHost:$RemoteAbs"
    & (Join-Path $here "Sync-RideAuditTree.ps1") -SshHost $SshHost -RemoteAbs $RemoteAbs -WithPublish:$WithPublish
}

$account = Ensure-SshAccount -Connection $cx
$fingerprint = Get-RideAuditSshFingerprint -SshHost $SshHost
Write-Host ("ssh fingerprint MD5 (host key, not a secret)={0}" -f $fingerprint)
$machine = Ensure-LinuxTarget -Connection $cx -Account $account -Fingerprint $fingerprint
$healthTask = Start-MachineHealth -Connection $cx -MachineId $machine.Id
if ($healthTask) {
    $health = Wait-OctopusTask -Connection $cx -TaskId $healthTask.Id -TimeoutSec 180
    Write-Host ("health task {0}" -f $health.State)
    $machine = Get-RideAuditOctopusJson -Connection $cx -Path "/api/machines/$($machine.Id)"
    Write-Host ("machine health={0} summary={1}" -f $machine.HealthStatus, $machine.StatusSummary)
}

$project = Ensure-ProjectAndProcess -Connection $cx
if ([string]::IsNullOrWhiteSpace($Version)) {
    $sha = (git rev-parse --short HEAD).Trim()
    $Version = "0.1.0-$sha"
}

$result = New-RideAuditReleaseAndDeploy -Connection $cx -Project $project -ReleaseVersion $Version
Write-Host ("release-state task={0}" -f $result.Task.State)
if ($result.Task.State -ne "Success") {
    throw "Octopus deployment task $($result.Task.Id) ended $($result.Task.State)"
}

Write-Host "probing $AdmissionUrl"
$probe = Invoke-WebRequest -Uri $AdmissionUrl -TimeoutSec 20
Write-Host ("probe HTTP={0} bytes={1}" -f [int]$probe.StatusCode, $probe.RawContentLength)
if ([int]$probe.StatusCode -ne 200) {
    throw "admission probe expected HTTP 200, got $([int]$probe.StatusCode)"
}

$web = "$($cx.PortalBase)/app#/Spaces-1/projects/$($project.Slug)/deployments/$($result.Deployment.Id)"
$apiWeb = "$($cx.ApiBase)/app#/Spaces-1/projects/$($project.Slug)/deployments/$($result.Deployment.Id)"
Write-Host "OCTOPUS_PROJECT=$($project.Id)"
Write-Host "OCTOPUS_RELEASE=$($result.Release.Id)"
Write-Host "OCTOPUS_RELEASE_VERSION=$($result.Release.Version)"
Write-Host "OCTOPUS_DEPLOYMENT=$($result.Deployment.Id)"
Write-Host "OCTOPUS_TASK=$($result.Task.Id)"
Write-Host "OCTOPUS_MACHINE=$($machine.Id)"
Write-Host "OCTOPUS_WEB=$web"
Write-Host "OCTOPUS_WEB_HTTP=$apiWeb"
Write-Host "OCTOPUS_PROBE_HTTP=$([int]$probe.StatusCode)"
Write-Host "RIDEAUDIT_OCTOPUS_OK"
