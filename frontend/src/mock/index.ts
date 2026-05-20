import type {
  User, Customer, CustomerContact, CustomerFollowup, Opportunity, Quotation,
  Project, ProjectTask, Ticket, WorkLog, Expense, Contract, Invoice,
  Notification, DashboardSummary, AuditLog,
} from '@/types/models'

const adminId   = '11111111-1111-1111-1111-111111111111'
const salesId   = '22222222-2222-2222-2222-222222222222'
const serviceId = '33333333-3333-3333-3333-333333333333'

const cust1 = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1'
const cust2 = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2'
const cust3 = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3'

const today = new Date()
const daysFromNow = (n: number) => new Date(today.getTime() + n * 86400_000).toISOString()
const dateFromNow = (n: number) => new Date(today.getTime() + n * 86400_000).toISOString().slice(0, 10)

export const mockUsers: User[] = [
  { id: adminId,   name: '系統管理員', email: 'admin@sofycrm.local',   role: 'Admin',   status: 'Active', phone: '0900000001', createdAt: daysFromNow(-30) },
  { id: salesId,   name: '王業務',     email: 'sales@sofycrm.local',   role: 'Sales',   status: 'Active', phone: '0900000002', createdAt: daysFromNow(-25) },
  { id: serviceId, name: '陳客服',     email: 'service@sofycrm.local', role: 'Service', status: 'Active', phone: '0900000003', createdAt: daysFromNow(-20) },
]

export const mockCustomers: Customer[] = [
  {
    id: cust1, companyName: 'Acme 股份有限公司', taxId: '12345678',
    address: '台北市信義區信義路五段7號', industry: '製造業',
    ownerUserId: salesId, ownerUser: mockUsers[1],
    status: 'Contacting', tags: ['VIP', '長期'],
    notes: '由王業務跟進', createdAt: daysFromNow(-15), updatedAt: daysFromNow(-1),
  },
  {
    id: cust2, companyName: 'Beta 科技有限公司', taxId: '23456789',
    address: '新北市板橋區文化路一段100號', industry: '軟體業',
    ownerUserId: salesId, ownerUser: mockUsers[1],
    status: 'Quoting', tags: ['新客戶'],
    notes: '正在報價中', createdAt: daysFromNow(-10), updatedAt: daysFromNow(-2),
  },
  {
    id: cust3, companyName: 'Gamma 工業', taxId: '34567890',
    address: '台中市西屯區市政北二路1號', industry: '貿易業',
    ownerUserId: adminId, ownerUser: mockUsers[0],
    status: 'Maintenance', tags: ['維護中'],
    notes: '舊客戶轉維護', createdAt: daysFromNow(-200), updatedAt: daysFromNow(-30),
  },
]

export const mockContacts: CustomerContact[] = [
  { id: 'c1', customerId: cust1, name: '張總經理', title: 'CEO',        phone: '02-12345678', email: 'ceo@acme.example',   isPrimary: true  },
  { id: 'c2', customerId: cust1, name: '李採購',   title: 'Purchasing', phone: '02-12345679', email: 'buy@acme.example',   isPrimary: false },
  { id: 'c3', customerId: cust2, name: '林經理',   title: 'PM',         phone: '02-22223333', email: 'pm@beta.example',    isPrimary: true  },
  { id: 'c4', customerId: cust3, name: '黃廠長',   title: '廠長',       phone: '04-33334444', email: 'plant@gamma.example',isPrimary: true  },
]

export const mockFollowups: CustomerFollowup[] = [
  { id: 'f1', customerId: cust1, userId: salesId, user: mockUsers[1], customer: mockCustomers[0], followupType: 'Call',    content: '電話確認需求，客戶希望下週報價', nextFollowupDate: daysFromNow(3),  createdAt: daysFromNow(-2) },
  { id: 'f2', customerId: cust1, userId: salesId, user: mockUsers[1], customer: mockCustomers[0], followupType: 'Meeting', content: '到府拜訪，已展示產品 Demo',       nextFollowupDate: daysFromNow(7),  createdAt: daysFromNow(-1) },
  { id: 'f3', customerId: cust2, userId: salesId, user: mockUsers[1], customer: mockCustomers[1], followupType: 'Email',   content: '寄送初步估價單',                  nextFollowupDate: daysFromNow(2),  createdAt: daysFromNow(-3) },
]

