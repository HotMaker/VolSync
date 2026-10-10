@echo off
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
echo Using %CSC%
"%CSC%" /nologo /target:winexe /out:VolSync.exe src\VolSync.cs
if errorlevel 1 ( echo Build FAILED & exit /b 1 )
echo.
echo Build OK -^> VolSync.exe
endlocal
