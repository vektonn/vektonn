@echo off

set ProjectName=%1

rem reset current directory to the location of this script
pushd "%~dp0"

if exist "./%ProjectName%/bin" (
    rd "./%ProjectName%/bin" /Q /S || exit /b 1
)

dotnet build --force --no-incremental --configuration Release || exit /b 1

dotnet pack --no-build --configuration Release "./%ProjectName%/%ProjectName%.csproj" || exit /b 1

pause