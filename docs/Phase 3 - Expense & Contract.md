# CRM System SA - Phase 3 Expense & Contract Management

# Goal

建立：

- 報銷系統
- 合約管理
- 收款管理
- 發票管理

---

# Expense Module

## Features

- 建立報銷
- 上傳發票
- 審核流程
- 匯出報表

---

# Expense Categories

- Meal
- Transportation
- Parking
- Gift
- Hotel
- Other

---

# expenses

| field | type |
|---|---|
| id | uuid |
| user_id | uuid |
| customer_id | uuid |
| category | enum |
| amount | decimal |
| expense_date | date |
| receipt_url | varchar |
| description | text |
| status | enum |

---

# Approval Flow

Sales -> Admin -> Approved

---

# Contract Module

## Features

- Contract Upload
- Renewal Reminder
- NDA Management
- Maintenance Contract

---

# contracts

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| contract_name | varchar |
| start_date | date |
| end_date | date |
| renewal_notice_days | int |
| file_url | varchar |

---

# Invoice Module

## invoices

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| invoice_no | varchar |
| amount | decimal |
| due_date | date |
| payment_status | enum |

---

# Payment Status

- Pending
- Partial Paid
- Paid
- Overdue

---

# Financial Dashboard

## Admin

- Monthly Revenue
- Outstanding Payment
- Expense Statistics
- Profit Margin

---

# Reminder System

提醒：

- 合約到期
- 發票逾期
- 未收款
- 高額報銷

---

# Frontend Routes

/expenses
/contracts
/invoices