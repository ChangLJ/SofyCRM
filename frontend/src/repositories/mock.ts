/**
 * Mock Repository 實作 — 全部從 `@/mock/index.ts` 的本地陣列讀寫，
 * 操作後仍同步存在 in-memory，下次重新整理會還原（除非寫到 localStorage）。
 * 與 `apiRepos` 對齊 IRepos 介面，view 不需感知差異。
 */
import * as mock from '@/mock'
import type {
  IRepos, IAuthRepo, IUserRepo, ICustomerRepo, IFollowupRepo, IOpportunityRepo,
  IQuotationRepo, IProjectRepo, ITicketRepo, IWorkLogRepo, IExpenseRepo,
  IContractRepo, IInvoiceRepo, IDashboardRepo, INotificationRepo, IAuditLogRepo,
  Page, QueryParams,
} from './types'
import type {
  User, Customer, CustomerContact, CustomerFollowup, Opportunity, Quotation,
  Project, ProjectTask, Ticket, WorkLog, Expense, Contract, Invoice,
  Notification, AuditLog,
} from '@/types/models'

const uid = () =>
  (crypto?.randomUUID?.() ?? Math.random().toString(36).slice(2) + Date.now().toString(36))

function page<T>(items: T[], params?: QueryParams): Page<T> {
  const p  = params?.page ?? 1
  const ps = params?.pageSize ?? 50
  return { items: items.slice((p - 1) * ps, p * ps), total: items.length, page: p, pageSize: ps }
}

function makeCrud<T extends { id: string }>(arr: T[]) {
  return {
    list:   async (params?: QueryParams) => page(arr, params),
    get:    async (id: string) => arr.find(x => x.id === id),
    create: async (dto: Partial<T>) => {
      const row = { ...(dto as T), id: uid() }
      arr.unshift(row); return row
    },
    update: async (id: string, dto: Partial<T>) => {
      const i = arr.findIndex(x => x.id === id)
      if (i < 0) throw new Error('Not found')
      arr[i] = { ...arr[i], ...(dto as T) }
      return arr[i]
    },
    remove: async (id: string) => {
      const i = arr.findIndex(x => x.id === id)
      if (i >= 0) arr.splice(i, 1)
    },
  }
}

const authRepo: IAuthRepo = {
  async login(email) {
    // Mock 模式：依 email prefix 推測角色
    const role: User['role'] =
      email.toLowerCase().startsWith('sales')   ? 'Sales'   :
      email.toLowerCase().startsWith('service') ? 'Service' : 'Admin'
    const user: User = {
      id: role === 'Admin' ? '11111111-1111-1111-1111-111111111111'
        : role === 'Sales' ? '22222222-2222-2222-2222-222222222222'
        : '33333333-3333-3333-3333-333333333333',
      name: role === 'Admin' ? '系統管理員 (Mock)'
          : role === 'Sales' ? '王業務 (Mock)'
          : '陳客服 (Mock)',
      email, role, status: 'Active',
    }
    return {
      accessToken:  'mock-access-token',
      refreshToken: 'mock-refresh-token',
      expiresAt:    new Date(Date.now() + 3600_000).toISOString(),
      user,
    }
  },
  async logout() { /* no-op */ },
  async me() {
    return mock.mockUsers[0]
  },
}

const usersRepo: IUserRepo = {
  ...makeCrud<User>(mock.mockUsers),
  resetPassword: async () => { /* no-op */ },
}

const customersRepo: ICustomerRepo = {
  ...makeCrud<Customer>(mock.mockCustomers),
  contacts: async (id) => mock.mockContacts.filter(c => c.customerId === id),
  addContact: async (id, dto) => {
    const partial = (dto ?? {}) as Partial<CustomerContact>
    const row: CustomerContact = {
      name:      partial.name ?? '',
      title:     partial.title ?? null,
      phone:     partial.phone ?? null,
      email:     partial.email ?? null,
      isPrimary: partial.isPrimary ?? false,
      id:        uid(),
      customerId: id,
    }
    mock.mockContacts.unshift(row)
    return row
  },
  deleteContact: async (cid) => {
    const i = mock.mockContacts.findIndex(x => x.id === cid)
    if (i >= 0) mock.mockContacts.splice(i, 1)
  },
}

const followupsRepo:     IFollowupRepo    = makeCrud<CustomerFollowup>(mock.mockFollowups)
const opportunitiesRepo: IOpportunityRepo = makeCrud<Opportunity>(mock.mockOpportunities)

// 報價單號規則：PO-yyyyMMdd-XXX（每日 001 起跳）— 由 mock repo 強制生成
function nextQuotationNo(): string {
  const d = new Date()
  const yyyymmdd = `${d.getFullYear()}${String(d.getMonth() + 1).padStart(2, '0')}${String(d.getDate()).padStart(2, '0')}`
  const prefix = `PO-${yyyymmdd}-`
  const todayCount = mock.mockQuotations.filter(q => q.quotationNo?.startsWith(prefix)).length
  return `${prefix}${String(todayCount + 1).padStart(3, '0')}`
}

