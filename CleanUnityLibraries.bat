@echo off
set ROOT=D:\UnityProjects

for /d %%D in ("%ROOT%\*") do (
    if exist "%%D\Library" (
        echo Deleting %%D\Library
        rmdir /s /q "%%D\Library"
    )
)

echo Done cleaning Library folders.
pause
