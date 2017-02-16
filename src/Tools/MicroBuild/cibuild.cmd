@echo off
powershell -noprofile -executionPolicy RemoteSigned -file %~dp0cibuild.ps1