const quotationsRepo: IQuotationRepo = {
  list:   async (params) => page(mock.mockQuotations, params),
  get:    async (id) => mock.mockQuotations.find(x => x.id === id),
  create: async (dto) => {
    const row: Quotation = {
      ...(dto as Quotation),
      id: uid(),
      quotationNo: nextQuotationNo(),
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    }
    mock.mockQuotations.unshift(row)
    return row
  },
  update: async (id, dto) => {
    const i = mock.mockQuotations.findIndex(x => x.id === id)
    if (i < 0) throw new Error('Not found')
    // 不允許修改 quotationNo
    const { quotationNo: _ignore, ...rest } = (dto as Partial<Quotation>)
    mock.mockQuotations[i] = { ...mock.mockQuotations[i], ...rest, updatedAt: new Date().toISOString() }
    return mock.mockQuotations[i]
  },
  remove: async (id) => {
    const i = mock.mockQuotations.findIndex(x => x.id === id)
    if (i >= 0) mock.mockQuotations.splice(i, 1)
  },
}

const projectsRepo: IProjectRepo = {
  ...makeCrud<Project>(mock.mockProjects),
  tasks: async (id) => mock.mockTasks.filter(t => t.projectId === id),
  addTask: async (id, dto) => {
    const partial = (dto ?? {}) as Partial<ProjectTask>
    const row: ProjectTask = {
      title:          partial.title ?? '',
      description:    partial.description ?? null,
      status:         partial.status ?? 'Todo',
      estimatedHours: partial.estimatedHours ?? 0,
      actualHours:    partial.actualHours ?? 0,
      assignedUserId: partial.assignedUserId ?? null,
      dueDate:        partial.dueDate ?? null,
      id:             uid(),
      projectId:      id,
    }
    mock.mockTasks.unshift(row); return row
  },
  updateTask: async (tid, dto) => {
    const i = mock.mockTasks.findIndex(t => t.id === tid)
    if (i < 0) throw new Error('Not found')
    mock.mockTasks[i] = { ...mock.mockTasks[i], ...(dto as ProjectTask) }
    return mock.mockTasks[i]
  },
  deleteTask: async (tid) => {
    const i = mock.mockTasks.findIndex(t => t.id === tid)
    if (i >= 0) mock.mockTasks.splice(i, 1)
  },
}

const ticketsRepo:  ITicketRepo  = makeCrud<Ticket>(mock.mockTickets)
const workLogsRepo: IWorkLogRepo = makeCrud<WorkLog>(mock.mockWorkLogs)

const expensesRepo: IExpenseRepo = {
  ...makeCrud<Expense>(mock.mockExpenses),
  approve: async (id) => {
    const e = mock.mockExpenses.find(x => x.id === id)!
    e.status = 'Approved'; return e
  },
  reject: async (id) => {
    const e = mock.mockExpenses.find(x => x.id === id)!
    e.status = 'Rejected'; return e
  },
}

const contractsRepo: IContractRepo = makeCrud<Contract>(mock.mockContracts)
const invoicesRepo:  IInvoiceRepo  = makeCrud<Invoice>(mock.mockInvoices)

const dashboardRepo: IDashboardRepo = {
  summary: async () => mock.mockDashboard,
}

// Mock 已讀狀態（包含動態產生的 auto-* id）持久化於 localStorage
const READ_KEY = 'sofycrm.mock.readNotifications'
function loadReadSet(): Set<string> {
  try { return new Set(JSON.parse(localStorage.getItem(READ_KEY) ?? '[]')) }
  catch { return new Set() }
}
function saveReadSet(s: Set<string>) {
  localStorage.setItem(READ_KEY, JSON.stringify([...s]))
}
const readSet = loadReadSet()

function getCurrentMockUserId(): string {
  try {
    const raw = localStorage.getItem('sofycrm.auth')
    if (raw) return JSON.parse(raw)?.user?.id ?? ''
  } catch { /* ignore */ }
  return ''
}

function allNotifications(): Notification[] {
  const uid = getCurrentMockUserId()
  if (!uid) return mock.mockNotifications.map(n => ({ ...n, isRead: readSet.has(n.id) || n.isRead }))

  // 靜態 + 依合約動態產生
  const dynamic = mock.buildContractNotificationsFor(uid)
  const merged: Notification[] = []
  const seen = new Set<string>()
  for (const n of [...dynamic, ...mock.mockNotifications.filter(n => n.userId === uid)]) {
    if (seen.has(n.id)) continue
    seen.add(n.id)
    merged.push({ ...n, isRead: readSet.has(n.id) || n.isRead })
  }
  return merged.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
}

const notificationsRepo: INotificationRepo = {
  list:        async (params) => page<Notification>(allNotifications(), params),
  markRead:    async (id) => {
    readSet.add(id); saveReadSet(readSet)
    const n = mock.mockNotifications.find(x => x.id === id); if (n) n.isRead = true
  },
  markAllRead: async () => {
    for (const n of allNotifications()) readSet.add(n.id)
    saveReadSet(readSet)
    mock.mockNotifications.forEach(n => n.isRead = true)
  },
}

const auditLogsRepo: IAuditLogRepo = {
  list: async (params) => {
    let items = mock.mockAuditLogs.slice()
    const mod = (params?.module as string | undefined)?.trim()
    const act = (params?.action as string | undefined)?.trim()
    const kw  = (params?.keyword as string | undefined)?.trim().toLowerCase()
    if (mod) items = items.filter(a => a.module === mod)
    if (act) items = items.filter(a => a.action === act)
    if (kw)  items = items.filter(a =>
      (a.module + ' ' + a.action + ' ' + (a.entityId ?? '') + ' ' + (a.user?.name ?? '')).toLowerCase().includes(kw))
    items = items.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
    return page<AuditLog>(items, params)
  },
}

export const mockRepos: IRepos = {
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
