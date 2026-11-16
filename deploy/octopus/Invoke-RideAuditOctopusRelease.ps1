# From PAYTON-LEGION2: ensure RideAudit Octopus project, sync tree, release, deploy.
# FR-RIDE-063 / UC-RIDE-032 / TR-RIDE-DEPLOY-001 / TR-RIDE-DEPLOY-002 / TEST-RIDE-038.
# No GHCR. Secrets stay in ~/.creds. SPDX-License-Identifier: GPL-2.0-only

[CmdletBinding()]
param(
    [string]$ProjectName = "RideAudit",
    [string]$EnvironmentName = "Development",
    [string]$TargetName = "LAB-OMARCHY-DOCKER",
    [string]$TargetRole = "rideaudit-host-lab-omarchy",
    [string]$SshHost = "LAB-OMARCHY",
    [string]$RemoteAbs = "/home/sharpninja/github/rideaudit",
    [string]$SshConnectHost = "172.21.0.1",
    [string]$AdmissionUrl = "http://192.168.1.182:28080/",
    [string]$ApiBase = "",
    [string]$CredPath = "",
    [string]$Version = "",
    [switch]$SkipSync,
    [switch]$SkipHealthCheck,
    [switch]$WithPublish,
    [ValidateSet("Tentacle", "Ssh", "RunOnServer")]
    [string]$WorkerMode = ""
)

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $here "OctopusApi.ps1")
. (Join-Path $here "..\omarchy\OmarchySsh.ps1")

$scriptBody = @"
set -eu
ROOT="$RemoteAbs"
test -f "`$ROOT/deploy/octopus/remote-build-and-run.sh"
command -v docker
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
    $accounts = @(Get-OctopusItems -Connection $Connection -Path "/api/Spaces-1/accounts/all")
    if (@($accounts).Count -eq 0) {
        $accounts = @(Get-OctopusItems -Connection $Connection -Path "/api/accounts/all")
    }
    $existing = @($accounts) | Where-Object { $_.Name -eq "RideAudit LAB-OMARCHY SSH" } | Select-Object -First 1
    if ($existing) {
        Write-Host ("using account {0}" -f $existing.Id)
        return $existing
    }

    $keyPath = Join-Path $env:USERPROFILE ".ssh\id_ed25519_lab_omarchy"
    $yamlPath = Join-Path $env:USERPROFILE ".creds\lab-omarchy.yaml"
    $body = $null
    if (Test-Path -LiteralPath $keyPath) {
        $pem = [IO.File]::ReadAllText($keyPath)
        $pemB64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($pem))
        $body = @{
            AccountType                     = "SshKeyPair"
            Name                            = "RideAudit LAB-OMARCHY SSH"
            Description                     = "FR-RIDE-063 Linux Docker host SSH. Created from LEGION2 key file, not git."
            Username                        = "sharpninja"
            PrivateKeyFile                  = @{ HasValue = $true; NewValue = $pemB64 }
            TenantedDeploymentParticipation = "Untenanted"
        }
        Write-Host "creating SSH key-pair account from local key file (not printed)"
    }
    elseif (Test-Path -LiteralPath $yamlPath) {
        $yaml = Get-RideAuditYamlMap -Path $yamlPath
        $body = @{
            AccountType                     = "UsernamePassword"
            Name                            = "RideAudit LAB-OMARCHY SSH"
            Description                     = "FR-RIDE-063 Linux Docker host SSH. Password from ~/.creds, not git."
            Username                        = $yaml.username
            Password                        = @{ HasValue = $true; NewValue = $yaml.password }
            TenantedDeploymentParticipation = "Untenanted"
        }
        Write-Host "creating username/password account from cred yaml (not printed)"
    }
    else {
        throw "no SSH key or lab-omarchy.yaml available to create an Octopus account"
    }

    $created = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/accounts" -Body $body
    Write-Host ("created account {0}" -f $created.Id)
    return $created
}

