@echo off

set InstallerProcess="FreeJoy Configurator.exe"

if Exist "C:\freejoy_tmp" rmdir "C:\freejoy_tmp" /s /q
MD C:\freejoy_tmp

xcopy "." "C:\freejoy_tmp" /s /q

"C:\freejoy_tmp\FreeJoy Configurator.application" /wait
goto :checker

:check
cls
echo "Installing application.."

:checker
tasklist | find %InstallerProcess%
if errorlevel 1 goto :check

echo "Installation complete!"
echo "Deleting tmp files.."
rmdir "C:\freejoy_tmp" /s /q
echo "Done!"
exit
