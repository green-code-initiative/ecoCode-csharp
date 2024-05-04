param (
    [string]$versionNumber
)

git checkout main
git fetch origin main
git pull origin main
git tag $versionNumber
git push origin $versionNumber