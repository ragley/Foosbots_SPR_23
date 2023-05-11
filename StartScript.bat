@echo off

start /min "" "C:/Foosball Sim/env/pyenv377/Scripts/python.exe" "c:/Foosball Sim/Foosbots_SPR_23/Assets/Scripts/Python/ball_tracking_limeGreen.py"

timeout /t 2 /nobreak >nul

start /min "" "C:/Foosball Sim/Foosbots_SPR_23/InfBuild/Foosbots_Inference_Engine.exe"

timeout /t 2 /nobreak >nul

echo Press Any Key To Quit...

pause >nul

if errorlevel 1 (
    echo The Enter key was not pressed
 
) else (
    call sendkeys.bat "Circle" "q"
    TASKKILL /F /IM "Foosbots_Inference_Engine.exe"
)


