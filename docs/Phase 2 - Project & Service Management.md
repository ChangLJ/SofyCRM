# CRM System SA - Phase 2 Project & Service Management

# Goal

建立：

- 專案管理
- Ticket 系統
- 客服支援
- 工時追蹤
- SLA 管理

---

# Project Module

## Features

### Project Dashboard

- Progress
- Milestones
- Tasks
- Bugs
- Acceptance

---

# Project Status

- Planning
- Development
- Testing
- UAT
- Completed
- Maintenance

---

# Database

## projects

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| project_name | varchar |
| PM_user_id | uuid |
| start_date | date |
| end_date | date |
| status | enum |

---

## project_tasks

| field | type |
|---|---|
| id | uuid |
| project_id | uuid |
| assigned_user_id | uuid |
| title | varchar |
| description | text |
| status | enum |
| estimated_hours | decimal |
| actual_hours | decimal |

---

# Ticket System

## Features

- Bug Report
- Feature Request
- Technical Support
- SLA Tracking

---

# Ticket Status

- Open
- Processing
- Waiting Customer
- Closed

---

# tickets

| field | type |
|---|---|
| id | uuid |
| customer_id | uuid |
| project_id | uuid |
| assigned_user_id | uuid |
| priority | enum |
| title | varchar |
| content | text |
| status | enum |

---

# SLA Rules

Priority:

- Low
- Medium
- High
- Critical

---

# Time Tracking

## work_logs

| field | type |
|---|---|
| id | uuid |
| project_id | uuid |
| user_id | uuid |
| work_date | date |
| hours | decimal |
| description | text |

---

# Permission Rules

## Sales

只能查看：

- 自己客戶專案
- 專案進度摘要

不能查看：

- 工時成本
- 技術細節

---

## Service

可查看：

- 被指派專案
- Ticket
- 工時

---

## Admin

全部可查看

---

# Frontend Routes

/projects
/projects/:id

/tickets
/tickets/:id

/worklogs

---

# Dashboard

## Service Dashboard

- Open Tickets
- SLA Warning
- Delayed Projects
- Pending Acceptance

---

# Notification System

事件通知：

- Ticket Assigned
- SLA Timeout
- Project Delayed
- Customer Reply

通知方式：

- In-App
- Email