param(
    [Parameter(Mandatory = $false)]
    [string]$UpstreamBranch
)

$ErrorActionPreference = "Stop"

# Ensure we are inside a git repository.
git rev-parse --is-inside-work-tree *> $null
if ($LASTEXITCODE -ne 0) {
    throw "This script must be run inside a Git repository."
}

$remotes = git remote
if (-not ($remotes -contains "upstream")) {
    throw "Remote 'upstream' is missing. Run .\\scripts\\setup-upstream.ps1 first."
}

if (-not $UpstreamBranch) {
    $UpstreamBranch = git config --local --get sync.upstreamBranch
}
if (-not $UpstreamBranch) {
    $UpstreamBranch = "main"
}

git fetch upstream --prune
if ($LASTEXITCODE -ne 0) {
    throw "Fetch failed."
}

# Validate branch existence and fallback to master if needed.
git show-ref --verify --quiet ("refs/remotes/upstream/" + $UpstreamBranch)
if ($LASTEXITCODE -ne 0) {
    if ($UpstreamBranch -eq "main") {
        git show-ref --verify --quiet "refs/remotes/upstream/master"
        if ($LASTEXITCODE -eq 0) {
            $UpstreamBranch = "master"
        }
        else {
            throw "Neither upstream/main nor upstream/master exists."
        }
    }
    else {
        throw "upstream/$UpstreamBranch does not exist."
    }
}

$needsStash = $false
$status = git status --porcelain
if ($status) {
    $needsStash = $true
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    git stash push -u -m ("auto-sync-" + $stamp)
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to stash local changes."
    }
    Write-Host "Local changes were stashed temporarily."
}

try {
    git rebase ("upstream/" + $UpstreamBranch)
    if ($LASTEXITCODE -ne 0) {
        throw "Rebase failed. Resolve conflicts, then continue with: git rebase --continue"
    }

    Write-Host "Sync completed successfully from upstream/$UpstreamBranch."
}
finally {
    if ($needsStash) {
        $stashList = git stash list
        if ($stashList -and $stashList.Contains("auto-sync-")) {
            git stash pop
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "Could not auto-apply stashed changes cleanly. Resolve conflicts manually."
            }
            else {
                Write-Host "Re-applied local stashed changes."
            }
        }
    }
}
