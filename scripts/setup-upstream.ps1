param(
    [Parameter(Mandatory = $true)]
    [string]$UpstreamUrl,

    [Parameter(Mandatory = $false)]
    [string]$UpstreamBranch = "main"
)

$ErrorActionPreference = "Stop"

# Ensure we are inside a git repository.
git rev-parse --is-inside-work-tree *> $null
if ($LASTEXITCODE -ne 0) {
    throw "This script must be run inside a Git repository."
}

$existing = git remote
if ($existing -contains "upstream") {
    git remote set-url upstream $UpstreamUrl
    Write-Host "Updated upstream remote URL."
}
else {
    git remote add upstream $UpstreamUrl
    Write-Host "Added upstream remote."
}

git fetch upstream --prune
if ($LASTEXITCODE -ne 0) {
    throw "Failed to fetch from upstream. Check URL and network access."
}

# Store preferred upstream branch in local git config for sync script.
git config --local sync.upstreamBranch $UpstreamBranch

Write-Host "Upstream is ready."
Write-Host "Remote URL: $UpstreamUrl"
Write-Host "Preferred branch: $UpstreamBranch"
Write-Host "Next step: run .\\scripts\\sync-upstream.ps1"
