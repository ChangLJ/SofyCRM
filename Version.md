# Version History

> 格式：`YYYYMMDD-XXX`（XXX 每日從 001 開始遞增）

---

## 20260518-001 — 初版專案骨架建立（前端 + 後端 + SQL + Docker）

**What**
- 依 `docs/Phase 0~5` 建立完整 SofyCRM 專案骨架。
- 新增 PostgreSQL Schema（Phase 1~3 全部資料表 + `audit_logs` + `sessions`）。
- 新增 C# .NET 8 + EF Core 後端：所有 Entity、DbContext、JWT + Session 驗證、RBAC、Audit.NET、所有模組 Controller。
- 新增 Vue 3 + Vite 前端：Tailwind + shadcn-vue + Motion-V + Lucide Vue、Pinia、Vue Router、所有 SA 規定的路由與頁面。
- Navbar 加入 `Mock / Real` 資料來源切換 Switch。
- Mock Data 已建立於 `frontend/src/mock/`。
- Docker Compose：`postgres`、`backend`、`frontend` 三服務。
- `Deploy.bat`：一鍵 build / restart 所有 Docker 容器。
- `sql_command.md`：GCP Cloud SQL 部署指令清單。

**Why**
- 使用者要求依 SA docs 完整生成前端 + 後端 + SQL 的初版專案。

**Related files**
- `docs/Phase 0 - Environment Settings.md`
- `docs/Phase 1 - Foundation & CRM Core.md`
- `docs/Phase 2 - Project & Service Management.md`
- `docs/Phase 3 - Expense & Contract.md`
- `docs/Phase 4 - Automation & AI.md`
- `docs/Phase 5 - Final Settings.md`
- `sql/01_schema.sql`, `sql/02_seed.sql`
- `backend/**`
- `frontend/**`
- `docker-compose.yml`, `Deploy.bat`, `sql_command.md`, `README.md`

---

## 20260518-002 — 修正部署後未登入即可進首頁、Mock/Real 切換造成 401

**What**
- 將 `useDataSourceStore` 預設值由 `'mock'` 改為 `'real'`（部署情境合理預設）。
- 移除 `router/index.ts` 內 `mock 模式自動建立 session` 的邏輯，**未登入一律導向 `/login`**。
- `useDataSourceStore.toggle()` / `set()` 改為 async：切換來源時若已登入會自動 `auth.logout()`，避免 mock token 撞 real API（或反之）造成 401。
- `Navbar.vue` 切換 Switch 後會主動導向 `/login`，請使用者用對應端帳號重新登入。

**Why**
- 使用者回報：Deploy 之後直接進到首頁 → 觸發 `GET /api/v1/...` 收到 401 Unauthorized。
- 原因：原本預設 `mock` 模式 + router 自動建立的 `mock-access-token` 殘留於 localStorage，切到 real 模式後該 token 不被後端認可。

**Related files**
- `frontend/src/stores/dataSource.ts`
- `frontend/src/router/index.ts`
- `frontend/src/components/layout/Navbar.vue`

---

## 20260518-003 — Repository 介面層 + 全模組 CRUD + Mock 身份切換 + Token 檢查 + Admin 帳號

**What**

1. **Repository Interface 層（Task 5）**
   - 新增 `frontend/src/repositories/types.ts`：定義 `IRepos` 與各模組介面（`ICustomerRepo` / `IOpportunityRepo` / ... 共 14 個 repo）。
   - 新增 `frontend/src/repositories/api.ts`：真實 API 實作（呼叫 `/api/v1/*`）。
   - 新增 `frontend/src/repositories/mock.ts`：本地 mock 實作（CRUD 完整支援、approve/reject、tasks/contacts 副資源）。
   - 新增 `frontend/src/repositories/index.ts`：`useRepos()` / `getRepos()` 依當前 `useDataSourceStore` 自動派發 mock 或 api 實作。
   - 重寫 `frontend/src/api/endpoints.ts` 為 Proxy 轉發層 — 確保 mock 與 real 永遠對齊同一份介面。
   - `stores/auth.ts` 改為透過 repos 取得 auth；新增 `verifyToken()` action。