export const mockOpportunities: Opportunity[] = [
  { id: 'o1', customerId: cust1, customer: mockCustomers[0], ownerUserId: salesId, ownerUser: mockUsers[1], title: 'Acme ERP 導入專案', amount: 1200000, status: 'Proposal',    expectedCloseDate: dateFromNow(30), description: '客戶有意導入 ERP 完整方案', createdAt: daysFromNow(-10), updatedAt: daysFromNow(-1) },
  { id: 'o2', customerId: cust2, customer: mockCustomers[1], ownerUserId: salesId, ownerUser: mockUsers[1], title: 'Beta 官網改版',      amount:  350000, status: 'Negotiation', expectedCloseDate: dateFromNow(14), description: '報價中等待客戶決議',         createdAt: daysFromNow(-7),  updatedAt: daysFromNow(-1) },
  { id: 'o3', customerId: cust3, customer: mockCustomers[2], ownerUserId: adminId, ownerUser: mockUsers[0], title: 'Gamma 維護續約',     amount:  120000, status: 'Won',         expectedCloseDate: dateFromNow(-5), description: '已續約',                     createdAt: daysFromNow(-40), updatedAt: daysFromNow(-5) },
]

export const mockQuotations: Quotation[] = [
  {
    id: 'q1', customerId: cust1, customer: mockCustomers[0], opportunityId: 'o1',
    quotationNo: 'Q-20260518-001', version: 1, totalAmount: 1200000, status: 'Sent',
    validUntil: dateFromNow(30), notes: '含一年保固',
    createdAt: daysFromNow(-5), updatedAt: daysFromNow(-1),
    items: [
      { itemName: 'ERP 軟體授權', description: '永久授權 50 人',         qty: 1, unitPrice: 800000, estimatedHours: 0,   sortOrder: 1 },
      { itemName: '導入服務',     description: '系統建置 & 教育訓練',     qty: 1, unitPrice: 350000, estimatedHours: 240, sortOrder: 2 },
      { itemName: '一年原廠保固', description: '系統維護',                qty: 1, unitPrice:  50000, estimatedHours: 0,   sortOrder: 3 },
    ],
  },
]

export const mockProjects: Project[] = [
  {
    id: 'p1', customerId: cust1, customer: mockCustomers[0],
    projectName: 'Acme ERP 導入', pmUserId: serviceId, pmUser: mockUsers[2],
    startDate: dateFromNow(-10), endDate: dateFromNow(80),
    status: 'Development', description: '依合約已開案',
  },
]

export const mockTasks: ProjectTask[] = [
  { id: 't1', projectId: 'p1', assignedUserId: serviceId, title: '需求訪談', description: '完成現場訪談',           status: 'Done',       estimatedHours: 16, actualHours: 18, dueDate: dateFromNow(-5) },
  { id: 't2', projectId: 'p1', assignedUserId: serviceId, title: '系統建置', description: '主機環境準備',           status: 'InProgress', estimatedHours: 40, actualHours: 20, dueDate: dateFromNow(10) },
  { id: 't3', projectId: 'p1', assignedUserId: serviceId, title: '教育訓練', description: '使用者訓練',             status: 'Todo',       estimatedHours: 16, actualHours:  0, dueDate: dateFromNow(60) },
]

export const mockTickets: Ticket[] = [
  {
    id: 'tk1', customerId: cust1, customer: mockCustomers[0], projectId: 'p1',
    assignedUserId: serviceId, assignedUser: mockUsers[2],
    priority: 'High', title: '報表匯出失敗', content: '使用者按下匯出後系統報錯',
    status: 'Processing', slaDueAt: daysFromNow(1),
    createdAt: daysFromNow(-1), updatedAt: daysFromNow(0),
  },
  {
    id: 'tk2', customerId: cust3, customer: mockCustomers[2],
    assignedUserId: serviceId, assignedUser: mockUsers[2],
    priority: 'Medium', title: '客戶帳密重設', content: '管理員幫忙重設密碼',
    status: 'Open', slaDueAt: daysFromNow(3),
    createdAt: daysFromNow(-0.2), updatedAt: daysFromNow(-0.2),
  },
]

