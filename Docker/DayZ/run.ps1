$ErrorActionPreference = 'Stop';
Set-Location C:\GameServer\ ;

# install / update DayZ
C:\steamcmd\steamcmd.exe +login anonymous +force_install_dir C:\DayZServer +app_update 223350 validate +quit;

# start server
try {
    Write-Host 'Start DayZ Docker Server.';
    & .\DayZServer_x64.exe -profiles=profiles -port=2302 -dologs -adminlog -netlog -freezecheck;
    Wait-Process -Name DayZServer_x64 ;
} catch {
    throw;
} finally {
    Write-Host 'End DayZ Docker Server';
}