function Ensure-Environment {
    param($Connection, [string]$Name)
    $envs = @(Get-OctopusItems -Connection $Connection -Path "/api/environments/all")
    $existing = @($envs) | Where-Object { $_.Name -eq $Name } | Select-Object -First 1
    if ($existing) {
        Write-Host ("using environment {0}" -f $existing.Id)
        return $existing
    }
    $created = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/environments" -Body @{
        Name             = $Name
        Description      = "FR-RIDE-063 LAB-OMARCHY"
        SortOrder        = 1
        UseGuidedFailure = $false
    }
    Write-Host ("created environment {0}" -f $created.Id)
    return $created
}

function Ensure-LinuxTarget {
    param($Connection, $Account, [string]$Fingerprint, [string[]]$EnvironmentIds)
    $machines = @(Get-OctopusItems -Connection $Connection -Path "/api/machines/all")
    $existing = @($machines) | Where-Object { $_.Name -eq "LAB-OMARCHY-LINUX" } | Select-Object -First 1
    if ($existing) {
        $dirty = $false
        if ($existing.Endpoint.Host -ne $SshConnectHost) {
            $existing.Endpoint.Host = $SshConnectHost
            $existing.Endpoint.Uri = "ssh://${SshConnectHost}:22/"
            $existing.Endpoint.Port = 22
            $dirty = $true
        }
        $roles = @($existing.Roles)
        if (($roles -notcontains $TargetRole) -or ($roles.Count -ne 1) -or ($roles[0] -ne $TargetRole)) {
            $existing.Roles = @($TargetRole)
            $dirty = $true
        }
        $missingEnvironments = @(@($EnvironmentIds) | Where-Object { @($existing.EnvironmentIds) -notcontains $_ })
        if ($missingEnvironments.Count -gt 0) {
            # The deploy pins SpecificMachineIds, so the reused target must be in the deploy environment.
            $existing.EnvironmentIds = @(@($existing.EnvironmentIds) + $missingEnvironments)
            $dirty = $true
        }
        if ($dirty) {
            $existing = Invoke-RideAuditOctopusApi -Connection $Connection -Method PUT -Path "/api/Spaces-1/machines/$($existing.Id)" -Body $existing
            Write-Host ("updated machine {0} host={1} roles={2}" -f $existing.Id, $existing.Endpoint.Host, (($existing.Roles) -join ","))
        }
        Write-Host ("using machine {0} health={1}" -f $existing.Id, $existing.HealthStatus)
        return $existing
    }

    $body = @{
        Name                            = "LAB-OMARCHY-LINUX"
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
        EnvironmentIds                  = @($EnvironmentIds)
        Roles                           = @($TargetRole)
        TenantIds                       = @()
    }
    $created = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/machines" -Body $body
    Write-Host ("created machine {0}" -f $created.Id)
    return $created
}

