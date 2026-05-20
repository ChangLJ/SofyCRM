/**
 * 系統內所有 Enum 的中文顯示對照表 + 對應的 Select options。
 *
 * 後端依然使用英文 enum 值（如 'Potential'），這裡只負責「呈現」與「選單顯示文字」。
 */
import type {
  CustomerStatus, FollowupType, OpportunityStatus, QuotationStatus,
  ProjectStatus, TaskStatus, TicketStatus, TicketPriority,
  ExpenseCategory, ExpenseStatus, PaymentStatus, UserRole,
} from '@/types/models'

// ---------- Customer ----------
export const customerStatusLabel: Record<CustomerStatus, string> = {
  Potential:   '潛在客戶',
  Contacting:  '接洽中',
  Quoting:     '報價中',
  Won:         '已成交',
  Lost:        '已失單',
  Maintenance: '維護中',
}

// ---------- Followup ----------
export const followupTypeLabel: Record<FollowupType, string> = {
  Call:    '電話',
  Email:   'Email',
  Meeting: '會議',
  Visit:   '拜訪',
  Line:    'LINE',
}

// ---------- Opportunity ----------
export const opportunityStatusLabel: Record<OpportunityStatus, string> = {
  NewLead:     '新名單',
  Contacted:   '已接觸',
  Proposal:    '提案中',
  Negotiation: '議價中',
  Won:         '已成交',
  Lost:        '已失單',
}

// ---------- Quotation ----------
export const quotationStatusLabel: Record<QuotationStatus, string> = {
  Draft:    '草稿',
  Sent:     '已寄出',
  Accepted: '已接受',
  Rejected: '已拒絕',
  Expired:  '已過期',
}

// ---------- Project ----------
export const projectStatusLabel: Record<ProjectStatus, string> = {
  Planning:    '規劃中',
  Development: '開發中',
  Testing:     '測試中',
  UAT:         '驗收中',
  Completed:   '已完成',
  Maintenance: '維護中',
}

// ---------- Task ----------
export const taskStatusLabel: Record<TaskStatus, string> = {
  Todo:       '待辦',
  InProgress: '進行中',
  Done:       '已完成',
  Blocked:    '受阻',
}

// ---------- Ticket ----------
export const ticketStatusLabel: Record<TicketStatus, string> = {
  Open:            '待處理',
  Processing:      '處理中',
  WaitingCustomer: '等待客戶',
  Closed:          '已結案',
}

export const ticketPriorityLabel: Record<TicketPriority, string> = {
  Low:      '低',
  Medium:   '中',
  High:     '高',
  Critical: '緊急',
}

// ---------- Expense ----------
export const expenseCategoryLabel: Record<ExpenseCategory, string> = {
  Meal:           '餐飲',
  Transportation: '交通',
  Parking:        '停車',
  Gift:           '禮品',
  Hotel:          '住宿',
  Other:          '其他',
}

export const expenseStatusLabel: Record<ExpenseStatus, string> = {
  Draft:     '草稿',
  Submitted: '已送出',
  Approved:  '已核准',
  Rejected:  '已退回',
  Paid:      '已付款',
}

// ---------- Payment ----------
export const paymentStatusLabel: Record<PaymentStatus, string> = {
  Pending:     '待收款',
  PartialPaid: '部分付款',
  Paid:        '已付清',
  Overdue:     '已逾期',
}

// ---------- User ----------
export const userRoleLabel: Record<UserRole, string> = {
  Admin:   '管理員',
  Sales:   '業務',
  Service: '客服',
}

// ---------------------------------------------------------------
// Helper: 把 Record<Enum, string> 轉成 SelectOption[]
// ---------------------------------------------------------------
export function toOptions<E extends string>(map: Record<E, string>): { label: string; value: E }[] {
  return (Object.keys(map) as E[]).map(k => ({ value: k, label: map[k] }))
}

export const customerStatusOptions    = toOptions(customerStatusLabel)
export const followupTypeOptions      = toOptions(followupTypeLabel)
export const opportunityStatusOptions = toOptions(opportunityStatusLabel)
export const quotationStatusOptions   = toOptions(quotationStatusLabel)
export const projectStatusOptions     = toOptions(projectStatusLabel)
export const taskStatusOptions        = toOptions(taskStatusLabel)
export const ticketStatusOptions      = toOptions(ticketStatusLabel)
export const ticketPriorityOptions    = toOptions(ticketPriorityLabel)
export const expenseCategoryOptions   = toOptions(expenseCategoryLabel)
export const expenseStatusOptions     = toOptions(expenseStatusLabel)
export const paymentStatusOptions     = toOptions(paymentStatusLabel)
export const userRoleOptions          = toOptions(userRoleLabel)
