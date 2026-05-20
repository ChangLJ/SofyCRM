export type UserRole   = 'Admin' | 'Sales' | 'Service'
export type UserStatus = 'Active' | 'Disabled'

export interface User {
  id: string
  name: string
  email: string
  role: UserRole
  phone?: string | null
  status?: UserStatus
  createdAt?: string
}

export type CustomerStatus = 'Potential' | 'Contacting' | 'Quoting' | 'Won' | 'Lost' | 'Maintenance'

export interface Customer {
  id: string
  companyName: string
  taxId?: string | null
  address?: string | null
  industry?: string | null
  ownerUserId?: string | null
  ownerUser?: User | null
  status: CustomerStatus
  tags: string[]
  notes?: string | null
  createdAt: string
  updatedAt: string
  contacts?: CustomerContact[]
  followups?: CustomerFollowup[]
  opportunities?: Opportunity[]
}

export interface CustomerContact {
  id: string
  customerId: string
  name: string
  title?: string | null
  phone?: string | null
  email?: string | null
  isPrimary: boolean
}

export type FollowupType = 'Call' | 'Email' | 'Meeting' | 'Visit' | 'Line'

export interface CustomerFollowup {
  id: string
  customerId: string
  customer?: Customer | null
  userId: string
  user?: User | null
  followupType: FollowupType
  content: string
  nextFollowupDate?: string | null
  createdAt: string
}

export type OpportunityStatus = 'NewLead' | 'Contacted' | 'Proposal' | 'Negotiation' | 'Won' | 'Lost'

export interface Opportunity {
  id: string
  customerId: string
  customer?: Customer | null
  ownerUserId: string
  ownerUser?: User | null
  title: string
  amount: number
  status: OpportunityStatus
  expectedCloseDate?: string | null
  description?: string | null
  createdAt: string
  updatedAt: string
}

export type QuotationStatus = 'Draft' | 'Sent' | 'Accepted' | 'Rejected' | 'Expired'

export interface QuotationItem {
  id?: string
  quotationId?: string
  itemName: string
  description?: string | null
  qty: number
  unitPrice: number
  estimatedHours: number
  sortOrder: number
}

export interface Quotation {
  id: string
  customerId: string
  customer?: Customer | null
  opportunityId?: string | null
  quotationNo: string
  version: number
  totalAmount: number
  status: QuotationStatus
  validUntil?: string | null
  notes?: string | null
  createdAt: string
  updatedAt: string
  items: QuotationItem[]
}

export type ProjectStatus = 'Planning' | 'Development' | 'Testing' | 'UAT' | 'Completed' | 'Maintenance'

export interface Project {
  id: string
  customerId: string
  customer?: Customer | null
  projectName: string
  pmUserId?: string | null
  pmUser?: User | null
  startDate?: string | null
  endDate?: string | null
  status: ProjectStatus
  description?: string | null
  tasks?: ProjectTask[]
}

export type TaskStatus = 'Todo' | 'InProgress' | 'Done' | 'Blocked'

export interface ProjectTask {
  id: string
  projectId: string
  assignedUserId?: string | null
  title: string
  description?: string | null
  status: TaskStatus
  estimatedHours: number
  actualHours: number
  dueDate?: string | null
}

export type TicketStatus   = 'Open' | 'Processing' | 'WaitingCustomer' | 'Closed'
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface Ticket {
  id: string
  customerId: string
  customer?: Customer | null
  projectId?: string | null
  assignedUserId?: string | null
  assignedUser?: User | null
  priority: TicketPriority
  title: string
  content?: string | null
  status: TicketStatus
  slaDueAt?: string | null
  closedAt?: string | null
  createdAt: string
  updatedAt: string
}

export interface WorkLog {
  id: string
  projectId: string
  project?: Project | null
  userId: string
  user?: User | null
  workDate: string
  hours: number
  description?: string | null
}

export type ExpenseCategory = 'Meal' | 'Transportation' | 'Parking' | 'Gift' | 'Hotel' | 'Other'
export type ExpenseStatus   = 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'Paid'

export interface Expense {
  id: string
  userId: string
  user?: User | null
  customerId?: string | null
  customer?: Customer | null
  category: ExpenseCategory
  amount: number
  expenseDate: string
  receiptUrl?: string | null
  description?: string | null
  status: ExpenseStatus
}

export interface Contract {
  id: string
  customerId: string
  customer?: Customer | null
  contractName: string
  startDate?: string | null
  endDate?: string | null
  renewalNoticeDays: number
  fileUrl?: string | null
  notes?: string | null
  ownerUserId?: string | null
  ownerUser?: User | null
}

export type PaymentStatus = 'Pending' | 'PartialPaid' | 'Paid' | 'Overdue'

export interface Invoice {
  id: string
  customerId: string
  customer?: Customer | null
  invoiceNo: string
  amount: number
  issuedDate?: string | null
  dueDate?: string | null
  paymentStatus: PaymentStatus
  paidAmount: number
  notes?: string | null
}

export interface Notification {
  id: string
  userId: string
  title: string
  message?: string | null
  type: string
  isRead: boolean
  link?: string | null
  createdAt: string
}

export interface AuditLog {
  id: string
  userId?: string | null
  user?: User | null
  module: string
  action: string
  entityId?: string | null
  beforeData?: string | null
  afterData?: string | null
  ipAddress?: string | null
  userAgent?: string | null
  createdAt: string
}

export interface DashboardSummary {
  customerCount: number
  openOpportunities: number
  monthlyRevenue: number
  openTickets: number
  winRate: number
  pipeline: { status: string; count: number; amount: number }[]
  revenueTrend: { month: number; amount: number }[]
}