export const mockWorkLogs: WorkLog[] = [
  { id: 'w1', projectId: 'p1', project: mockProjects[0], userId: serviceId, user: mockUsers[2], workDate: dateFromNow(-3), hours: 4, description: '需求訪談會議' },
  { id: 'w2', projectId: 'p1', project: mockProjects[0], userId: serviceId, user: mockUsers[2], workDate: dateFromNow(-2), hours: 6, description: '撰寫訪談紀錄' },
  { id: 'w3', projectId: 'p1', project: mockProjects[0], userId: serviceId, user: mockUsers[2], workDate: dateFromNow(-1), hours: 8, description: '系統環境建置' },
]

export const mockExpenses: Expense[] = [
  { id: 'e1', userId: salesId,   user: mockUsers[1], customerId: cust1, customer: mockCustomers[0], category: 'Meal',           amount: 1200, expenseDate: dateFromNow(-5), description: '客戶午餐會議', status: 'Submitted' },
  { id: 'e2', userId: salesId,   user: mockUsers[1], customerId: cust2, customer: mockCustomers[1], category: 'Transportation', amount:  800, expenseDate: dateFromNow(-3), description: '高鐵往返',     status: 'Approved'  },
  { id: 'e3', userId: serviceId, user: mockUsers[2], customerId: cust1, customer: mockCustomers[0], category: 'Parking',        amount:  200, expenseDate: dateFromNow(-2), description: '停車費',       status: 'Draft'     },
]

export const mockContracts: (Contract & { ownerUserId?: string | null; ownerUser?: User | null })[] = [
  { id: 'ct1', customerId: cust1, customer: mockCustomers[0], contractName: 'Acme ERP 主合約',     startDate: dateFromNow(-10),  endDate: dateFromNow(355),  renewalNoticeDays: 30, notes: '一年期',  ownerUserId: salesId,   ownerUser: mockUsers[1] },
  { id: 'ct2', customerId: cust3, customer: mockCustomers[2], contractName: 'Gamma 維護合約',       startDate: dateFromNow(-200), endDate: dateFromNow(165),  renewalNoticeDays: 30, notes: '維護',    ownerUserId: adminId,   ownerUser: mockUsers[0] },
  // 一個月內到期 → 會觸發即將到期通知
  { id: 'ct3', customerId: cust2, customer: mockCustomers[1], contractName: 'Beta NDA 合約',        startDate: dateFromNow(-330), endDate: dateFromNow(15),   renewalNoticeDays: 30, notes: '即將到期',ownerUserId: salesId,   ownerUser: mockUsers[1] },
  // 已過期未續約
  { id: 'ct4', customerId: cust3, customer: mockCustomers[2], contractName: 'Gamma 顧問合約',       startDate: dateFromNow(-400), endDate: dateFromNow(-5),   renewalNoticeDays: 30, notes: '已過期未續',ownerUserId: adminId, ownerUser: mockUsers[0] },
]

export const mockInvoices: Invoice[] = [
  { id: 'i1', customerId: cust1, customer: mockCustomers[0], invoiceNo: 'INV-202605-0001', amount: 600000, issuedDate: dateFromNow(-7),  dueDate: dateFromNow(23),  paymentStatus: 'Pending',     paidAmount:     0 },
  { id: 'i2', customerId: cust3, customer: mockCustomers[2], invoiceNo: 'INV-202604-0098', amount: 120000, issuedDate: dateFromNow(-40), dueDate: dateFromNow(-10), paymentStatus: 'Overdue',     paidAmount:     0 },
  { id: 'i3', customerId: cust2, customer: mockCustomers[1], invoiceNo: 'INV-202605-0002', amount:  88000, issuedDate: dateFromNow(-5),  dueDate: dateFromNow(25),  paymentStatus: 'PartialPaid', paidAmount: 40000 },
]

