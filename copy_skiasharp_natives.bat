@echo off
REM Copy SkiaSharp native libraries to ensure they're available at runtime
REM Usage: copy_skiasharp_natives.bat [OutputPath] [ProjectDir]

set "OUTPUT_DIR=%~1"
set "PROJECT_DIR=%~2"

if "%OUTPUT_DIR%"=="" (
    echo Error: Output directory not specified
    exit /b 1
)

if "%PROJECT_DIR%"=="" (
    echo Error: Project directory not specified
    exit /b 1
)

echo Copying SkiaSharp native assets...
echo Output directory: %OUTPUT_DIR%
echo Project directory: %PROJECT_DIR%

REM Create architecture-specific directories
if not exist "%OUTPUT_DIR%x64" mkdir "%OUTPUT_DIR%x64"
if not exist "%OUTPUT_DIR%x86" mkdir "%OUTPUT_DIR%x86"
if not exist "%OUTPUT_DIR%arm64" mkdir "%OUTPUT_DIR%arm64"

REM Copy native libraries from packages to output
set "PACKAGES_DIR=%PROJECT_DIR%..\packages\SkiaSharp.NativeAssets.Win32.3.119.0\runtimes"

if exist "%PACKAGES_DIR%\win-x64\native\libSkiaSharp.dll" (
    echo Copying x64 native library...
    copy "%PACKAGES_DIR%\win-x64\native\libSkiaSharp.dll" "%OUTPUT_DIR%x64\libSkiaSharp.dll" >nul
    copy "%PACKAGES_DIR%\win-x64\native\libSkiaSharp.dll" "%OUTPUT_DIR%libSkiaSharp.dll" >nul
) else (
    echo Warning: x64 native library not found at "%PACKAGES_DIR%\win-x64\native\libSkiaSharp.dll"
)

if exist "%PACKAGES_DIR%\win-x86\native\libSkiaSharp.dll" (
    echo Copying x86 native library...
    copy "%PACKAGES_DIR%\win-x86\native\libSkiaSharp.dll" "%OUTPUT_DIR%x86\libSkiaSharp.dll" >nul
) else (
    echo Warning: x86 native library not found at "%PACKAGES_DIR%\win-x86\native\libSkiaSharp.dll"
)

if exist "%PACKAGES_DIR%\win-arm64\native\libSkiaSharp.dll" (
    echo Copying arm64 native library...
    copy "%PACKAGES_DIR%\win-arm64\native\libSkiaSharp.dll" "%OUTPUT_DIR%arm64\libSkiaSharp.dll" >nul
) else (
    echo Warning: arm64 native library not found at "%PACKAGES_DIR%\win-arm64\native\libSkiaSharp.dll"
)

echo SkiaSharp native assets copied successfully!
exit /b 0