# SofyCRM – GCP Cloud SQL 部署指令清單

> 本檔案彙整 PostgreSQL Schema / Seed 的執行順序，以及 GCP Cloud SQL（PostgreSQL）建立、連線與匯入指令。
> 對應 Phase 5 Requirement #5。

---

## 1. 本機 / Docker 內初始化

`docker-compose.yml` 已將下列檔案掛載至 `postgres` 容器的 `/docker-entrypoint-initdb.d/`，**第一次** 啟動會自動執行：

```
sql/01_schema.sql   -> 建立所有資料表、enum、index、constraint
sql/02_seed.sql     -> 預設使用者、客戶、商機等 demo data
```

若要重跑 Schema：

```bash
docker compose down -v          # 注意：-v 會清空 DB
docker compose up -d --build
```

手動執行（容器已存在時）：

```bash
docker exec -i sofycrm-postgres psql -U sofycrm -d sofycrm < sql/01_schema.sql
docker exec -i sofycrm-postgres psql -U sofycrm -d sofycrm < sql/02_seed.sql
```

---

## 2. GCP Cloud SQL – 建立執行個體

```bash
# 設定預設專案
gcloud config set project YOUR_PROJECT_ID

# 建立 Cloud SQL for PostgreSQL 16
gcloud sql instances create sofycrm-db \
  --database-version=POSTGRES_16 \
  --tier=db-custom-1-3840 \
  --region=asia-east1 \
  --storage-size=20GB \
  --storage-type=SSD \
  --availability-type=ZONAL \
  --backup-start-time=18:00

# 建立資料庫
gcloud sql databases create sofycrm --instance=sofycrm-db

# 建立應用使用者（請改用安全密碼）
gcloud sql users create sofycrm \
  --instance=sofycrm-db \
  --password=ChangeMe-StrongPassword
```

---

## 3. 匯入 Schema / Seed

### 方法 A：透過 Cloud Storage（官方推薦）

```bash
# 1) 建立 GCS bucket（若尚未建立）
gsutil mb -l asia-east1 gs://YOUR_BUCKET_SOFYCRM_SQL

# 2) 上傳 SQL
gsutil cp sql/01_schema.sql gs://YOUR_BUCKET_SOFYCRM_SQL/
gsutil cp sql/02_seed.sql   gs://YOUR_BUCKET_SOFYCRM_SQL/

# 3) 授權 Cloud SQL Service Account 讀取 bucket
SQL_SA=$(gcloud sql instances describe sofycrm-db --format="value(serviceAccountEmailAddress)")
gsutil iam ch serviceAccount:${SQL_SA}:objectViewer gs://YOUR_BUCKET_SOFYCRM_SQL

# 4) 匯入
gcloud sql import sql sofycrm-db \
  gs://YOUR_BUCKET_SOFYCRM_SQL/01_schema.sql \
  --database=sofycrm

gcloud sql import sql sofycrm-db \
  gs://YOUR_BUCKET_SOFYCRM_SQL/02_seed.sql \
  --database=sofycrm
```

### 方法 B：透過 Cloud SQL Auth Proxy + psql

```bash
# 1) 啟動 proxy（背景）
cloud-sql-proxy YOUR_PROJECT_ID:asia-east1:sofycrm-db \
  --port=5432 &

# 2) 直接用 psql 灌檔
PGPASSWORD=ChangeMe-StrongPassword \
  psql -h 127.0.0.1 -U sofycrm -d sofycrm -f sql/01_schema.sql

PGPASSWORD=ChangeMe-StrongPassword \
  psql -h 127.0.0.1 -U sofycrm -d sofycrm -f sql/02_seed.sql
```

---

## 4. Cloud Run 部署 Backend

```bash
# 1) Build & Push image（Artifact Registry）
gcloud artifacts repositories create sofycrm \
  --repository-format=docker --location=asia-east1 \
  --description="SofyCRM images"

gcloud builds submit ./backend \
  --tag=asia-east1-docker.pkg.dev/YOUR_PROJECT_ID/sofycrm/backend:latest

# 2) Deploy 到 Cloud Run，並連線 Cloud SQL
gcloud run deploy sofycrm-backend \
  --image=asia-east1-docker.pkg.dev/YOUR_PROJECT_ID/sofycrm/backend:latest \
  --region=asia-east1 \
  --platform=managed \
  --allow-unauthenticated \
  --add-cloudsql-instances=YOUR_PROJECT_ID:asia-east1:sofycrm-db \
  --set-env-vars="ASPNETCORE_URLS=http://+:8080" \
  --set-env-vars="ConnectionStrings__Default=Host=/cloudsql/YOUR_PROJECT_ID:asia-east1:sofycrm-db;Database=sofycrm;Username=sofycrm;Password=ChangeMe-StrongPassword" \
  --set-env-vars="Jwt__Issuer=SofyCRM" \
  --set-env-vars="Jwt__Audience=SofyCRM.Client" \
  --set-env-vars="Jwt__Secret=REPLACE_WITH_STRONG_SECRET_32+chars" \
  --set-env-vars="Jwt__AccessTokenMinutes=60" \
  --set-env-vars="Jwt__RefreshTokenDays=14"
```

---

## 5. Cloud Run 部署 Frontend

```bash
gcloud builds submit ./frontend \
  --tag=asia-east1-docker.pkg.dev/YOUR_PROJECT_ID/sofycrm/frontend:latest

gcloud run deploy sofycrm-frontend \
  --image=asia-east1-docker.pkg.dev/YOUR_PROJECT_ID/sofycrm/frontend:latest \
  --region=asia-east1 \
  --platform=managed \
  --allow-unauthenticated \
  --port=80
```

> 前端的 `VITE_API_BASE_URL` 需於 build 時注入，請在 `cloudbuild.yaml` 或本地 build 時帶入正式 Backend URL。

---

## 6. 常用維運指令

```bash
# 連線
gcloud sql connect sofycrm-db --user=sofycrm --database=sofycrm

# 備份
gcloud sql backups create --instance=sofycrm-db --description="manual-backup"

# 列出備份
gcloud sql backups list --instance=sofycrm-db

# 還原
gcloud sql backups restore BACKUP_ID --backup-instance=sofycrm-db --restore-instance=sofycrm-db

# 匯出整個資料庫到 GCS
gcloud sql export sql sofycrm-db \
  gs://YOUR_BUCKET_SOFYCRM_SQL/backup_$(date +%Y%m%d).sql \
  --database=sofycrm
```

---

## 7. 重置資料庫（危險）

```sql
-- 連到 sofycrm DB 後執行
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO sofycrm;
GRANT ALL ON SCHEMA public TO public;
```

之後重新匯入 `01_schema.sql` / `02_seed.sql`。