2. **Real Data 新增帳號 Admin / Admin@123（Task 4）**
   - 後端 `LoginRequestDto`：移除 `[EmailAddress]` 限制，允許純字串帳號（仍保持必填）。
   - `DbInitializer.cs`：自動補建 `email='admin', password='Admin@123', role=Admin`。
   - `sql/02_seed.sql`：寫入 `Admin` 簡易帳號（含預生的 BCrypt 雜湊）。
   - `Login.vue`：帳號欄位由 `type="email"` 改為 `type="text"`、預設值 `Admin / Admin@123`。

3. **Real 模式 Token 檢查（Task 3）**
   - `auth.verifyToken()`：若 token 過期或後端回 401 → 自動 `logout()`。
   - `router/index.ts`：Real 模式進入受保護頁面前一律 `verifyToken()`；Mock 模式跳過。

4. **Mock 模式選擇身份（Task 1）**
   - `Login.vue`：Mock 模式時顯示 3 顆「Admin / Sales / Service」一鍵登入卡片；Real 模式隱藏。
   - `Navbar.vue`：Mock 模式時加入「Mock 身份」快速切換 segmented control（Admin / Sales / Service），切換不需登出再登入；Real 模式完全隱藏。

5. **各頁 CRUD（Task 2）**
   - 新增 UI 元件：`Dialog.vue`（含 motion-v 動畫）、`Select.vue`、`Textarea.vue`、`Label.vue`、`ConfirmDialog.vue`。
   - 新增 `components/common/RowActions.vue`：通用「編輯 / 刪除」按鈕，**刪除按鈕僅 Admin 可見**（`auth.isAdmin`）。
   - 所有列表頁 (Customers, Opportunities, Quotations, Followups, Projects, Tickets, WorkLogs, Expenses, Contracts, Invoices, AdminUsers) 全部加入：
     - 工具列「+ 新增」按鈕 + 對應編輯 Dialog
     - 列尾 `_actions` 欄：編輯 / 刪除 (Admin)
     - `ConfirmDialog` 二次確認
   - `Expenses.vue`：Admin 額外有「核准 / 退回」按鈕（針對 Submitted）。
   - `AdminUsers.vue`：另含「重設密碼」對話框。

**Why**
1. 介面層避免 Mock 與 Real 行為漂移、view 不再有 `if (isMock) ... else ...` 的判斷散落。
2. Admin/Admin@123 為使用者要求的簡易 demo 帳號。
3. Real 模式下若 token 失效仍能訪問畫面會造成 401 與糟糕 UX，需在進入時主動驗證。
4. 展示時可一鍵切換身份觀察不同角色看到的資料 / 操作差異。
5. SA 規定各模組都應具備 CRUD，且應符合 RBAC（刪除為高權限動作 → Admin 才行）。

**Related files**
- `frontend/src/repositories/{types,api,mock,index}.ts`
- `frontend/src/api/endpoints.ts`
- `frontend/src/stores/auth.ts`
- `frontend/src/router/index.ts`
- `frontend/src/views/Login.vue`
- `frontend/src/components/layout/Navbar.vue`
- `frontend/src/components/ui/{Dialog,Select,Textarea,Label,ConfirmDialog,index}.vue`/`.ts`
- `frontend/src/components/common/RowActions.vue`
- `frontend/src/views/**`（所有列表頁）
- `backend/SofyCRM.Api/Dtos/Dtos.cs`
- `backend/SofyCRM.Api/Data/DbInitializer.cs`
- `sql/02_seed.sql`

## 20260518-004 — 表單體驗 / 中文化 / 客戶選單 / 自動帶人 / 報價單號 / 合約負責人 / 通知鈴鐺

**What**
1. **全域 — 表單與彈窗**
   - 所有 `Dialog` 移除「ESC / 背景點擊」關閉，改為**只能透過 [取消] 或 [X] 按鈕關閉**，避免誤觸丟失輸入。
   - 各 view 新增前端必填驗證（`errors` map + `validate()`）；`Input` / `Select` / `Textarea` 新增 `:error` prop（紅框 + 紅字提示）；未通過驗證時直接 `return`，不會儲存。
   - 新增 `frontend/src/lib/labels.ts`：集中管理 CustomerStatus / FollowupType / OpportunityStatus / QuotationStatus / ProjectStatus / TaskStatus / TicketStatus / TicketPriority / ExpenseCategory / ExpenseStatus / PaymentStatus / UserRole 12 組中文對照表 + 對應 `<Select>` options，全站 Badge / Select 都改顯示中文。

