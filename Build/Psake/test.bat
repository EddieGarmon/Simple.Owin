@echo off

powershell.exe -NoProfile -ExecutionPolicy unrestricted -Command "& {Import-Module .\psake.psm1; Invoke-psake .\build_script.ps1 Test;}" 

Pause