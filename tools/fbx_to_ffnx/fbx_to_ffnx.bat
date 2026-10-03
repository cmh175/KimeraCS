@echo off
rem FBX -> FFNx glTF. Drag a character folder onto this file.
rem
rem The folder needs:
rem   reference\<NAME>.gltf + .bin   KimeraCS export of the FF7 model being replaced (e.g. AAAC), with the
rem                                  animations you are replacing. NAME becomes the output name.
rem   character.fbx (or model.fbx)  optional: the new model. Without it the model is taken from the idle's FBX
rem                                  (the reference's first animation), since every animation FBX holds it too
rem   ACFE.fbx, AAFF.fbx, ...       one FBX per animation, named after the FF7 animation it replaces
rem Output: <folder>\ffnx\<NAME>.gltf + .bin + textures, and <NAME>_export_report.txt
rem
rem Root motion: by default the new animations keep their own hip height and travel (FFNx with animation
rem independence uses the glTF's root motion; FFNx 1.24.0 ignores root motion). For FFNx builds that apply the
rem game's root motion instead, run from a command line: fbx_to_ffnx.bat <folder> --root-motion game
rem
rem Needs Blender 4.5 (set BLENDER below if it is installed somewhere else).

set "BLENDER=C:\Program Files\Blender Foundation\Blender 4.5\blender.exe"
if not exist "%BLENDER%" (
    echo Blender 4.5 was not found at "%BLENDER%". Edit BLENDER at the top of this file.
    pause
    exit /b 1
)
if "%~1"=="" (
    echo Drag a character folder onto this file.
    pause
    exit /b 1
)

"%BLENDER%" -b --factory-startup -P "%~dp0fbx_to_ffnx.py" -- --folder %* > "%TEMP%\fbx_to_ffnx.log" 2>&1
findstr /b /c:"  " /c:"Reference" /c:"Model" /c:"Root motion" /c:"Skeleton" /c:"Size" /c:"Meshes" /c:"Animations" /c:"Warnings" /c:"Written" "%TEMP%\fbx_to_ffnx.log"
findstr /c:"FBX_TO_FFNX_DONE" "%TEMP%\fbx_to_ffnx.log" >nul
if errorlevel 1 (
    echo.
    echo CONVERSION FAILED - the last lines of Blender's log:
    powershell -NoProfile -Command "Get-Content '%TEMP%\fbx_to_ffnx.log' -Tail 15"
)
pause
