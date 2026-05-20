/**
 * Real API Repository 實作 — 透過 axios http 與後端 /api/v1 溝通
 */
import { http } from '@/api/http'
import type {
  IRepos, IAuthRepo, IUserRepo, ICustomerRepo, IFollowupRepo, IOpportunityRepo,
  IQuotationRepo, IProjectRepo, ITicketRepo, IWorkLogRepo, IExpenseRepo,
  IContractRepo, IInvoiceRepo, IDashboardRepo, INotificationRepo, IAuditLogRepo,
  Page, QueryParams,
} from './types'
import type {
  User, Customer, CustomerContact, CustomerFollowup, Opportunity, Quotation,
  Project, ProjectTask, Ticket, WorkLog, Expense, Contract, Invoice,
  DashboardSummary, Notification, AuditLog,
} from '@/types/models'

function crud<T>(base: string) {
  return {
    list:   (params?: QueryParams) => http.get<Page<T>>(base, { params }).then(r => r.data),
    get:    (id: string)           => http.get<T>(`${base}/${id}`).then(r => r.data),
    create: (dto: Partial<T>)      => http.post<T>(base, dto).then(r => r.data),
    update: (id: string, dto: Partial<T>) => http.put<T>(`${base}/${id}`, dto).then(r => r.data),
    remove: (id: string)           => http.delete(`${base}/${id}`).then(() => undefined),
  }
}

const authRepo: IAuthRepo = {
  async login(email, password) {
    const { data } = await http.post('/auth/login', { email, password })
    return data
  },
  async logout() { /* handled at store layer */ },
  async me() {
    const { data } = await http.get<User>('/users/me')
    return data
  },
}

const usersRepo: IUserRepo = {
  ...crud<User>('/users'),
  resetPassword: (id, newPassword) => http.post(`/users/${id}/reset-password`, { newPassword }).then(() => undefined),
}

const customersRepo: ICustomerRepo = {
  ...crud<Customer>('/customers'),
  contacts:       (id) => http.get<CustomerContact[]>(`/customers/${id}/contacts`).then(r => r.data),
  addContact:     (id, dto) => http.post<CustomerContact>(`/customers/${id}/contacts`, dto).then(r => r.data),
  deleteContact:  (cId) => http.delete(`/customers/contacts/${cId}`).then(() => undefined),
}

const followupsRepo:     IFollowupRepo     = crud<CustomerFollowup>('/followups')
const opportunitiesRepo: IOpportunityRepo  = crud<Opportunity>('/opportunities')
const quotationsRepo:    IQuotationRepo    = crud<Quotation>('/quotations')

const projectsRepo: IProjectRepo = {
  ...crud<Project>('/projects'),
  tasks:        (id) => http.get<ProjectTask[]>(`/projects/${id}/tasks`).then(r => r.data),
  addTask:      (id, dto) => http.post<ProjectTask>(`/projects/${id}/tasks`, dto).then(r => r.data),
  updateTask:   (tid, dto) => http.put<ProjectTask>(`/projects/tasks/${tid}`, dto).then(r => r.data),
  deleteTask:   (tid) => http.delete(`/projects/tasks/${tid}`).then(() => undefined),
}

const ticketsRepo:   ITicketRepo  = crud<Ticket>('/tickets')
const workLogsRepo:  IWorkLogRepo = crud<WorkLog>('/worklogs')

const expensesRepo: IExpenseRepo = {
  ...crud<Expense>('/expenses'),
  approve: (id) => http.post<Expense>(`/expenses/${id}/approve`).then(r => r.data),
  reject:  (id) => http.post<Expense>(`/expenses/${id}/reject`).then(r => r.data),
}

const contractsRepo: IContractRepo = crud<Contract>('/contracts')
const invoicesRepo:  IInvoiceRepo  = crud<Invoice>('/invoices')

const dashboardRepo: IDashboardRepo = {
  summary: () => http.get<DashboardSummary>('/dashboard/summary').then(r => r.data),
}

const notificationsRepo: INotificationRepo = {
  list:        (params) => http.get<Page<Notification>>('/notifications', { params }).then(r => r.data),
  markRead:    (id) => http.post(`/notifications/${id}/read`).then(() => undefined),
  markAllRead: ()   => http.post('/notifications/read-all').then(() => undefined),
}

const auditLogsRepo: IAuditLogRepo = {
  list: (params) => http.get<Page<AuditLog>>('/audit-logs', { params }).then(r => r.data),
}

export const apiRepos: IRepos = {
  auth: authRepo,
  users: usersRepo,
  customers: customersRepo,
  followups: followupsRepo,
  opportunities: opportunitiesRepo,
  quotations: quotationsRepo,
  projects: projectsRepo,
  tickets: ticketsRepo,
  worklogs: workLogsRepo,
  expenses: expensesRepo,
  contracts: contractsRepo,
  invoices: invoicesRepo,
  dashboard: dashboardRepo,
  notifications: notificationsRepo,
  auditLogs: auditLogsRepo,
}
