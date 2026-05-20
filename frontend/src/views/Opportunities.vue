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
import type { Opportunity, OpportunityStatus } from '@/types/models'
import { formatDate, formatMoney } from '@/lib/utils'
import { opportunityStatusLabel, opportunityStatusOptions } from '@/lib/labels'

const repos = useRepos()
const auth  = useAuthStore()
const items = ref<Opportunity[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers, users, userOptions, loadUsers } = useLookups()

const variant: Record<OpportunityStatus, string> = {
  NewLead: 'info', Contacted: 'info', Proposal: 'warning',
  Negotiation: 'warning', Won: 'success', Lost: 'danger',
}

const columns = [
  { key: 'title',             label: '商機名稱' },
  { key: 'customer',          label: '客戶' },
  { key: 'amount',            label: '金額', align: 'right' as const },
  { key: 'status',            label: '狀態' },
  { key: 'expectedCloseDate', label: '預計成交' },
  { key: 'owner',             label: '負責人' },
  { key: '_actions',          label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<Opportunity> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Opportunity | null>(null)

function openCreate() {
  editing.value = {
    title: '', amount: 0, status: 'NewLead',
    customerId: '',
    ownerUserId: auth.user?.id ?? '',
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Opportunity) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.title?.trim()) errors.value.title       = '此欄位必填'
  if (!editing.value?.customerId)    errors.value.customerId  = '請選擇客戶'
  if (!editing.value?.ownerUserId)   errors.value.ownerUserId = '請選擇負責人'
  if (!editing.value?.status)        errors.value.status      = '請選擇狀態'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const customer  = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const ownerUser = users.value.find(u => u.id === editing.value!.ownerUserId) ?? null
  const dto: Partial<Opportunity> = { ...editing.value, customer, ownerUser }
  if (dto.id) await repos.value.opportunities.update(dto.id, dto)
  else        await repos.value.opportunities.create(dto)
  editorOpen.value = false
  await load()
}
function askRemove(row: Opportunity) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.opportunities.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.opportunities.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers(), loadUsers()])
})
</script>

<template>
  <PageHeader title="商機管理" subtitle="銷售 Pipeline：新名單 → 已接觸 → 提案中 → 議價中 → 成交/失單">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增商機</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無商機'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as Opportunity).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'amount'">{{ formatMoney((row as Opportunity).amount) }}</template>
      <template v-else-if="col.key === 'status'">
        <Badge :variant="(variant[(row as Opportunity).status] as any) ?? 'default'">{{ opportunityStatusLabel[(row as Opportunity).status] }}</Badge>
      </template>
      <template v-else-if="col.key === 'expectedCloseDate'">{{ formatDate((row as Opportunity).expectedCloseDate) }}</template>
      <template v-else-if="col.key === 'owner'">{{ (row as Opportunity).ownerUser?.name ?? '-' }}</template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Opportunity)" @remove="askRemove(row as Opportunity)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯商機' : '新增商機'" max-width="34rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>商機名稱</Label>
        <Input v-model="editing.title" :error="errors.title" />
      </div>
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label>金額</Label>
        <Input v-model="editing.amount as any" type="number" />
      </div>
      <div>
        <Label required>狀態</Label>
        <Select v-model="editing.status as any" :options="opportunityStatusOptions" :error="errors.status" />
      </div>
      <div>
        <Label required>負責人</Label>
        <Select v-model="editing.ownerUserId as any" :options="userOptions" placeholder="選擇負責人" :error="errors.ownerUserId" />
      </div>
      <div>
        <Label>預計成交日</Label>
        <Input v-model="editing.expectedCloseDate" type="date" />
      </div>
      <div class="sm:col-span-2">
        <Label>備註</Label>
        <Textarea v-model="editing.description" :rows="3" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen"
    title="刪除商機"
    :message="`確定要刪除「${pendingDelete?.title}」？`"
    @confirm="doRemove" />
</template>
