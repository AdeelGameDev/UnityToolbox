@echo off
setlocal

echo Enter the root folder containing your Unity projects:
set /p ROOT=Path: 

if not exist "%ROOT%" (
    echo The specified path does not exist.
    pause
    exit /b
)

echo.
echo This will permanently delete all Library folders
echo inside Unity projects under:
echo %ROOT%
echo.
echo Make sure all Unity projects are CLOSED.
echo.

set /p CONFIRM=Are you sure you want to continue? (Y/N): 
if /I not "%CONFIRM%"=="Y" (
    echo Operation cancelled.
    pause
    exit /b
)

echo.
echo Scanning projects...
echo.

for /d %%D in ("%ROOT%\*") do (
    if exist "%%D\Assets" if exist "%%D\ProjectSettings" if exist "%%D\Library" (
        echo Cleaning: %%D
        rmdir /s /q "%%D\Library"
    )
)

echo.
echo Finished cleaning all Library folders.
pause
