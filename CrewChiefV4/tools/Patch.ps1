﻿# Patches CC install directory with newest Release files.  Not complete, can be grown and improved to cleanup stuff, settings, sounds etc.
# For some unknown reason does not work from git powershell prompt, so either run from Explorer, ISE or elevated PS prompt.
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) {   
    $arguments = "& '" + $myinvocation.mycommand.definition + "'"
    Start-Process powershell -Verb RunAs -ArgumentList $arguments
    break
}

function GetScriptDirectory {
  $invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $invocation.MyCommand.Path
}

function OverwriteFile($from, $to) {

    Write-Host "Overwriting "  $to " with " $from
    Copy-Item $from -Destination $to -Force
    
    echo ""
}

function MirrorDirectory($from, $to) {

    Write-Host "Mirroring " $from " with " $to
    
    $cmdArgs = @("$from","$to",'/MIR')
    robocopy @cmdArgs

    echo ""
}

$toolsPath = GetScriptDirectory
$releaseBinPath = $toolsPath + "\..\bin\Release\"
$rootPath = $toolsPath + "\..\"
$ccLayoutMainPath = ${env:ProgramFiles(x86)} + "\Britton IT Ltd\CrewChiefV4\"

echo "Patching main CC install"
echo ""

# MirrorDirectory $rootPath\"plugins" $ccLayoutMainPath"\plugins"
MirrorDirectory $rootPath\"sounds\background_sounds" $env:LOCALAPPDATA"\CrewChiefV4\sounds\background_sounds"
MirrorDirectory $rootPath\"sounds\driver_names" $env:LOCALAPPDATA"\CrewChiefV4\sounds\driver_names"
MirrorDirectory $rootPath\"sounds\fx" $env:LOCALAPPDATA"\CrewChiefV4\sounds\fx"
MirrorDirectory $rootPath\"sounds\voice" $env:LOCALAPPDATA"\CrewChiefV4\sounds\voice"

OverwriteFile $releaseBinPath\"CrewChiefV4.exe" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"CrewChiefV4.exe.config" $ccLayoutMainPath
OverwriteFile $rootPath\"ui_text.txt" $ccLayoutMainPath
OverwriteFile $rootPath\"carClassData.json" $ccLayoutMainPath
OverwriteFile $rootPath\"trackLandmarksData.json" $ccLayoutMainPath
OverwriteFile $rootPath\"saved_command_macros.json" $ccLayoutMainPath
OverwriteFile $rootPath\"speech_recognition_config.txt" $ccLayoutMainPath

# Update binary dependencies.
OverwriteFile $releaseBinPath\"AutoUpdater.NET.dll" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"MathNet.Numerics.dll" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"NAudio.dll" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"Newtonsoft.Json.dll" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"SharpDX.dll" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"SharpDX.DirectInput.dll" $ccLayoutMainPath
OverwriteFile $releaseBinPath\"websocket-sharp.dll" $ccLayoutMainPath

echo "Press any key to finish..."

Read-Host