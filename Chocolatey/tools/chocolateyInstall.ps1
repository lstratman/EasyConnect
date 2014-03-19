$version = "1.6.0.0"
$packageName = "EasyConnect"
$installerType = "msi"
$url = "http://lstratman.github.io/EasyConnect/updates/EasyConnect-$version.msi"
$silentArgs = "/quiet"
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$installerType" "$silentArgs" "$url"  -validExitCodes $validExitCodes
