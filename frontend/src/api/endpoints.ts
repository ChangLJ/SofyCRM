/**
 * View 端的 API 入口 — 全部透過 Repository 介面層派發。
 *
 * 切換 Mock / Real 由 `useDataSourceStore` 控制，
 * 這裡的物件會在「每次方法呼叫時」動態解析當前要用哪個實作，
 * 因此 view 不需感知資料來源。
 *
 * 新程式碼推薦使用：
 *   import { useRepos } from '@/repositories'
 *   const repos = useRepos()
 *   const list  = await repos.value.customers.list()
 */
import { getRepos } from '@/repositories'
import type {
  IAuthRepo, IUserRepo, ICustomerRepo, IFollowupRepo, IOpportunityRepo,
  IQuotationRepo, IProjectRepo, ITicketRepo, IWorkLogRepo, IExpenseRepo,
  IContractRepo, IInvoiceRepo, IDashboardRepo, INotificationRepo,
} from '@/repositories/types'

function proxy<T extends object>(getter: () => T): T {
  return new Proxy({} as T, {
    get(_, p) {
      const target = getter() as any
      const v = target[p]
      return typeof v === 'function' ? v.bind(target) : v
    },
  })
}

export const authApi:          IAuthRepo         = proxy(() => getRepos().auth)
export const usersApi:         IUserRepo         = proxy(() => getRepos().users)
export const customersApi:     ICustomerRepo     = proxy(() => getRepos().customers)
export const followupsApi:     IFollowupRepo     = proxy(() => getRepos().followups)
export const opportunitiesApi: IOpportunityRepo  = proxy(() => getRepos().opportunities)
export const quotationsApi:    IQuotationRepo    = proxy(() => getRepos().quotations)
export const projectsApi:      IProjectRepo      = proxy(() => getRepos().projects)
export const ticketsApi:       ITicketRepo       = proxy(() => getRepos().tickets)
export const workLogsApi:      IWorkLogRepo      = proxy(() => getRepos().worklogs)
export const expensesApi:      IExpenseRepo      = proxy(() => getRepos().expenses)
export const contractsApi:     IContractRepo     = proxy(() => getRepos().contracts)
export const invoicesApi:      IInvoiceRepo      = proxy(() => getRepos().invoices)
export const dashboardApi:     IDashboardRepo    = proxy(() => getRepos().dashboard)
export const notificationsApi: INotificationRepo = proxy(() => getRepos().notifications)
