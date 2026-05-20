import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const routes: RouteRecordRaw[] = [
  { path: '/login', name: 'login', component: () => import('@/views/Login.vue'), meta: { layout: 'blank' } },
  {
    path: '/',
    component: () => import('@/components/layout/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '',                 redirect: '/dashboard' },
      { path: 'dashboard',        name: 'dashboard',     component: () => import('@/views/Dashboard.vue') },
      { path: 'customers',        name: 'customers',     component: () => import('@/views/customers/CustomerList.vue') },
      { path: 'customers/:id',    name: 'customer-detail', component: () => import('@/views/customers/CustomerDetail.vue') },
      { path: 'opportunities',    name: 'opportunities', component: () => import('@/views/Opportunities.vue'), meta: { roles: ['Admin', 'Sales'] } },
      { path: 'quotations',       name: 'quotations',    component: () => import('@/views/Quotations.vue'),    meta: { roles: ['Admin', 'Sales'] } },
      { path: 'followups',        name: 'followups',     component: () => import('@/views/Followups.vue'),     meta: { roles: ['Admin', 'Sales'] } },
      { path: 'projects',         name: 'projects',      component: () => import('@/views/Projects.vue') },
      { path: 'tickets',          name: 'tickets',       component: () => import('@/views/Tickets.vue') },
      { path: 'worklogs',         name: 'worklogs',      component: () => import('@/views/WorkLogs.vue'),      meta: { roles: ['Admin', 'Service'] } },
      { path: 'expenses',         name: 'expenses',      component: () => import('@/views/Expenses.vue') },
      { path: 'contracts',        name: 'contracts',     component: () => import('@/views/Contracts.vue'),     meta: { roles: ['Admin', 'Sales'] } },
      { path: 'invoices',         name: 'invoices',      component: () => import('@/views/Invoices.vue'),      meta: { roles: ['Admin', 'Sales'] } },
      { path: 'admin/users',      name: 'admin-users',   component: () => import('@/views/admin/AdminUsers.vue'),     meta: { roles: ['Admin'] } },
      { path: 'admin/audit-logs', name: 'admin-audit',   component: () => import('@/views/admin/AuditLogs.vue'),      meta: { roles: ['Admin'] } },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  // 未登入 → 一律導向登入頁
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { path: '/login', query: { redirect: to.fullPath } }
  }

  // RBAC：角色不符 → 回 dashboard
  const roles = to.meta.roles as string[] | undefined
  if (roles && auth.user && !roles.includes(auth.user.role)) {
    return { path: '/dashboard' }
  }

  // 已登入仍進到 /login → 改回 dashboard
  if (to.name === 'login' && auth.isAuthenticated) {
    return { path: '/dashboard' }
  }
})

export default router
