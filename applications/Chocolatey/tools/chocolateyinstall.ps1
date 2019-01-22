
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/richardszalay/iishosts/releases/download/v1.0.1/iishosts_1.0.1_x86.msi'
$url64      = 'https://github.com/richardszalay/iishosts/releases/download/v1.0.1/iishosts_1.0.1_x64.msi'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'msi'
  url           = $url
  url64bit      = $url64

  softwareName  = 'iishosts*'

  checksum      = '25C244C987570ECD489E232C3440AADD4793C09B2358BC5C64E1252429F8606A'
  checksumType  = 'sha256'
  checksum64    = 'C7FB961943090AA26454F306036ADB28E6F74F41929DEF473AE449567DD4F42F'
  checksumType64= 'sha256'

  #NOTE: quiet installation (/qn) has been excluded here because prevented the installer from correctly identifying the iis version  
  silentArgs    = "/norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}

Install-ChocolateyPackage @packageArgs
