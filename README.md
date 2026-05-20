# SofyCRM

> 中小型軟體專案公司的 CRM 系統 — 涵蓋客戶 / 商機 / 報價 / 專案 / Ticket / 報銷 / 合約 / 發票 / Dashboard / Audit。

依據 `docs/` 內的 SA 文件實作，Phase 0 為總環境設定，Phase 1~4 為功能模組，Phase 5 為最終整備。

---

## 技術棧

| 層級 | 技術 |
| --- | --- |
| 容器 | Docker / Docker Compose |
| 前端 | Vue 3 + Vite + TypeScript |
| UI | Tailwind CSS + shadcn-vue + Motion-V + Lucide Vue |
| 後端 | C# (.NET 8) + EF Core |
| Audit | EFCore.NamingConventions + Audit.NET |
| 資料庫 | PostgreSQL 16 |
| 驗證 | JWT + Session Table |
| 雲端 | GCP Cloud Run + Cloud SQL |

---

## 目錄結構

```
SofyCRM/
├── docs/                      SA 文件 (Phase 0~5)
├── sql/                       PostgreSQL Schema / Seed
│   ├── 01_schema.sql
│   └── 02_seed.sql
├── backend/                   C# .NET 8 Web API
│   ├── SofyCRM.Api/
│   └── Dockerfile
├── frontend/                  Vue 3 + Vite
│   ├── src/
│   └── Dockerfile
├── docker-compose.yml
├── Deploy.bat                 一鍵 Build / Push / Restart
├── sql_command.md             GCP Cloud SQL 部署指令
├── Version.md                 版本記錄
└── README.md
```

---

## 快速啟動 (Local Docker)

```bash
docker compose up -d --build
```

| 服務 | URL |
| --- | --- |
| Frontend | http://localhost:5173 |
| Backend API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| PostgreSQL | localhost:5432 (sofycrm / sofycrm / sofycrm) |

預設帳號（由 `02_seed.sql` 寫入）：

| 角色 | Email | 密碼 |
| --- | --- | --- |
| Admin | admin@sofycrm.local | Admin@123 |
| Sales | sales@sofycrm.local | Sales@123 |
| Service | service@sofycrm.local | Service@123 |

---

## RBAC

| 角色 | 權限 |
| --- | --- |
| Admin | 全部資料 + 使用者管理 |
| Sales | 自己客戶 / 商機 / 報價 |
| Service | 被指派客戶 / 案件 / 工單 |

---

## Mock / Real Data 切換

Navbar 右上角提供 `Mock ⇄ Real` Switch，存於 Pinia store `useDataSourceStore`：

- `Mock`：直接讀取 `src/mock/*.ts` 靜態資料，可離線 demo。
- `Real`：呼叫 `/api/v1/*` RESTful API。

---

## 部署

- **本機 / 私有環境**：`Deploy.bat`
- **GCP Cloud Run + Cloud SQL**：依 `sql_command.md` 操作
