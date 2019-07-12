@echo off
TITLE restartgamespy
:autorestart
GamespyMasterServer.exe -ip 96.254.179.4
echo Restarting gamespy

goto autorestart