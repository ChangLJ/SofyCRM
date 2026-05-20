# CRM System SA - Phase 1 Foundation & CRM Core

# Project Overview

建立一套適合中小型軟體專案公司的 CRM 系統。

核心目標：

- 客戶管理
- 業務追蹤
- 權限控管
- 商機管理
- 報價管理
- Dashboard

系統角色：

- Admin
- Sales
- Service

---

# System Architecture

## Frontend

- Next.js
- React
- TypeScript
- TailwindCSS
- shadcn/ui
- Zustand
- React Query

## Backend

- NestJS
- Prisma ORM
- PostgreSQL
- JWT Authentication
- Redis (optional)

---

# RBAC Permission Design

## Roles

### Admin

可查看所有資料：

- 所有客戶
- 所有商機
- 所有報價
- 所有 Dashboard
- 使用者管理

### Sales

只能查看：

- 自己客戶
- 自己商機
- 自己報價

### Service

只能查看：

- 被指派客戶
- 被指派案件
- 工單資訊

---

# Authentication Module

## Features

### Login

- Email Login
- Password Login
- JWT Token
- Refresh Token

### User Management

Admin only：

- Create User
- Disable User
- Reset Password
- Assign Role

---

# Database Design

## users

| field | type |
|---|---|
| id | uuid |
| name | varchar |
| email | varchar |
| password_hash | varchar |
| role | enum |
| status | enum |
| created_at | timestamp |

---

# API Design

## Auth APIs

POST /api/v1/auth/login
POST /api/v1/auth/logout
POST /api/v1/auth/refresh
GET /api/v1/users/me

---

# Customer Module

## Features

### Customer List

搜尋條件：

- Company Name
- Contact Name
- Phone
- Email
- Tags

### Customer Detail

包含：

- 基本資訊
- 聯絡人
- 聯絡紀錄
- 商機
- 報價

---

# Customer Status

- Potential
- Contacting
- Quoting
- Won
- Lost
- Maintenance

---

# Database

## customers

| field | type |
|---|---|
| id | uuid |
| company_name | varchar |
| tax_id | varchar |
| address | text |
| industry | varchar |
| owner_user_id | uuid |
| status | enum |
| notes | text |
| created_at | timestamp |

---

## customer_contacts

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| name | varchar |
| title | varchar |
| phone | varchar |
| email | varchar |

---

# Followup Module

## Features

快速新增：

- Call
- Email
- Meeting
- Visit
- LINE

---

# Followup Database

## customer_followups

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| user_id | uuid |
| followup_type | enum |
| content | text |
| next_followup_date | timestamp |
| created_at | timestamp |

---

# Opportunity Module

## Features

銷售 Pipeline：

- New Lead
- Contacted
- Proposal
- Negotiation
- Won
- Lost

---

# opportunities

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| owner_user_id | uuid |
| title | varchar |
| amount | decimal |
| status | enum |
| expected_close_date | date |

---

# Quotation Module

## Features

- 建立報價
- 多版本
- PDF 匯出
- Email 發送

---

# quotations

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| quotation_no | varchar |
| version | int |
| total_amount | decimal |
| status | enum |

---

# quotation_items

| field | type |
|---|---|
| id | uuid |
| quotation_id | uuid |
| item_name | varchar |
| qty | int |
| unit_price | decimal |
| estimated_hours | decimal |

---

# Dashboard

## Admin Dashboard

- Monthly Revenue
- Total Opportunities
- Win Rate
- Top Sales

## Sales Dashboard

- My Opportunities
- Today's Followups
- Upcoming Deals

## Service Dashboard

- Assigned Customers
- Pending Tickets

---

# Frontend Routes

/login

/dashboard

/customers
/customers/:id

/opportunities

/quotations

/followups

/admin/users

---

# Non Functional Requirements

- Responsive Design
- Mobile Friendly
- JWT Security
- API Validation
- Audit Logs
- Pagination
- Search Filters

---

# Audit Log

## audit_logs

| field | type |
|---|---|
| id | uuid |
| user_id | uuid |
| module | varchar |
| action | varchar |
| before_data | json |
| after_data | json |
| created_at | timestamp |