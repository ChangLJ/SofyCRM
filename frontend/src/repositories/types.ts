/**
 * Repository Interfaces — View 與 Data Source 之間的抽象層。
 *
 * 設計目標：
 *  - View 只依賴 Repository 介面，不直接知道資料來自 mock 或真 API。
 *  - Mock 與 Real 兩種實作必須符合 *同一份介面*，避免行為漂移。
 *
 * 使用方式：在 component 中
 *   const repos = useRepos()
 *   const customers = await repos.customers.list()
 */

import type {
  User, Customer, CustomerContact, CustomerFollowup, Opportunity, Quotation,
  Project, ProjectTask, Ticket, WorkLog, Expense, Contract, Invoice,
  DashboardSummary, Notification, AuditLog,
} from '@/types/models'

export interface Page<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface QueryParams {
  keyword?: string
  status?: string
  page?: number
  pageSize?: number
  [k: string]: unknown
}

// ---- 通用 CRUD 介面（List + Get + Create + Update + Remove） ---------------
export interface ICrudRepo<T, TCreate = Partial<T>, TUpdate = Partial<T>> {
  list(params?: QueryParams): Promise<Page<T>>
  get(id: string): Promise<T | undefined>
  create(dto: TCreate): Promise<T>
  update(id: string, dto: TUpdate): Promise<T>
  remove(id: string): Promise<void>
}

// ---- 各模組 Repository -----------------------------------------------------
export interface IAuthRepo {
  login(email: string, password: string): Promise<{ accessToken: string; refreshToken: string; expiresAt: string; user: User }>
  logout(): Promise<void>
  me(): Promise<User>
}

export interface IUserRepo extends ICrudRepo<User> {
  resetPassword(id: string, newPassword: string): Promise<void>
}

export interface ICustomerRepo extends ICrudRepo<Customer> {
  contacts(id: string): Promise<CustomerContact[]>
  addContact(id: string, dto: Partial<CustomerContact>): Promise<CustomerContact>
  deleteContact(contactId: string): Promise<void>
}

export interface IFollowupRepo extends ICrudRepo<CustomerFollowup> {}
export interface IOpportunityRepo extends ICrudRepo<Opportunity> {}
export interface IQuotationRepo  extends ICrudRepo<Quotation> {}

export interface IProjectRepo extends ICrudRepo<Project> {
  tasks(id: string): Promise<ProjectTask[]>
  addTask(id: string, dto: Partial<ProjectTask>): Promise<ProjectTask>
  updateTask(taskId: string, dto: Partial<ProjectTask>): Promise<ProjectTask>
  deleteTask(taskId: string): Promise<void>
}

export interface ITicketRepo extends ICrudRepo<Ticket> {}
export interface IWorkLogRepo extends ICrudRepo<WorkLog> {}

export interface IExpenseRepo extends ICrudRepo<Expense> {
  approve(id: string): Promise<Expense>
  reject(id: string): Promise<Expense>
}

export interface IContractRepo extends ICrudRepo<Contract> {}
export interface IInvoiceRepo extends ICrudRepo<Invoice> {}

export interface IDashboardRepo {
  summary(): Promise<DashboardSummary>
}

export interface INotificationRepo {
  list(params?: QueryParams): Promise<Page<Notification>>
  markRead(id: string): Promise<void>
  markAllRead(): Promise<void>
}

export interface IAuditLogRepo {
  list(params?: QueryParams): Promise<Page<AuditLog>>
}

// ---- Repository 總集 -------------------------------------------------------
export interface IRepos {
  auth:          IAuthRepo
  users:         IUserRepo
  customers:     ICustomerRepo
  followups:     IFollowupRepo
  opportunities: IOpportunityRepo
  quotations:    IQuotationRepo
  projects:      IProjectRepo
  tickets:       ITicketRepo
  worklogs:      IWorkLogRepo
  expenses:      IExpenseRepo
  contracts:     IContractRepo
  invoices:      IInvoiceRepo
  dashboard:     IDashboardRepo
  notifications: INotificationRepo
  auditLogs:     IAuditLogRepo
}
