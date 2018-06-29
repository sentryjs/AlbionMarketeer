@echo off

Pushd C:\Users\joshu\source\repos\AlbionMarketeer\AlbionMarketeer\Output

"c:\Program Files (x86)\Windows Kits\10\bin\x86\signtool.exe" sign /f VigilGaming.pfx /p !!JJmm159753 /t http://timestamp.verisign.com/scripts/timstamp.dll "AlbionMarketeerSetup.exe"
