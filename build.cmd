@echo off
pushd %~dp0
powershell -File build.ps1 -Arguments %*
popd