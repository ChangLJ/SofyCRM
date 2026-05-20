<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRepos } from '@/repositories'
import { useAuthStore } from '@/stores/auth'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Badge, Button, Dialog, Input, Label, Select, Textarea, ConfirmDialog } from '@/components/ui'
import { Plus } from 'lucide-vue-next'
import type { Project, ProjectStatus } from '@/types/models'
import { formatDate } from '@/lib/utils'
import { projectStatusLabel, projectStatusOptions } from '@/lib/labels'

const repos = useRepos()
const auth  = useAuthStore()
const items = ref<Project[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers, users, userOptions, loadUsers } = useLookups()

const variant: Record<ProjectStatus, string> = {
  Planning: 'info', Development: 'warning', Testing: 'warning',
  UAT: 'warning', Completed: 'success', Maintenance: 'secondary',
}

const columns = [
  { key: 'projectName', label: '專案名稱' },
  { key: 'customer',    label: '客戶' },
  { key: 'pm',          label: 'PM' },
  { key: 'startDate',   label: '起始日' },
  { key: 'endDate',     label: '結束日' },
  { key: 'status',      label: '狀態' },
  { key: '_actions',    label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<Project> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Project | null>(null)

function openCreate() {
  editing.value = {
    projectName: '', status: 'Planning',
    customerId: '',
    pmUserId: auth.user?.id ?? '',
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Project) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.projectName?.trim()) errors.value.projectName = '此欄位必填'
  if (!editing.value?.customerId)          errors.value.customerId  = '請選擇客戶'
  if (!editing.value?.pmUserId)            errors.value.pmUserId    = '請選擇 PM'
  if (!editing.value?.status)              errors.value.status      = '請選擇狀態'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const customer = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const pmUser   = users.value.find(u => u.id === editing.value!.pmUserId) ?? null
  const dto: Partial<Project> = { ...editing.value, customer, pmUser }
  if (dto.id) await repos.value.projects.update(dto.id, dto)
  else        await repos.value.projects.create(dto)
  editorOpen.value = false
  await load()
}
function askRemove(row: Project) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.projects.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.projects.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers(), loadUsers()])
})
</script>

<template>
  <PageHeader title="專案管理" subtitle="規劃中 → 開發中 → 測試中 → 驗收中 → 已完成 → 維護中">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增專案</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無專案'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as Project).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'pm'">{{ (row as Project).pmUser?.name ?? '-' }}</template>
      <template v-else-if="col.key === 'startDate'">{{ formatDate((row as Project).startDate) }}</template>
      <template v-else-if="col.key === 'endDate'">{{ formatDate((row as Project).endDate) }}</template>
      <template v-else-if="col.key === 'status'">
        <Badge :variant="(variant[(row as Project).status] as any) ?? 'default'">{{ projectStatusLabel[(row as Project).status] }}</Badge>
      </template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Project)" @remove="askRemove(row as Project)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯專案' : '新增專案'" max-width="34rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>專案名稱</Label>
        <Input v-model="editing.projectName" :error="errors.projectName" />
      </div>
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label required>PM</Label>
        <Select v-model="editing.pmUserId as any" :options="userOptions" placeholder="選擇 PM" :error="errors.pmUserId" />
      </div>
      <div>
        <Label required>狀態</Label>
        <Select v-model="editing.status as any" :options="projectStatusOptions" :error="errors.status" />
      </div>
      <div>
        <Label>開始日期</Label>
        <Input v-model="editing.startDate" type="date" />
      </div>
      <div>
        <Label>結束日期</Label>
        <Input v-model="editing.endDate" type="date" />
      </div>
      <div class="sm:col-span-2">
        <Label>描述</Label>
        <Textarea v-model="editing.description" :rows="3" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen" title="刪除專案"
    :message="`確定要刪除「${pendingDelete?.projectName}」？`" @confirm="doRemove" />
</template>
