@echo off
REM ============================================================
REM   SofyCRM - 一鍵部署「前端 + Mock Data」至 GCP Cloud Run
REM   不需後端 / Cloud SQL（純靜態 + 本地 Mock）
REM ============================================================
setlocal enabledelayedexpansion
cd /d "%~dp0"

echo.
echo ==========================================================
echo   SofyCRM Cloud Run Deploy (Frontend + Mock Data)
echo ==========================================================
echo.

where gcloud >nul 2>nul
if errorlevel 1 (
    echo [ERROR] 找不到 gcloud CLI，請先安裝 Google Cloud SDK
    echo https://cloud.google.com/sdk/docs/install
    pause
    exit /b 1
)

if "%GCP_PROJECT_ID%"=="" (
    for /f "delims=" %%i in ('gcloud config get-value project 2^>nul') do set GCP_PROJECT_ID=%%i
)
if "%GCP_PROJECT_ID%"=="" (
    echo [ERROR] 請先設定專案： gcloud config set project YOUR_PROJECT_ID
    pause
    exit /b 1
)

set REGION=asia-east1
set SERVICE=sofycrm

echo Project : %GCP_PROJECT_ID%
echo Region  : %REGION%
echo Service : %SERVICE%
echo.

echo [1/3] 啟用必要 API ...
gcloud services enable run.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com --project=%GCP_PROJECT_ID%

echo.
echo [2/3] 從原始碼建置並部署至 Cloud Run（約 3-8 分鐘）...
gcloud run deploy %SERVICE% ^
  --source=frontend ^
  --region=%REGION% ^
  --project=%GCP_PROJECT_ID% ^
  --allow-unauthenticated ^
  --port=8080 ^
  --memory=512Mi ^
  --cpu=1 ^
  --min-instances=0 ^
  --max-instances=3

if errorlevel 1 (
    echo [ERROR] 部署失敗
    pause
    exit /b 1
)

echo.
echo [3/3] 取得服務網址 ...
gcloud run services describe %SERVICE% --region=%REGION% --project=%GCP_PROJECT_ID% --format="value(status.url)"

echo.
echo ==========================================================
echo   部署完成！請用上方 URL 開啟，登入頁選擇 Mock 身份即可。
echo ==========================================================
echo.
pause
endlocal
