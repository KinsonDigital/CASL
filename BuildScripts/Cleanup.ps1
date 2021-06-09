
param ($dirToDelete)

if ($null -eq $dirToDelete -or $dirToDelete -eq "")
{
    throw "The variable dirToDelete parameter must not be null or empty."
}
else
{
    if (Test-Path $dirToDelete)
    {
        Remove-Item $dirToDelete -Force -Recurse
    }
}
