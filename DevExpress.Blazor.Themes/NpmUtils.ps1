Set-StrictMode -version latest
$ErrorActionPreference = "Stop"

function CheckLastExitCode() {
    if($LastExitCode -ne 0) {
        Write-Error -ErrorAction Stop "Last exit code: $LastExitCode"
    }
}

function EnsureNodeJs() {
    $env:NODE_OPTIONS = $null
    $nodeCommand = Get-Command node -ErrorAction SilentlyContinue

    if($nodeCommand) {
        Write-Host "Node.js found in PATH: $($nodeCommand.Source)"
        Write-Host "Node.js version: $($nodeCommand.Version)"
    } else {
        $newPathEntry = "C:\Program Files\nodejs" # Default for builder03 demos publish

        $node = @(Get-ChildItem 'C:\Program Files (x86)\Microsoft Visual Studio\*\*\MSBuild\Microsoft\VisualStudio\NodeJs') | Sort -desc
        if($node.Length -ne 0) {
            $newPathEntry = $node[0]
        }

        # TODO: Check that the path entry does not exist before adding
        Write-Host "Add $newPathEntry to the PATH"
        $env:path += ";$newPathEntry"
    }
}

function ResolveVersion($version) {
    $useVersion = -not [string]::IsNullOrWhitespace($version)
    if($useVersion) {
        $nums = $version.Split('.')
        if($nums.Count -ne 3 -or $buildType -ne "release") {
            $useVersion = $false
        } elseif($nums[2] -eq "2") {
            $version += "-beta"
        }
    }

    return $useVersion, $version
}

function ShouldEnableLegacyPeerDeps($buildVersion, $buildType) {
    $useResolved, $resolved = ResolveVersion $buildVersion
    return -Not($useResolved)
}

function IsDxPackage($name) {
    return ($name -Match '^@?devex(treme|press)')
}

function ResolvePackageVersion($buildVersion, $buildType, $packageName) {
    $npmdeps = (Get-Content npmdeps.json -raw | ConvertFrom-Json).dependencies

    $useBuildVersion, $buildVersion = ResolveVersion $buildVersion $buildType
    $resolvedVersion = if($useBuildVersion -and (IsDxPackage $packageName)) { $buildVersion } else { $npmdeps.$package }
    return $resolvedVersion
}

function GetTarballUrl($package) {
    $url = npm view $package dist.tarball --json --registry http://npmproxy:4873 | ConvertFrom-Json
    CheckLastExitCode
    if(-not $url) {
        Write-Error "Can't get tarball url for $package"
    }
    return $url
}

function DownloadFile($url, $targetPath) {
    Write-Host "downloading: $url -> $targetPath"

    $originalPreference = $ProgressPreference
    try {
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest $url -OutFile $targetPath
    } finally {
        $ProgressPreference = $originalPreference
    }
}

function UnpackTgz($file, $outDir, [switch]$extractAll) {
    Write-Host "UnpackTgz: $file -> $outDir"

    $guid = [Guid]::NewGuid() # See https://github.com/PowerShell/PowerShell/issues/14100
    $tempDir = Join-Path $env:TMP $guid
    New-Item -ItemType Directory -Path $tempDir | Out-Null
    try {
        Write-Host "Extracting to $tempDir"
        $extractPattern = if($extractAll) { 'package' } else { 'package/dist' }
        tar -xzf $file -C $tempDir --strip-components 1 $extractPattern
        CheckLastExitCode

        $outDir = Resolve-Path $outDir
        Write-Host "Copying files to $outDir"
        Copy-Item -Path $tempDir\* -Destination $outDir -Recurse
    } finally {
        Remove-Item $tempDir -Recurse
    }
}

function DownloadPackages(
    [string]$buildVersion,
    [string]$buildType,
    [Parameter(Mandatory)][string[]]$packages,
    $targetDir = "./node_modules",
    [switch]$extractAll
) {
    Write-Host "Requested packages: $($packages -join ', ')"

    foreach($package in $packages) {
        $version = ResolvePackageVersion $buildVersion $buildType $package
        Write-Host "package:" $package@$version

        $packageDir = Join-Path $targetDir $package
        if(-Not(Test-Path $packageDir)) {
            New-Item -Type Directory -Path $packageDir | Out-Null

            $url = GetTarballUrl $package@$version
            # Workaround for `The term 'New-TemporaryFile' is not recognized as the name of a cmdlet, ...`
            # See https://github.com/PowerShell/PowerShell/issues/14100
            #
            # $file = New-TemporaryFile
            $file = [System.IO.Path]::GetTempFileName()
            try {
                DownloadFile $url $file
                UnpackTgz $file $packageDir -extractAll:$extractAll
            } finally {
                Remove-Item $file
            }
        } else {
            Write-Host "Already exists, skipped: $packageDir"
        }
    }
}

# NPM/Local-Paths: https://docs.npmjs.com/cli/v8/configuring-npm/package-json#local-paths
function GetLocalPathPackages() {
    return (Get-Content .\package.json | ConvertFrom-Json).dependencies.PSObject.Properties |
        Where-Object { $_.Value.EndsWith(".tgz") } |
        Select-Object -ExpandProperty Name
}

function DownloadTarballs(
    [string]$buildVersion,
    [string]$buildType,
    [string]$targetDir = "./.npmdeps"
) {
    $packages = GetLocalPathPackages
    Write-Host "Tarball dependencies: $($packages -join ', ')"

    if(-Not(Test-Path $targetDir)) {
        mkdir $targetDir | Out-Null
    }

    foreach($package in $packages) {
        $version = ResolvePackageVersion $buildVersion $buildType $package
        Write-Host "package:" $package@$version

        $url = GetTarballUrl $package@$version
        $file = Join-Path $targetDir ("$package.tgz".Replace("/", "_"))
        DownloadFile $url $file
    }
}

function InstallPackages(
    [string]$buildVersion,
    [string]$buildType
) {
    DownloadTarballs -buildVersion $buildVersion -buildType $buildType

    $installArgs = "install", "--no-audit", "--no-fund", "--prefer-offline", "--ignore-scripts", "--no-package-lock", "--registry=http://npmproxy:4873"
    if(ShouldEnableLegacyPeerDeps -buildVersion $buildVersion -buildType $buildType) {
        $installArgs += "--legacy-peer-deps"
    }
    npm @installArgs
    CheckLastExitCode
}

New-Alias -Name Install-Packages -Value InstallPackages
New-Alias -Name Download-Packages -Value DownloadPackages
New-Alias -Name Check-LastExitCode -Value CheckLastExitCode
New-Alias -Name Ensure-NodeJS -Value EnsureNodeJS