export const mockNotifications: Notification[] = [
  { id: 'n1', userId: adminId,   title: '系統初始化完成', message: 'Mock data 已載入',       type: 'System',           isRead: false, link: '/dashboard',  createdAt: daysFromNow(-0.05) },
  { id: 'n2', userId: salesId,   title: '今日跟進',       message: '您有 2 筆客戶今日需追蹤', type: 'FollowupReminder', isRead: false, link: '/followups',  createdAt: daysFromNow(-0.1)  },
  { id: 'n3', userId: serviceId, title: 'SLA 即將逾時',   message: 'Ticket TK-0001 即將到期', type: 'SlaWarning',       isRead: false, link: '/tickets',    createdAt: daysFromNow(-0.2)  },
  // 合約即將到期 / 已過期未續約（依 mockContracts 自動同步）
  { id: 'n4', userId: adminId,   title: '合約即將到期',    message: 'Beta NDA 合約 將於 15 天內到期，請聯絡客戶續約', type: 'ContractExpiring', isRead: false, link: '/contracts', createdAt: daysFromNow(-0.3) },
  { id: 'n5', userId: salesId,   title: '合約即將到期',    message: 'Beta NDA 合約 將於 15 天內到期，請聯絡客戶續約', type: 'ContractExpiring', isRead: false, link: '/contracts', createdAt: daysFromNow(-0.3) },
  { id: 'n6', userId: adminId,   title: '合約已過期',      message: 'Gamma 顧問合約 已過期 5 天，尚未續約',           type: 'ContractOverdue',  isRead: false, link: '/contracts', createdAt: daysFromNow(-0.4) },
]

/**
 * 依目前 mockContracts 計算「即將到期 / 已過期未續」的通知，回傳對應 user 的清單。
 * 與 `mockNotifications` 合併使用（同 id 的不重複）。
 */
export function buildContractNotificationsFor(userId: string): Notification[] {
  const out: Notification[] = []
  const now = Date.now()
  for (const c of mockContracts) {
    if (!c.endDate) continue
    const days = Math.round((new Date(c.endDate).getTime() - now) / 86400_000)
    const owner = c.ownerUserId
    // 只通知合約負責人 + Admin
    const targets = new Set<string>()
    if (owner) targets.add(owner)
    targets.add(adminId)
    if (!targets.has(userId)) continue

    if (days >= 0 && days <= (c.renewalNoticeDays ?? 30)) {
      out.push({
        id: `auto-expire-${c.id}-${userId}`,
        userId,
        title: '合約即將到期',
        message: `${c.contractName} 將於 ${days} 天後到期，請聯絡客戶續約`,
        type: 'ContractExpiring',
        isRead: false,
        link: '/contracts',
        createdAt: new Date(now - 3600_000).toISOString(),
      })
    } else if (days < 0) {
      out.push({
        id: `auto-overdue-${c.id}-${userId}`,
        userId,
        title: '合約已過期',
        message: `${c.contractName} 已過期 ${-days} 天，尚未續約`,
        type: 'ContractOverdue',
        isRead: false,
        link: '/contracts',
        createdAt: new Date(now - 7200_000).toISOString(),
      })
    }
  }
  return out
}