function Wait-RoleTarget {
    param($Connection, [string]$EnvironmentId, [int]$TimeoutSec = 240)
    if ([string]::IsNullOrWhiteSpace($EnvironmentId)) {
        throw "Wait-RoleTarget needs the $EnvironmentName environment id; refuse to pick a target outside it"
    }
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    do {
        $machines = @(Get-OctopusItems -Connection $Connection -Path "/api/machines/all")
        $namedMatches = @(@($machines) | Where-Object { $_.Name -eq $TargetName })
        # Only a target in the deploy environment with the deploy role can be pinned with SpecificMachineIds.
        $roledMatches = @(@($machines) | Where-Object { (@($_.Roles) -contains $TargetRole) -and (@($_.EnvironmentIds) -contains $EnvironmentId) })

        if (-not [string]::IsNullOrWhiteSpace($TargetName)) {
            if ($namedMatches.Count -gt 1) {
                throw "ambiguous Octopus targets named $TargetName count=$($namedMatches.Count)"
            }
            if ($namedMatches.Count -eq 1) {
                $hit = $namedMatches[0]
                $inEnvironment = @($hit.EnvironmentIds) -contains $EnvironmentId
                $hasRole = @($hit.Roles) -contains $TargetRole
                if (-not ($inEnvironment -and $hasRole)) {
                    throw ("Octopus target {0} ({1}) is not eligible: in {2}={3}, role {4}={5}; refuse to pin a deploy to it" -f $TargetName, $hit.Id, $EnvironmentName, $inEnvironment, $TargetRole, $hasRole)
                }
                Write-Host ("using target {0} name={1} health={2} style={3}" -f $hit.Id, $hit.Name, $hit.HealthStatus, $hit.Endpoint.CommunicationStyle)
                return $hit
            }
            if ($roledMatches.Count -gt 1) {
                $names = ($roledMatches | ForEach-Object { $_.Name }) -join ", "
                throw "ambiguous Octopus role $TargetRole matches multiple targets ($names) while TargetName=$TargetName was set but not found; refuse role-first fallback to an old host"
            }
            if ($roledMatches.Count -eq 1) {
                $hit = $roledMatches[0]
                Write-Host ("using unique-role target {0} name={1} health={2} style={3} (exact TargetName $TargetName not found)" -f $hit.Id, $hit.Name, $hit.HealthStatus, $hit.Endpoint.CommunicationStyle)
                return $hit
            }
        }
        else {
            if ($roledMatches.Count -gt 1) {
                $names = ($roledMatches | ForEach-Object { $_.Name }) -join ", "
                throw "ambiguous Octopus role $TargetRole matches multiple targets ($names); set TargetName to the exact machine"
            }
            if ($roledMatches.Count -eq 1) {
                $hit = $roledMatches[0]
                Write-Host ("using target {0} name={1} health={2} style={3}" -f $hit.Id, $hit.Name, $hit.HealthStatus, $hit.Endpoint.CommunicationStyle)
                return $hit
            }
        }

        Write-Host "waiting for tentacle/target name=$TargetName role=$TargetRole"
        Start-Sleep -Seconds 5
    } while ((Get-Date) -lt $deadline)
    throw "no Octopus target with name $TargetName or unique role $TargetRole in $EnvironmentName after $TimeoutSec seconds"
}