2. **共用查表 `useLookups`**
   - 新增 `frontend/src/composables/useLookups.ts`：`loadCustomers / loadUsers / loadProjects` + 對應 `customerOptions / userOptions / projectOptions`，供各 view 注入下拉選單資料。
   - 後端 `UsersController.List`：放寬 Authorize Roles，所有已登入者可讀取（僅給選單使用，不影響 admin 寫入）。

3. **各模組 — 客戶選單 / 自動帶人 / 驗證**
   - **客戶**：自動帶入 `ownerUserId = currentUser`、`createdAt = now`；負責人下拉；建立時間欄位顯示。
   - **聯絡紀錄**：客戶下拉 + 經辦下拉（預設 = currentUser）+ 時間自動填。
   - **商機**：客戶下拉 + 負責人下拉（預設 = currentUser）。
   - **報價**：**報價單號改由系統自動生成**規則 `PO-yyyyMMdd-XXX`（每日 001 起跳遞增），UI 顯示為 readonly；無客戶選單欄位、客戶必填。
     - mock：在 `repositories/mock.ts` 計算當日已有的同 prefix 數量產生下一個流水號。
     - backend：`QuotationsController.Create` 改為 `count(today) + 1` 後綴。
   - **專案**：客戶下拉 + PM 下拉（預設 = currentUser）。
   - **Ticket**：客戶下拉 + 負責人下拉（預設 = currentUser）。
   - **工時**：專案下拉 + 人員下拉（預設 = currentUser）。
   - **報銷**：自動帶入申請人；**新增時狀態強制為 Draft 且 Select disabled**；編輯模式可改狀態，Sales 限 Draft/Submitted，Admin 全開；增加客戶下拉（選填）。
   - **合約**：新增「負責人」欄位 (`ownerUserId`)；客戶下拉必填。
   - **發票**：客戶下拉必填。

4. **合約到期通知系統**
   - 後端：`Contract` entity + `01_schema.sql` 新增 `owner_user_id`（含 `ALTER TABLE ... ADD COLUMN IF NOT EXISTS`，舊資料庫可平滑升級）。
   - 前端 `mock/index.ts`：
     - 新增 2 筆合約 — `ct3` 15 天後到期、`ct4` 已過期 5 天，用以觸發通知。
     - 新增 `buildContractNotificationsFor(userId)`：依目前 mock 合約「即將到期 / 已過期未續」動態產生通知（每位通知對象皆會收到，包含合約 owner + Admin）。
   - 前端 `repositories/mock.ts`：merge 靜態 + 動態通知；以 `localStorage('sofycrm.mock.readNotifications')` 持久化已讀狀態。
   - 新增 `stores/notifications.ts`：unread count、`refresh()`、`markRead`、`markAllRead`；每 60 秒輪詢。
   - 新增 `components/layout/NotificationBell.vue`：
     - 有未讀時鈴鐺改為 `BellRing` + **紅點 `animate-ping` 脈動** + 右上角 unread 數字徽章
     - 點擊展開下拉面板（motion-v 動畫）— 顯示通知列表、未讀醒目背景、`ContractExpiring` 黃色 / `ContractOverdue` 紅色圖示
     - 點擊單筆 → `markRead(id)` 後 router push 連結；亦提供「全部已讀」按鈕
   - `Navbar.vue` 以 `NotificationBell` 取代原本的純 `<Bell>` icon。

**Why**
1. 使用者反映 MockData 測試時：必填空值可儲存、誤點背景就把表單關掉、enum 顯示英文不直觀 → 一次補齊全域 UX。
2. CRM 表單高頻動作（建立客戶、聯絡、商機…）一律應該自動代入「現在登入者 + 現在時間」，避免使用者逐欄填寫；同時客戶為核心 FK 必須顯式下拉、不能拍腦袋打字。
3. 報價單號是對外文件編號，應由系統依日期遞增；前端可編輯會破壞唯一性與審計，後端應為唯一來源。
4. 合約一個月內到期 / 已逾期未續是高商業價值的告警，鈴鐺常駐 Navbar + 紅色脈動才能拉住注意力；已讀狀態需持久化避免每次重整都跳出。

