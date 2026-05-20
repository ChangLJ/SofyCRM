<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRepos } from '@/repositories'
import { useAuthStore } from '@/stores/auth'
import { useNotificationsStore } from '@/stores/notifications'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Badge, Button, Dialog, Input, Label, Select, Textarea, ConfirmDialog } from '@/components/ui'
import { Plus } from 'lucide-vue-next'
import type { Contract } from '@/types/models'
import { formatDate } from '@/lib/utils'

const repos = useRepos()
const auth  = useAuthStore()
const notes = useNotificationsStore()
const items = ref<Contract[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers, users, userOptions, loadUsers } = useLookups()

const columns = [
  { key: 'contractName', label: '合約名稱' },
  { key: 'customer',     label: '客戶' },
  { key: 'owner',        label: '負責人' },
  { key: 'startDate',    label: '起始日' },
  { key: 'endDate',      label: '到期日' },
  { key: 'renewal',      label: '續約提醒' },
  { key: '_actions',     label: '', width: '90px', align: 'right' as const },
]

function daysToEnd(c: Contract): number | null {
  if (!c.endDate) return null
  return Math.round((new Date(c.endDate).getTime() - Date.now()) / 86400_000)
}
function isExpiringSoon(c: Contract) {
  const d = daysToEnd(c); return d != null && d >= 0 && d <= (c.renewalNoticeDays ?? 30)
}
function isOverdue(c: Contract) {
  const d = daysToEnd(c); return d != null && d < 0
}

const editorOpen = ref(false)
const editing = ref<Partial<Contract> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Contract | null>(null)

function openCreate() {
  editing.value = {
    contractName: '', renewalNoticeDays: 30,
    customerId: '',
    ownerUserId: auth.user?.id ?? null,
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Contract) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.contractName?.trim()) errors.value.contractName = '此欄位必填'
  if (!editing.value?.customerId)           errors.value.customerId   = '請選擇客戶'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const customer  = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const ownerUser = users.value.find(u => u.id === editing.value!.ownerUserId) ?? null
  const dto: Partial<Contract> = { ...editing.value, customer, ownerUser }
  if (dto.id) await repos.value.contracts.update(dto.id, dto)
  else        await repos.value.contracts.create(dto)
  editorOpen.value = false
  await load()
  await notes.refresh() // 新合約 / 修改 endDate 可能影響通知
}
function askRemove(row: Contract) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.contracts.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
    await notes.refresh()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.contracts.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers(), loadUsers()])
})
</script>

<template>
  <PageHeader title="合約管理" subtitle="合約上傳 / NDA / 維護合約 / 自動到期提醒（一個月內到期會在右上鈴鐺通知）">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增合約</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無合約'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as Contract).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'owner'">{{ (row as Contract).ownerUser?.name ?? '-' }}</template>
      <template v-else-if="col.key === 'startDate'">{{ formatDate((row as Contract).startDate) }}</template>
      <template v-else-if="col.key === 'endDate'">{{ formatDate((row as Contract).endDate) }}</template>
      <template v-else-if="col.key === 'renewal'">
        <Badge v-if="isOverdue(row as Contract)" variant="danger">已過期未續</Badge>
        <Badge v-else-if="isExpiringSoon(row as Contract)" variant="warning">即將到期</Badge>
        <span v-else class="text-muted-foreground text-xs">{{ (row as Contract).renewalNoticeDays }} 天前提醒</span>
      </template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Contract)" @remove="askRemove(row as Contract)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯合約' : '新增合約'" max-width="34rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>合約名稱</Label>
        <Input v-model="editing.contractName" :error="errors.contractName" />
      </div>
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label>負責人</Label>
        <Select v-model="editing.ownerUserId as any" :options="userOptions" placeholder="選擇負責人" />
      </div>
      <div>
        <Label>續約提前提醒（天）</Label>
        <Input v-model="editing.renewalNoticeDays as any" type="number" />
      </div>
      <div>
        <Label>起始日</Label>
        <Input v-model="editing.startDate" type="date" />
      </div>
      <div>
        <Label>到期日</Label>
        <Input v-model="editing.endDate" type="date" />
      </div>
      <div class="sm:col-span-2">
        <Label>檔案網址</Label>
        <Input v-model="editing.fileUrl" placeholder="https://…" />
      </div>
      <div class="sm:col-span-2">
        <Label>備註</Label>
        <Textarea v-model="editing.notes" :rows="3" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen" title="刪除合約"
    :message="`確定要刪除「${pendingDelete?.contractName}」？`" @confirm="doRemove" />
</template>
