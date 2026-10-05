@echo off
rem KimeraCS glTF -> FBX files for retargeting (iClone, Maya, MotionBuilder, UE5). Drag a .gltf onto this file.
rem
rem Output: a "fbx" folder next to the .gltf with one FBX per animation (ACFE.fbx, AAFF.fbx ...), each holding the
rem model, its skeleton and that animation with textures embedded, plus <name>_fbx_report.txt. The skeleton gets a
rem "pelvis" bone on top that carries the body motion (standing height, walking, jumping), FF7 joints under it.
rem
rem Options can follow the file when run from a command line, e.g.:
rem   gltf_to_fbx.bat RTAM.gltf --fps 15          (sets the frame rate; default: the glTF's own)
rem   gltf_to_fbx.bat AAAC.gltf --height 1.8 --anims ACFE,AAFF
rem   --skeleton-only (animation files without the model), --model-file (also <name>.fbx in the rest pose)
rem
rem Needs Blender 4.5 (set BLENDER below if it is installed somewhere else).

set "BLENDER=C:\Program Files\Blender Foundation\Blender 4.5\blender.exe"
if not exist "%BLENDER%" (
    echo Blender 4.5 was not found at "%BLENDER%". Edit BLENDER at the top of this file.
    pause
    exit /b 1
)
if "%~1"=="" (
    echo Drag a KimeraCS .gltf export onto this file.
    pause
    exit /b 1
)

"%BLENDER%" -b --factory-startup -P "%~dp0gltf_to_fbx.py" -- %* > "%TEMP%\gltf_to_fbx.log" 2>&1
findstr /b /c:"  " /c:"Model" /c:"Skeleton" /c:"Meshes" /c:"Written" /c:"Done" "%TEMP%\gltf_to_fbx.log"
findstr /c:"GLTF_TO_FBX_DONE" "%TEMP%\gltf_to_fbx.log" >nul
if errorlevel 1 (
    echo.
    echo CONVERSION FAILED - the last lines of Blender's log:
    powershell -NoProfile -Command "Get-Content '%TEMP%\gltf_to_fbx.log' -Tail 15"
)
pause
