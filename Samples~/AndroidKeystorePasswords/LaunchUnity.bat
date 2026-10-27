@echo off
REM ===========================================================================
REM  Unity Toolbox - Android keystore password automation
REM
REM  Sets the keystore passwords as environment variables and launches Unity,
REM  where KeystorePasswordSetter.cs picks them up and applies them to the
REM  Android Player Settings.
REM
REM  Passwords are NEVER stored in this file. Put them in local-secrets.bat
REM  next to this script (it is gitignored), or export them yourself.
REM ===========================================================================
setlocal

REM -------------------------------
REM  Edit these two paths
REM -------------------------------
set "UNITY_EDITOR_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.52f1\Editor\Unity.exe"
set "UNITY_PROJECT_PATH=D:\UnityProjects\YourProjectName"
REM -------------------------------

if not exist "%UNITY_EDITOR_PATH%" (
    echo [ERROR] Unity Editor not found at:
    echo         %UNITY_EDITOR_PATH%
    echo         Update UNITY_EDITOR_PATH in this file.
    pause
    exit /b 1
)

REM Load local passwords if that file exists.
if exist "%~dp0local-secrets.bat" (
    call "%~dp0local-secrets.bat"
)

if not defined UNITY_KEYSTORE_PASS (
    echo [ERROR] UNITY_KEYSTORE_PASS is not set.
    echo.
    echo Create "local-secrets.bat" next to this script containing:
    echo     set "UNITY_KEYSTORE_PASS=your-keystore-password"
    echo     set "UNITY_KEYALIAS_PASS=your-key-alias-password"
    echo.
    echo That file is already listed in .gitignore - do not commit it.
    pause
    exit /b 1
)

echo Launching Unity with keystore credentials from the environment...
start "" "%UNITY_EDITOR_PATH%" -projectPath "%UNITY_PROJECT_PATH%"
