param([string] $newVersion)
        
$assemblyPattern = """[0-9]+(\.([0-9]+|\*)){1,3}"""
$foundFiles = Get-ChildItem .\*AssemblyInfo.cs -Recurse

foreach ($file in $foundFiles)
{   
	(Get-Content $file) | ForEach-Object {$_ -Replace $assemblyPattern, ("""" + $newVersion + """") } | Set-Content $file                               
}

$foundFiles = Get-ChildItem .\*.wxs -Recurse

foreach ($file in $foundFiles)
{   
	(Get-Content $file) | ForEach-Object {$_ -Replace ("ProductVersion=" + $assemblyPattern), ("ProductVersion=""" + $newVersion + """") } | Set-Content $file                               
}