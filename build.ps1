$DotNetInstallerUri = 'https://dot.net/v1/dotnet-install.ps1';
$DotNetUnixInstallerUri = 'https://dot.net/v1/dotnet-install.sh'
$DotNetChannel = 'LTS'

$DotNetVersion = Get-Content -Path .\global.json | ConvertFrom-Json | Select-Object -ExpandProperty sdk | Select-Object -ExpandProperty version

if ([string]::IsNullOrEmpty($DotNetVersion)) {
    'Failed to pars .NET SDK Version'
    exit 1
}

# Make sure tools folder exists
$ToolPath = Join-Path $PSScriptRoot "tools"
if (!(Test-Path $ToolPath)) {
    Write-Verbose "Creating tools directory..."
    New-Item -Path $ToolPath -Type Directory -Force | out-null
}

$PlaywrightBrowserDir = Join-Path $PSScriptRoot "tools/browsers"
if (!(Test-Path $PlaywrightBrowserDir)) {
    Write-Verbose "Creating browsers directory..."
    New-Item -Path $PlaywrightBrowserDir -Type Directory -Force | out-null
}

$ENV:PLAYWRIGHT_BROWSERS_PATH = $PlaywrightBrowserDir

if ($PSVersionTable.PSEdition -ne 'Core') {
    # Attempt to set highest encryption available for SecurityProtocol.
    # PowerShell will not set this by default (until maybe .NET 4.6.x). This
    # will typically produce a message for PowerShell v2 (just an info
    # message though)
    try {
        # Set TLS 1.2 (3072), then TLS 1.1 (768), then TLS 1.0 (192), finally SSL 3.0 (48)
        # Use integers because the enumeration values for TLS 1.2 and TLS 1.1 won't
        # exist in .NET 4.0, even though they are addressable if .NET 4.5+ is
        # installed (.NET 4.5 is an in-place upgrade).
        [System.Net.ServicePointManager]::SecurityProtocol = 3072 -bor 768 -bor 192 -bor 48
      } catch {
        Write-Output 'Unable to set PowerShell to use TLS 1.2 and TLS 1.1 due to old .NET Framework installed. If you see underlying connection closed or trust errors, you may need to upgrade to .NET Framework 4.5+ and PowerShell v3'
      }
}

###########################################################################
# INSTALL .NET CORE CLI
###########################################################################

Function Remove-PathVariable([string]$VariableToRemove)
{
    $SplitChar = ';'
    if ($IsMacOS -or $IsLinux) {
        $SplitChar = ':'
    }

    $path = [Environment]::GetEnvironmentVariable("PATH", "User")
    if ($path -ne $null)
    {
        $newItems = $path.Split($SplitChar, [StringSplitOptions]::RemoveEmptyEntries) | Where-Object { "$($_)" -inotlike $VariableToRemove }
        [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join($SplitChar, $newItems), "User")
    }

    $path = [Environment]::GetEnvironmentVariable("PATH", "Process")
    if ($path -ne $null)
    {
        $newItems = $path.Split($SplitChar, [StringSplitOptions]::RemoveEmptyEntries) | Where-Object { "$($_)" -inotlike $VariableToRemove }
        [Environment]::SetEnvironmentVariable("PATH", [System.String]::Join($SplitChar, $newItems), "Process")
    }
}

# Get .NET Core CLI path if installed.
$FoundDotNetCliVersion = $null;
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $FoundDotNetCliVersion = dotnet --version;
}
if($FoundDotNetCliVersion -lt $DotNetVersion -or
    $FoundDotNetVersion -gt $DotNetVersion.SubString($DotNetVersion.IndexOf('.'))) {
    $InstallPath = Join-Path $PSScriptRoot ".dotnet"
    
    if (!(Test-Path $InstallPath)) {
        New-Item -Path $InstallPath -ItemType Directory -Force | Out-Null;
    }
    
    if ($IsMacOS -or $IsLinux) {
        (New-Object System.Net.WebClient).DownloadFile($DotNetUnixInstallerUri, "$InstallPath\dotnet-install.sh");
        & bash $InstallPath\dotnet-install.sh --version "$DotNetVersion" --install-dir "$InstallPath" --channel "$DotNetChannel" --no-path
    } else {
        (New-Object System.Net.WebClient).DownloadFile($DotNetInstallerUri, "$InstallPath\dotnet-install.ps1");
        & $InstallPath\dotnet-install.ps1 -Channel $DotNetChannel -Version $DotNetVersion -InstallDir $InstallPath;
    }
    
    Remove-PathVariable "$InstallPath"
    $env:PATH = "$InstallPath;$env:PATH"
}

$DotNetRoot = Get-Command dotnet -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source | Split-Path -Parent

$env:DOTNET_ROOT=$DotNetRoot
$env:DOTNET_CLI_TELEMETRY_OPTOUT=1
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Restore .NET tools
dotnet tool restore

###########################################################################
# RUN BUILD SCRIPT
###########################################################################

& dotnet tool run dotnet-cake ./build.cake --bootstrap
if ($LASTEXITCODE -eq 0){
    & dotnet tool run dotnet-cake ./build.cake $args
}
exit $LASTEXITCODE