param (
    [string]$BLAISE_LICENSE_KEY,
    [string]$BLAISE_ACTIVATION_CODE
)

. "$PSScriptRoot\functions\LoggingFunctions.ps1"

LogInfo("Registering Blaise")

$regPath = 'HKLM:\SOFTWARE\StatNeth\Blaise\5.0'

if (-not (Test-Path $regPath)) {
    New-Item -Path $regPath -Force | Out-Null
}

Set-ItemProperty -Path $regPath -Name 'LicenseKey' -Value $BLAISE_LICENSE_KEY
Set-ItemProperty -Path $regPath -Name 'ActivationCode' -Value $BLAISE_ACTIVATION_CODE
Set-ItemProperty -Path $regPath -Name 'Licensee' -Value "ONS"
Set-ItemProperty -Path $regPath -Name 'CompanyName' -Value "Office for National Statistics"
Set-ItemProperty -Path $regPath -Name 'UserName' -Value 'ONS-USER'
