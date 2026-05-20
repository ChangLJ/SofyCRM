#!/usr/bin/env bash
# 修復 Cloud Run「從原始碼部署」時的 IAM 權限不足
# 錯誤例：PERMISSION_DENIED ... 933927389657-compute@developer.gserviceaccount.com
set -euo pipefail

PROJECT_ID="${1:-$(gcloud config get-value project 2>/dev/null)}"

if [[ -z "${PROJECT_ID}" || "${PROJECT_ID}" == "(unset)" ]]; then
  echo "用法: ./fix-cloudrun-iam.sh YOUR_PROJECT_ID"
  echo "或先執行: gcloud config set project YOUR_PROJECT_ID"
  exit 1
fi

PROJECT_NUMBER=$(gcloud projects describe "${PROJECT_ID}" --format='value(projectNumber)')
CB_SA="${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com"
COMPUTE_SA="${PROJECT_NUMBER}-compute@developer.gserviceaccount.com"

echo "Project ID     : ${PROJECT_ID}"
echo "Project Number : ${PROJECT_NUMBER}"
echo "Cloud Build SA : ${CB_SA}"
echo "Compute SA     : ${COMPUTE_SA}"
echo ""

bind_role() {
  local member="$1"
  local role="$2"
  echo "  + ${role}  ->  ${member}"
  gcloud projects add-iam-policy-binding "${PROJECT_ID}" \
    --member="serviceAccount:${member}" \
    --role="${role}" \
    --condition=None \
    --quiet >/dev/null
}

echo "[1/2] 啟用 API ..."
gcloud services enable run.googleapis.com cloudbuild.googleapis.com \
  artifactregistry.googleapis.com storage.googleapis.com \
  --project="${PROJECT_ID}"

echo ""
echo "[2/2] 授予 IAM 角色（需專案 Owner 或 IAM Admin）..."

# Cloud Build 服務帳戶 — 建置與部署 Cloud Run 所需
for role in \
  roles/run.admin \
  roles/artifactregistry.writer \
  roles/storage.admin \
  roles/iam.serviceAccountUser \
  roles/logging.logWriter \
  roles/cloudbuild.builds.builder; do
  bind_role "${CB_SA}" "${role}"
done

# 預設 Compute 服務帳戶 — 上傳 run-sources 至 GCS 時會用到
for role in \
  roles/storage.objectAdmin \
  roles/storage.admin; do
  bind_role "${COMPUTE_SA}" "${role}"
done

echo ""
echo "IAM 設定完成。請重新執行："
echo "  cd SofyCRM && ./deploy-cloudrun.sh"
echo ""
echo "若仍失敗，請確認你的帳號 (fndplay1111@gmail.com) 具備 roles/owner 或 roles/resourcemanager.projectIamAdmin"
