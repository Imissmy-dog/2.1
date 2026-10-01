@echo off
cd /d "%~dp0"
if exist "publish\SnakeGame.exe" (
    start "" "publish\SnakeGame.exe"
) else (
    dotnet run
)
