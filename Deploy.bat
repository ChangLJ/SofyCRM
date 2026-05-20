@echo off
REM ============================================================
REM   SofyCRM - One-click Docker Deploy Script
REM   Phase 5 Requirement #4
REM ============================================================
setlocal enabledelayedexpansion
cd /d "%~dp0"

echo.
echo ==========================================================
echo   SofyCRM Deploy - %DATE% %TIME%
echo ==========================================================
echo.

where docker >nul 2>nul
if errorlevel 1 (
    echo [ERROR] Docker not found. Please install Docker Desktop first.
    pause
    exit /b 1
)

echo [1/5] Stopping existing containers ...
docker compose down
if errorlevel 1 (
    echo [WARN] docker compose down returned non-zero, continuing...
)

echo.
echo [2/5] Pulling latest base images ...
docker compose pull --ignore-pull-failures

echo.
echo [3/5] Building images (no cache for backend / frontend) ...
docker compose build --no-cache backend frontend
if errorlevel 1 (
    echo [ERROR] docker compose build failed.
    pause
    exit /b 1
)

echo.
echo [4/5] Starting containers ...
docker compose up -d
if errorlevel 1 (
    echo [ERROR] docker compose up failed.
    pause
    exit /b 1
)

echo.
echo [5/5] Status:
docker compose ps

echo.
echo ==========================================================
echo   Deploy completed.
echo     Frontend : http://localhost:5173
echo     Backend  : http://localhost:8080
echo     Swagger  : http://localhost:8080/swagger
echo     Postgres : localhost:5432  (sofycrm / sofycrm / sofycrm)
echo ==========================================================
echo.

pause
endlocal
