@echo off
pushd %~dp0
powershell -File pre-commit.ps1 -Arguments %*
popd
exit /b %ERRORLEVEL%