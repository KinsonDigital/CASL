
$baseDirPath = $PSScriptRoot;
$file = "$baseDirPath/data.txt";
$content = Get-Content -Path $file;
Write-Output $content;
