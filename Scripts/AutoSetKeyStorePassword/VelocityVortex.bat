@echo off
REM Set environment variables for keystore and key alias passwords
set UNITY_KEYSTORE_PASS=1234567
set UNITY_KEYALIAS_PASS=1234567

@echo off
REM Launch Unity with the specified project in a separate process
start "" "C:\Program Files\Unity\Hub\Editor\2022.3.52f1\Editor\Unity.exe" -projectPath "D:\Projects\VelocityVortex\VelocityVortex - 2022.3.52"