**Related files**
- `frontend/src/lib/labels.ts`（新）
- `frontend/src/composables/useLookups.ts`（新）
- `frontend/src/stores/notifications.ts`（新）
- `frontend/src/components/layout/NotificationBell.vue`（新）
- `frontend/src/components/ui/{Dialog,Input,Select,Textarea}.vue`
- `frontend/src/components/layout/Navbar.vue`
- `frontend/src/repositories/mock.ts`
- `frontend/src/mock/index.ts`
- `frontend/src/types/models.ts`（Contract 加 `ownerUserId/ownerUser`）
- `frontend/src/views/customers/CustomerList.vue`
- `frontend/src/views/Followups.vue`
- `frontend/src/views/Opportunities.vue`
- `frontend/src/views/Quotations.vue`
- `frontend/src/views/Projects.vue`
- `frontend/src/views/Tickets.vue`
- `frontend/src/views/WorkLogs.vue`
- `frontend/src/views/Expenses.vue`
- `frontend/src/views/Contracts.vue`
- `frontend/src/views/Invoices.vue`
- `backend/SofyCRM.Api/Entities/Models.cs`（Contract OwnerUserId）
- `backend/SofyCRM.Api/Data/AppDbContext.cs`（Contract Owner FK）
- `backend/SofyCRM.Api/Controllers/UsersController.cs`（List 放寬授權）
- `backend/SofyCRM.Api/Controllers/QuotationsController.cs`（PO-yyyyMMdd-XXX）
- `sql/01_schema.sql`（contracts.owner_user_id + ALTER TABLE 補欄位）

## 20260518-005 — Dashboard 商機中文化 + 報銷列表「送出」按鈕

**What**
1. **Dashboard 商機 Pipeline 中文化**
   - `Dashboard.vue` import `opportunityStatusLabel`，新增 `statusLabel(s)` helper；商機 Pipeline 卡片的 `row.status` 改顯示中文（新名單 / 已接觸 / 提案中 / 議價中 / 已成交 / 已失單），保留 fallback 避免後端回未知 enum 時崩潰。

2. **報銷列表新增「送出」按鈕**
   - `Expenses.vue` 新增 `submitDraft(row)` action：把 `status: 'Draft'` 改 `'Submitted'` 後重新載入。
   - 加 `canSubmit(row)` 條件：`status === 'Draft'` 且（自己是該報銷申請人 或 Admin）才顯示。
   - UI：列尾 `_actions` 欄在「核准 / 退回」按鈕前面再加一顆 `Send` icon（藍色），與其他操作按鈕對齊。
   - 因此完整動作鏈為：草稿（Draft）→ 點送出 → 已送出（Submitted）→ Admin 核准/退回 → 已核准 / 已退回。

**Why**
1. Pipeline 在改完 Badge/Select 顯示中文後仍漏掉 Dashboard 圖卡裡的「商機狀態」字串 — 使用者反映依然顯示英文 enum。
2. 上一版報銷新增固定為 Draft 後，Sales 自己沒有任何 UI 把它改成 Submitted（要點進編輯才能改），不直覺 — 加一顆列表上的快速送出按鈕讓流程一鍵完成。

**Related files**
- `frontend/src/views/Dashboard.vue`
- `frontend/src/views/Expenses.vue`

## 20260518-006 — 預設 Mock + Admin 稽核紀錄頁

**What**
1. **預設 Data Source 改回 Mock**
   - `frontend/src/stores/dataSource.ts` 的 `loadMode()`：首次使用（localStorage 無記錄）時回傳 `'mock'`，避免一開啟就打到還沒登入的真實 API 而出現 401。
   - 一旦使用者切到 Real / 登入 Real 帳號後，localStorage 會記下選擇，下次重新整理仍維持 Real。

2. **新增 Audit Log（稽核紀錄）功能**
   - 新增 `frontend/src/types/models.ts` 的 `AuditLog` 介面（id / user / module / action / entityId / before / after / ip / userAgent / createdAt）。
   - 新增 `IAuditLogRepo` 介面與兩端實作：
     - **Real**：`GET /api/v1/audit-logs?module&action&page&pageSize`（backend `AuditLogsController` 已存在，Admin only）。
     - **Mock**：`mockAuditLogs` 15 筆 demo（涵蓋 Auth / Customer / Opportunity / Quotation / Ticket / User / Expense / Contract / Invoice / WorkLog 各模組與 CRUD / Submit / Approve / ResetPassword / Login 等動作），支援 module / action / keyword 篩選。
   - 新增 `frontend/src/views/admin/AuditLogs.vue`：
     - **篩選列**：關鍵字（操作者/對象ID/模組）+ 模組下拉 + 動作下拉，套用後 reload。
     - **資料表**：時間 / 操作者 / 模組（中文 Badge）/ 動作（中文 Badge 配色：Create 綠 / Update 藍 / Delete 紅 / Approve 綠 / Reject 黃 / ResetPassword 黃）/ 對象 ID（monospace）/ IP。
     - **變更檢視 Dialog**：點「檢視」開啟左右分欄，顯示 before / after 的 JSON 美化內容。
     - **分頁器**：上一頁 / 下一頁 + 顯示「共 N 筆 · 目前 a–b 筆」。
   - 路由 `'/admin/audit-logs'` (`meta.roles: ['Admin']`)、Sidebar 加「稽核紀錄」連結（`ScrollText` icon，僅 Admin 可見）。