// ---------- Audit Logs ----------
export const mockAuditLogs: AuditLog[] = [
  { id: 'al01', userId: adminId,   user: mockUsers[0], module: 'Auth',         action: 'Login',         entityId: adminId,                                  ipAddress: '127.0.0.1',   userAgent: 'Mozilla/5.0 (Windows)',          createdAt: daysFromNow(-0.01) },
  { id: 'al02', userId: salesId,   user: mockUsers[1], module: 'Customer',     action: 'Create',        entityId: cust1,                                    beforeData: null, afterData: '{"companyName":"Acme 股份有限公司"}',  ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.05) },
  { id: 'al03', userId: salesId,   user: mockUsers[1], module: 'Customer',     action: 'Update',        entityId: cust1,                                    beforeData: '{"status":"Potential"}', afterData: '{"status":"Contacting"}', ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.1)  },
  { id: 'al04', userId: salesId,   user: mockUsers[1], module: 'Opportunity',  action: 'Create',        entityId: 'op1',                                    afterData: '{"title":"Acme ERP 升級案","amount":1200000}',                       ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.2)  },
  { id: 'al05', userId: salesId,   user: mockUsers[1], module: 'Quotation',    action: 'Create',        entityId: 'q1',                                     afterData: '{"quotationNo":"PO-20260516-001","totalAmount":1500000}',            ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.3)  },
  { id: 'al06', userId: serviceId, user: mockUsers[2], module: 'Ticket',       action: 'Update',        entityId: 'tk1',                                    beforeData: '{"status":"Open"}', afterData: '{"status":"Processing"}', ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Mac)',     createdAt: daysFromNow(-0.4)  },
  { id: 'al07', userId: adminId,   user: mockUsers[0], module: 'User',         action: 'ResetPassword', entityId: salesId,                                                                                                                  ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.5)  },
  { id: 'al08', userId: salesId,   user: mockUsers[1], module: 'Expense',      action: 'Submit',        entityId: 'ex1',                                    beforeData: '{"status":"Draft"}', afterData: '{"status":"Submitted"}', ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.6)  },
  { id: 'al09', userId: adminId,   user: mockUsers[0], module: 'Expense',      action: 'Approve',       entityId: 'ex1',                                                                                                                    ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-0.7)  },
  { id: 'al10', userId: adminId,   user: mockUsers[0], module: 'Contract',     action: 'Create',        entityId: 'ct1',                                    afterData: '{"contractName":"Acme ERP 主合約"}',                                                                                  ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-1.5)  },
  { id: 'al11', userId: salesId,   user: mockUsers[1], module: 'Invoice',      action: 'Create',        entityId: 'inv1',                                   afterData: '{"invoiceNo":"INV-202605-0001","amount":808000}',                    ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-2.0)  },
  { id: 'al12', userId: serviceId, user: mockUsers[2], module: 'WorkLog',      action: 'Create',        entityId: 'wl1',                                    afterData: '{"hours":8,"projectName":"Acme ERP 升級"}',                          ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Mac)',     createdAt: daysFromNow(-2.5)  },
  { id: 'al13', userId: adminId,   user: mockUsers[0], module: 'User',         action: 'Create',        entityId: 'new-user',                               afterData: '{"name":"新使用者","role":"Sales"}',                                  ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-3.0)  },
  { id: 'al14', userId: salesId,   user: mockUsers[1], module: 'Customer',     action: 'Delete',        entityId: 'cust-old',                               beforeData: '{"companyName":"舊客戶"}',                                            ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-3.5)  },
  { id: 'al15', userId: adminId,   user: mockUsers[0], module: 'Auth',         action: 'Logout',        entityId: adminId,                                                                                                                  ipAddress: '127.0.0.1',  userAgent: 'Mozilla/5.0 (Windows)', createdAt: daysFromNow(-4.0)  },
]

export const mockDashboard: DashboardSummary = {
  customerCount: 3,
  openOpportunities: 2,
  monthlyRevenue: 808000,
  openTickets: 2,
  winRate: 100,
  pipeline: [
    { status: 'NewLead',     count: 0, amount: 0 },
    { status: 'Contacted',   count: 0, amount: 0 },
    { status: 'Proposal',    count: 1, amount: 1200000 },
    { status: 'Negotiation', count: 1, amount: 350000  },
    { status: 'Won',         count: 1, amount: 120000  },
    { status: 'Lost',        count: 0, amount: 0 },
  ],
  revenueTrend: [
    { month: 1, amount: 250000 }, { month: 2, amount: 320000 }, { month: 3, amount: 480000 },
    { month: 4, amount: 410000 }, { month: 5, amount: 808000 }, { month: 6, amount:       0 },
  ],
}