function Start-MachineHealth {
    param($Connection, [string]$MachineId)
    $body = @{
        Name        = "Health"
        Description = "Health check $TargetName for FR-RIDE-063"
        SpaceId     = "Spaces-1"
        Arguments   = @{
            MachineIds         = @($MachineId)
            Timeout            = "00:05:00"
            MachineTimeout     = "00:02:00"
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
    param($Connection, [string]$RunMode)
    $projects = @(Get-OctopusItems -Connection $Connection -Path "/api/projects/all")
    $project = @($projects) | Where-Object { $_.Name -eq $ProjectName } | Select-Object -First 1
    if (-not $project) {
        $body = @{
            Name           = $ProjectName
            Description    = "FR-RIDE-063: Octopus builds RideAudit containers and deploys them to LAB-OMARCHY. No GHCR."
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

    $runOnServer = if ($RunMode -eq "RunOnServer") { "true" } else { "false" }
    $stepProps = @{
        "Octopus.Action.RunOnServer"         = $runOnServer
        "Octopus.Action.EnabledFeatures"     = ""
        "Octopus.Action.Script.ScriptSource" = "Inline"
        "Octopus.Action.Script.Syntax"       = "Bash"
        "Octopus.Action.Script.ScriptBody"   = $scriptBody
    }
    $stepTopProps = @{}
    if ($RunMode -ne "RunOnServer") {
        $stepTopProps["Octopus.Action.TargetRoles"] = $TargetRole
    }

    $process = Get-RideAuditOctopusJson -Connection $Connection -Path "/api/Spaces-1/deploymentprocesses/$($project.DeploymentProcessId)"
    $stepName = "Build and run RideAudit containers"
    $step = @{
        Name               = $stepName
        Type               = "Step"
        PackageRequirement = "LetOctopusDecide"
        Properties         = $stepTopProps
        Condition          = "Success"
        StartTrigger       = "StartAfterPrevious"
        Actions            = @(
            @{
                ActionType                    = "Octopus.Script"
                Name                          = $stepName
                IsDisabled                    = $false
                IsRequired                    = $true
                CanBeUsedForProjectVersioning = $false
                WorkerPoolId                  = $null
                Container                     = @{ Image = $null; FeedId = $null }
                Environments                  = @()
                ExcludedEnvironments          = @()
                Channels                      = @()
                TenantTags                    = @()
                Packages                      = @()
                Condition                     = "Success"
                Properties                    = $stepProps
            }
        )
    }
    $process.Steps = @($step)
    $updated = Invoke-RideAuditOctopusApi -Connection $Connection -Method PUT -Path "/api/Spaces-1/deploymentprocesses/$($process.Id)" -Body $process
    Write-Host ("updated process {0} version={1} worker={2}" -f $updated.Id, $updated.Version, $RunMode)
    return $project
}

function New-RideAuditReleaseAndDeploy {
    param($Connection, $Project, [string]$ReleaseVersion, [string]$MachineId = "")
    $channels = @(Get-OctopusItems -Connection $Connection -Path "/api/Spaces-1/projects/$($Project.Id)/channels")
    $channel = @($channels) | Select-Object -First 1
    if (-not $channel) { throw "project $($Project.Id) has no channel" }
    $releaseBody = @{
        ProjectId    = $Project.Id
        ChannelId    = $channel.Id
        Version      = $ReleaseVersion
        ReleaseNotes = "FR-RIDE-063 RideAudit containers to LAB-OMARCHY. Local Docker only. No GHCR."
    }
    $release = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/releases" -Body $releaseBody
    Write-Host ("created release {0} version={1}" -f $release.Id, $release.Version)

    $envs = @(Get-OctopusItems -Connection $Connection -Path "/api/environments/all")
    $env = @($envs) | Where-Object { $_.Name -eq $EnvironmentName } | Select-Object -First 1
    if (-not $env) { throw "environment $EnvironmentName not found" }

    $deployBody = @{
        ReleaseId     = $release.Id
        EnvironmentId = $env.Id
        Comments      = "FR-RIDE-063 / UC-RIDE-032"
    }
    if (-not [string]::IsNullOrWhiteSpace($MachineId)) {
        # Pin to the machine Wait-RoleTarget / Ensure-LinuxTarget chose so a shared role cannot fan out to Tentacle + SSH.
        $deployBody["SpecificMachineIds"] = @($MachineId)
        Write-Host ("deploy pinned to SpecificMachineIds={0}" -f $MachineId)
    }
    $deployment = Invoke-RideAuditOctopusApi -Connection $Connection -Method POST -Path "/api/Spaces-1/deployments" -Body $deployBody
    Write-Host ("created deployment {0} task={1}" -f $deployment.Id, $deployment.TaskId)
    $task = Wait-OctopusTask -Connection $Connection -TaskId $deployment.TaskId -TimeoutSec 5400
    return [pscustomobject]@{
        Release     = $release
        Deployment  = $deployment
        Task        = $task
        Channel     = $channel
        Environment = $env
    }
}

Write-Host "FR-RIDE-063 RideAudit Octopus release from PAYTON-LEGION2. No GHCR."
$rideauditCred = Join-Path $env:USERPROFILE ".creds\octopus-rideaudit.cred.xml"
if ([string]::IsNullOrWhiteSpace($ApiBase)) {
    if (Test-Path -LiteralPath $rideauditCred) {
        $ApiBase = "http://192.168.1.182:18066"
    }
    else {
        $ApiBase = "http://192.168.1.182:8066"
    }
}
if ([string]::IsNullOrWhiteSpace($CredPath) -and $ApiBase -match ":18066" -and (Test-Path -LiteralPath $rideauditCred)) {
    $CredPath = $rideauditCred
}
if ([string]::IsNullOrWhiteSpace($WorkerMode)) {
    $WorkerMode = if ($ApiBase -match ":18066") { "Tentacle" } else { "Ssh" }
}
if ([string]::IsNullOrWhiteSpace($CredPath)) {
    $cx = Get-RideAuditOctopusConnection -ApiBase $ApiBase
}
else {
    $cx = Get-RideAuditOctopusConnection -ApiBase $ApiBase -CredPath $CredPath
}
Write-Host ("octopus api={0} portal={1} stored-url={2} cred={3} worker={4}" -f $cx.ApiBase, $cx.PortalBase, $cx.ServerUrl, $cx.CredPath, $WorkerMode)

$root = Get-RideAuditOctopusJson -Connection $cx -Path "/api"
Write-Host ("octopus version={0} installation={1}" -f $root.Version, $root.InstallationId)

if (-not $SkipSync) {
    Write-Host "syncing tree to ${SshHost}:${RemoteAbs}"
    & (Join-Path $here "Sync-RideAuditTree.ps1") -SshHost $SshHost -RemoteAbs $RemoteAbs -WithPublish:$WithPublish
}

$environment = Ensure-Environment -Connection $cx -Name $EnvironmentName
$machine = $null
if ($WorkerMode -eq "Tentacle") {
    $machine = Wait-RoleTarget -Connection $cx -EnvironmentId $environment.Id -TimeoutSec 240
}
elseif ($WorkerMode -eq "Ssh") {
    $account = Ensure-SshAccount -Connection $cx
    $fingerprint = Get-RideAuditSshFingerprint -SshHost $SshHost
    Write-Host ("ssh fingerprint MD5 (host key, not a secret)={0}" -f $fingerprint)
    $machine = Ensure-LinuxTarget -Connection $cx -Account $account -Fingerprint $fingerprint -EnvironmentIds @($environment.Id)
}

if ($machine -and -not $SkipHealthCheck) {
    $healthTask = Start-MachineHealth -Connection $cx -MachineId $machine.Id
    if ($healthTask) {
        try {
            $health = Wait-OctopusTask -Connection $cx -TaskId $healthTask.Id -TimeoutSec 180
            Write-Host ("health task {0}" -f $health.State)
        }
        catch {
            Write-Host ("health wait skipped: {0}" -f $_.Exception.Message)
        }
        $machine = Get-RideAuditOctopusJson -Connection $cx -Path "/api/machines/$($machine.Id)"
        Write-Host ("machine health={0} summary={1}" -f $machine.HealthStatus, $machine.StatusSummary)
    }
}

$project = Ensure-ProjectAndProcess -Connection $cx -RunMode $WorkerMode
if ([string]::IsNullOrWhiteSpace($Version)) {
    $sha = (git rev-parse --short HEAD).Trim()
    $stamp = Get-Date -Format "yyyyMMddHHmmss"
    $Version = "0.1.0-$sha-$stamp"
}

if (($WorkerMode -eq "Tentacle" -or $WorkerMode -eq "Ssh") -and (-not $machine -or [string]::IsNullOrWhiteSpace($machine.Id))) {
    throw "WorkerMode=$WorkerMode requires a selected Octopus machine before deploy; refuse role-wide deploy"
}
$machineIdForDeploy = if ($machine) { $machine.Id } else { $null }
$result = New-RideAuditReleaseAndDeploy -Connection $cx -Project $project -ReleaseVersion $Version -MachineId $machineIdForDeploy
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
if ($machine) { Write-Host "OCTOPUS_MACHINE=$($machine.Id)" }
Write-Host "OCTOPUS_WEB=$web"
Write-Host "OCTOPUS_WEB_HTTP=$apiWeb"
Write-Host "OCTOPUS_PROBE_HTTP=$([int]$probe.StatusCode)"
Write-Host "RIDEAUDIT_OCTOPUS_OK"