**Why**
1. 上一版預設改 Real 雖能避免 mock 假 token 打到真 API 的問題，但首次部署/沒設好後端時會直接出 401 嚇到使用者。改回 Mock 是合理的 demo 預設，使用者可隨時透過 Navbar 切到 Real。
2. SA 規定所有重要動作需可稽核，這是 Admin 排查問題、追溯責任時的重要工具，先以 view + repository pattern 接好（後端 API 已就緒），mock 直接給 demo 紀錄方便不連 DB 也能展示。

**Related files**
- `frontend/src/stores/dataSource.ts`
- `frontend/src/types/models.ts`（+ AuditLog）
- `frontend/src/repositories/types.ts`（+ IAuditLogRepo / IRepos.auditLogs）
- `frontend/src/repositories/api.ts`（+ auditLogsRepo）
- `frontend/src/repositories/mock.ts`（+ auditLogsRepo + 篩選邏輯）
- `frontend/src/mock/index.ts`（+ mockAuditLogs 15 筆）
- `frontend/src/views/admin/AuditLogs.vue`（新）
- `frontend/src/router/index.ts`（+ /admin/audit-logs 路由）
- `frontend/src/components/layout/Sidebar.vue`（+ 稽核紀錄選單）

## 20260519-001 — 固定 Mock Data 模式（停用 Real API 切換）

**What**
1. **資料來源固定為 Mock**
   - `stores/dataSource.ts`：`isMock` 恆為 `true`、`toggle()` / `set()` 改為 no-op，`init()` 寫入 `localStorage = mock`。
   - `repositories/index.ts`：`useRepos()` / `getRepos()` 永遠回傳 `mockRepos`，不再載入 `apiRepos`。
   - `router/index.ts`：移除 Real 模式 `verifyToken()` 守衛（Mock 不需打後端）。

2. **UI 移除切換開關**
   - `Navbar.vue`：移除「Mock Data / Real API」Switch 區塊；保留 Admin / Sales / Service 身份快速切換。
   - `Login.vue`：移除底部資料來源 Switch 與 Real 帳號說明；登入流程一律走 Mock（一鍵身份 + 表單登入）。

3. **啟動整理**
   - `sessionBootstrap.ts`：固定 mock，清除殘留的非 mock token。

**Why**
使用者要求停用 Mock/Real 切換、只使用 Mock Data，避免 Docker 部署後仍因 localStorage 殘留 `real` 模式或過期 JWT 而對後端發出 401 請求。

**Related files**
- `frontend/src/stores/dataSource.ts`
- `frontend/src/repositories/index.ts`
- `frontend/src/router/index.ts`
- `frontend/src/lib/sessionBootstrap.ts`
- `frontend/src/components/layout/Navbar.vue`
- `frontend/src/views/Login.vue`

## 20260520-001 — Cloud Run 前端 Mock 部署 + GitHub 上傳

**What**
1. **Cloud Run 專用前端映像**：`frontend/Dockerfile` + `nginx.conf.template`（監聽 `${PORT}`，Cloud Run 用 8080）。
2. **部署**：`DeployCloudRun.bat`、`cloudbuild.yaml`、`DEPLOY_CLOUDRUN.md`。
3. **docker-compose** 本機前端 `PORT=80`。

**Why**
部署前端 + Mock Data 至 GCP Cloud Run，並上傳至 GitHub。

**Related files**
- `frontend/Dockerfile`, `frontend/nginx.conf.template`
- `cloudbuild.yaml`, `DeployCloudRun.bat`, `DEPLOY_CLOUDRUN.md`
- `docker-compose.yml`
