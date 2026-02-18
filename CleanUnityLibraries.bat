@echo off
set ROOT=D:\Game Development\Projects

for /d %%D in ("%ROOT%\*") do (
    if exist "%%D\Assets" if exist "%%D\ProjectSettings" if exist "%%D\Library" (
        echo Deleting %%D\Library
        rmdir /s /q "%%D\Library"
    )
)

echo Done cleaning Library folders.
pause
