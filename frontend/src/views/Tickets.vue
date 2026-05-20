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
import type { Ticket, TicketStatus, TicketPriority } from '@/types/models'
import { formatDateTime } from '@/lib/utils'
import { ticketStatusLabel, ticketStatusOptions, ticketPriorityLabel, ticketPriorityOptions } from '@/lib/labels'

const repos = useRepos()
const auth  = useAuthStore()
const items = ref<Ticket[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers, users, userOptions, loadUsers } = useLookups()

const stColor: Record<TicketStatus, string> = {
  Open: 'info', Processing: 'warning', WaitingCustomer: 'secondary', Closed: 'success',
}
const prColor: Record<TicketPriority, string> = {
  Low: 'secondary', Medium: 'info', High: 'warning', Critical: 'danger',
}

const columns = [
  { key: 'title',         label: '主旨' },
  { key: 'customer',      label: '客戶' },
  { key: 'priority',      label: '優先' },
  { key: 'status',        label: '狀態' },
  { key: 'assignedUser',  label: '負責人' },
  { key: 'slaDueAt',      label: 'SLA 到期' },
  { key: '_actions',      label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<Ticket> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Ticket | null>(null)

function openCreate() {
  editing.value = {
    title: '', priority: 'Medium', status: 'Open',
    customerId: '',
    assignedUserId: auth.user?.id ?? '',
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Ticket) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.title?.trim()) errors.value.title          = '此欄位必填'
  if (!editing.value?.customerId)    errors.value.customerId     = '請選擇客戶'
  if (!editing.value?.assignedUserId) errors.value.assignedUserId = '請選擇負責人'
  if (!editing.value?.priority)      errors.value.priority       = '請選擇優先度'
  if (!editing.value?.status)        errors.value.status         = '請選擇狀態'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const customer     = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const assignedUser = users.value.find(u => u.id === editing.value!.assignedUserId) ?? null
  const dto: Partial<Ticket> = { ...editing.value, customer, assignedUser }
  if (dto.id) await repos.value.tickets.update(dto.id, dto)
  else        await repos.value.tickets.create(dto)
  editorOpen.value = false
  await load()
}
function askRemove(row: Ticket) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.tickets.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.tickets.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers(), loadUsers()])
})
</script>

<template>
  <PageHeader title="Ticket / 客服" subtitle="待處理 · 處理中 · 等待客戶 · 已結案">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增 Ticket</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無 Ticket'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as Ticket).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'assignedUser'">{{ (row as Ticket).assignedUser?.name ?? '-' }}</template>
      <template v-else-if="col.key === 'priority'">
        <Badge :variant="(prColor[(row as Ticket).priority] as any)">{{ ticketPriorityLabel[(row as Ticket).priority] }}</Badge>
      </template>
      <template v-else-if="col.key === 'status'">
        <Badge :variant="(stColor[(row as Ticket).status] as any)">{{ ticketStatusLabel[(row as Ticket).status] }}</Badge>
      </template>
      <template v-else-if="col.key === 'slaDueAt'">{{ formatDateTime((row as Ticket).slaDueAt) }}</template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Ticket)" @remove="askRemove(row as Ticket)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯 Ticket' : '新增 Ticket'" max-width="36rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>主旨</Label>
        <Input v-model="editing.title" :error="errors.title" />
      </div>
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label required>負責人</Label>
        <Select v-model="editing.assignedUserId as any" :options="userOptions" placeholder="選擇負責人" :error="errors.assignedUserId" />
      </div>
      <div>
        <Label required>優先度</Label>
        <Select v-model="editing.priority as any" :options="ticketPriorityOptions" :error="errors.priority" />
      </div>
      <div>
        <Label required>狀態</Label>
        <Select v-model="editing.status as any" :options="ticketStatusOptions" :error="errors.status" />
      </div>
      <div>
        <Label>SLA 到期時間</Label>
        <Input v-model="editing.slaDueAt" type="datetime-local" />
      </div>
      <div class="sm:col-span-2">
        <Label>內容</Label>
        <Textarea v-model="editing.content" :rows="4" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen" title="刪除 Ticket"
    :message="`確定要刪除「${pendingDelete?.title}」？`" @confirm="doRemove" />
</template>
