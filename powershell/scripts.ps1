<# list files by creationtime #>
$path = "C:\Users\mp\Downloads"
Get-ChildItem -path $path -r | Where-Object { $_.CreationTime.Date -eq (get-date).AddDays(0).Date } | Select-Object Name, CreationTime > result.txt

