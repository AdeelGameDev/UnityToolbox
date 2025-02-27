@echo off
REM ================================
REM Unity Build Launcher Script
REM ================================

REM Set environment variables for keystore and key alias passwords.
REM Replace these with actual passwords if needed.
set UNITY_KEYSTORE_PASS=1234567
set UNITY_KEYALIAS_PASS=1234567

REM --------------------------------
REM Specify the Unity Editor path
REM --------------------------------
REM Change this path to match your installed Unity Editor version and location.
set UNITY_EDITOR_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.52f1\Editor\Unity.exe"

REM --------------------------------
REM Specify the Unity project path
REM --------------------------------
REM Change this path to match your actual Unity project directory.
set UNITY_PROJECT_PATH="D:\UnityProjects\YourProjectName"

REM --------------------------------
REM Launch Unity with the specified project
REM --------------------------------
REM The 'start' command runs Unity in a separate process.
start "" %UNITY_EDITOR_PATH% -projectPath %UNITY_PROJECT_PATH%
