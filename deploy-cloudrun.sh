#!/usr/bin/env bash
# SofyCRM — 部署「前端 + Mock Data」至 GCP Cloud Run（Cloud Shell / Linux / macOS）
set -euo pipefail

PROJECT_ID="${GCP_PROJECT_ID:-$(gcloud config get-value project 2>/dev/null)}"
REGION="${GCP_REGION:-asia-east1}"
SERVICE="${GCP_SERVICE:-sofycrm}"

if [[ -z "${PROJECT_ID}" || "${PROJECT_ID}" == "(unset)" ]]; then
  echo "[ERROR] 請先設定專案： gcloud config set project YOUR_PROJECT_ID"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "${SCRIPT_DIR}"

if [[ ! -d frontend ]]; then
  echo "[ERROR] 找不到 frontend/ 目錄，請在專案根目錄執行此腳本"
  exit 1
fi

echo "============================================================"
echo "  SofyCRM Cloud Run Deploy (Frontend + Mock Data)"
echo "============================================================"
echo "Project : ${PROJECT_ID}"
echo "Region  : ${REGION}"
echo "Service : ${SERVICE}"
echo ""

echo "[1/3] 啟用必要 API ..."
gcloud services enable run.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com \
  --project="${PROJECT_ID}"

echo ""
echo "[2/3] 從原始碼建置並部署至 Cloud Run（約 3-8 分鐘）..."
gcloud run deploy "${SERVICE}" \
  --source=frontend \
  --region="${REGION}" \
  --project="${PROJECT_ID}" \
  --allow-unauthenticated \
  --port=8080 \
  --memory=512Mi \
  --cpu=1 \
  --min-instances=0 \
  --max-instances=3

echo ""
echo "[3/3] 服務網址："
gcloud run services describe "${SERVICE}" \
  --region="${REGION}" \
  --project="${PROJECT_ID}" \
  --format="value(status.url)"

echo ""
echo "部署完成！開啟上方 URL，登入頁選擇 Admin / Sales / Service 即可。"
