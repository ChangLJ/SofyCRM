# SofyCRM — GCP Cloud Run 部署（前端 + Mock Data）

本指南部署**僅前端**至 [Cloud Run](https://cloud.google.com/run)，資料來源固定為 **Mock Data**（不需後端 API、不需 Cloud SQL）。

## 前置需求

1. [Google Cloud SDK](https://cloud.google.com/sdk/docs/install)（`gcloud`）
2. GCP 專案與帳單已啟用
3. 本機已安裝 Git（若要推送到 GitHub）

```bash
gcloud auth login
gcloud config set project YOUR_PROJECT_ID
```

## 方式 A：一鍵腳本

**Windows**
```bat
DeployCloudRun.bat
```

**Cloud Shell / Linux / macOS**
```bash
git clone https://github.com/ChangLJ/SofyCRM.git
cd SofyCRM
chmod +x deploy-cloudrun.sh
./deploy-cloudrun.sh
```

完成後終端機會印出服務 URL，例如：`https://sofycrm-xxxxx-de.a.run.app`

## 方式 B：gcloud 指令

```bash
gcloud services enable run.googleapis.com cloudbuild.googleapis.com --project=YOUR_PROJECT_ID

gcloud run deploy sofycrm \
  --source=frontend \
  --region=asia-east1 \
  --allow-unauthenticated \
  --port=8080 \
  --memory=512Mi
```

## 方式 C：Cloud Build（CI/CD）

```bash
gcloud builds submit --config cloudbuild.yaml .
```

會建置 Docker 映像、推送到 Artifact Registry，並部署至 Cloud Run。

## 使用方式

1. 開啟 Cloud Run 服務 URL
2. 在登入頁點選 **Admin / Sales / Service** 一鍵登入
3. 所有資料皆為 Mock，不會呼叫後端 API

## 推送到 GitHub

遠端倉庫：[https://github.com/ChangLJ/SofyCRM.git](https://github.com/ChangLJ/SofyCRM.git)

```bash
git init
git add .
git commit -m "SofyCRM: frontend mock data + Cloud Run deploy"
git branch -M main
git remote add origin https://github.com/ChangLJ/SofyCRM.git
git push -u origin main
```

若倉庫已有內容且為空倉庫，直接 push 即可。若需權限，請使用 Personal Access Token 或 SSH。

## 技術說明

| 項目 | 說明 |
|------|------|
| 映像 | `frontend/Dockerfile` — Vite build + nginx |
| 埠號 | Cloud Run `PORT=8080`，nginx 透過 `nginx.conf.template` 動態監聽 |
| API | 固定 Mock，`repositories/index.ts` 僅載入 `mockRepos` |
| 健康檢查 | `GET /healthz` 回傳 `ok` |

## 常見問題

### PERMISSION_DENIED：Build failed / compute@developer.gserviceaccount.com

Cloud Run 從原始碼部署時，預設服務帳戶需存取 GCS（`run-sources-*` bucket）。在 Cloud Shell 執行：

```bash
cd SofyCRM   # 或 git clone 後進入目錄
chmod +x fix-cloudrun-iam.sh
./fix-cloudrun-iam.sh macro-truck-496913-m1
```

或手動（將 `PROJECT_NUMBER` 換成你的專案編號，例如 `933927389657`）：

```bash
PROJECT_ID=macro-truck-496913-m1
PROJECT_NUMBER=$(gcloud projects describe $PROJECT_ID --format='value(projectNumber)')

gcloud services enable run.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com storage.googleapis.com

# Cloud Build 服務帳戶
for ROLE in roles/run.admin roles/artifactregistry.writer roles/storage.admin roles/iam.serviceAccountUser roles/cloudbuild.builds.builder; do
  gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:${PROJECT_NUMBER}@cloudbuild.gserviceaccount.com" \
    --role=$ROLE
done

# 預設 Compute 服務帳戶（上傳原始碼 zip 時需要）
gcloud projects add-iam-policy-binding $PROJECT_ID \
  --member="serviceAccount:${PROJECT_NUMBER}-compute@developer.gserviceaccount.com" \
  --role=roles/storage.objectAdmin
```

完成後再執行 `./deploy-cloudrun.sh`。

參考：[Cloud Run build service account](https://cloud.google.com/run/docs/configuring/services/build-service-account)

### 仍看到 401？

清除瀏覽器該網站的 localStorage（`sofycrm.auth`、`sofycrm.dataSource`），重新整理後用 Mock 身份登入。

### 要連真實後端？

需另外部署 `backend` 至 Cloud Run / GKE（目前專案已固定 Mock）。
