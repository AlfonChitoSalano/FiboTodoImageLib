param($version, $buildType)

Set-StrictMode -version latest

function GetNpmBuildScriptName() {
#     if($buildType -ne "release") {
#         return "build-dev"
#     }
    return "build"
}

function Main() {
    . .\NpmUtils.ps1

    EnsureNodeJs
    
    npm --prefer-offline --no-audit --progress=false ci
    CheckLastExitCode

    npm run (GetNpmBuildScriptName)
    CheckLastExitCode
}

Main
