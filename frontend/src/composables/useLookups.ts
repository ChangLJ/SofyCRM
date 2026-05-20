import { ref, computed } from 'vue'
import { useRepos } from '@/repositories'
import { userRoleLabel } from '@/lib/labels'
import type { Customer, User, Project } from '@/types/models'

/**
 * 共用查表 composable — 用於各表單需要客戶/負責人/專案下拉選單。
 * 用法：
 *   const { customers, customerOptions, loadCustomers } = useLookups()
 *   onMounted(loadCustomers)
 */
export function useLookups() {
  const repos = useRepos()

  const customers = ref<Customer[]>([])
  const users     = ref<User[]>([])
  const projects  = ref<Project[]>([])

  async function loadCustomers() {
    try {
      const r = await repos.value.customers.list({ pageSize: 500 })
      customers.value = r.items
    } catch { /* silent */ }
  }
  async function loadUsers() {
    try {
      const r = await repos.value.users.list({ pageSize: 500 })
      users.value = r.items
    } catch { /* silent */ }
  }
  async function loadProjects() {
    try {
      const r = await repos.value.projects.list({ pageSize: 500 })
      projects.value = r.items
    } catch { /* silent */ }
  }

  const customerOptions = computed(() =>
    customers.value.map(c => ({ value: c.id, label: c.companyName })),
  )
  const userOptions = computed(() =>
    users.value.map(u => ({ value: u.id, label: `${u.name}（${userRoleLabel[u.role]}）` })),
  )
  const projectOptions = computed(() =>
    projects.value.map(p => ({ value: p.id, label: p.projectName })),
  )

  return {
    customers, users, projects,
    customerOptions, userOptions, projectOptions,
    loadCustomers, loadUsers, loadProjects,
  }
}
