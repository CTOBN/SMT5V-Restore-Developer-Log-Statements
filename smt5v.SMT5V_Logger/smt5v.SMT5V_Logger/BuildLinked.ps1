# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/smt5v.SMT5V_Logger/*" -Force -Recurse
dotnet publish "./smt5v.SMT5V_Logger.csproj" -c Release -o "$env:RELOADEDIIMODS/smt5v.SMT5V_Logger" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location