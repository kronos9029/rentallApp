@echo off
setlocal

dotnet run "%~dp0ProtectSecrets.cs" -- %*
exit /b %errorlevel%
