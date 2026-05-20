<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRepos } from '@/repositories'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Badge, Button, Dialog, Input, Label, Select, Textarea, ConfirmDialog } from '@/components/ui'
import { Plus } from 'lucide-vue-next'
import type { Quotation, QuotationStatus } from '@/types/models'
import { formatDate, formatMoney } from '@/lib/utils'
import { quotationStatusLabel, quotationStatusOptions } from '@/lib/labels'

const repos = useRepos()
const items = ref<Quotation[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers } = useLookups()

const variant: Record<QuotationStatus, string> = {
  Draft: 'secondary', Sent: 'info', Accepted: 'success', Rejected: 'danger', Expired: 'warning',
}

const columns = [
  { key: 'quotationNo', label: '報價單號' },
  { key: 'customer',    label: '客戶' },
  { key: 'version',     label: '版本', align: 'center' as const },
  { key: 'totalAmount', label: '總金額', align: 'right' as const },
  { key: 'status',      label: '狀態' },
  { key: 'validUntil',  label: '有效期限' },
  { key: '_actions',    label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<Quotation> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Quotation | null>(null)

function openCreate() {
  editing.value = {
    quotationNo: '(系統自動產生)',
    version: 1, totalAmount: 0, status: 'Draft', items: [],
    customerId: '',
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Quotation) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.customerId) errors.value.customerId = '請選擇客戶'
  if (!editing.value?.status)     errors.value.status     = '請選擇狀態'
  if (editing.value?.totalAmount == null || isNaN(Number(editing.value.totalAmount))) errors.value.totalAmount = '請輸入金額'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const customer = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const dto: Partial<Quotation> = { ...editing.value, customer }
  if (dto.id) {
    // 編輯時保留原 quotationNo，避免被覆蓋
    await repos.value.quotations.update(dto.id, dto)
  } else {
    // 新增不傳 quotationNo，後端 / mock 會自動填入 PO-yyyyMMdd-XXX
    const { quotationNo: _ignore, ...rest } = dto
    await repos.value.quotations.create(rest)
  }
  editorOpen.value = false
  await load()
}
function askRemove(row: Quotation) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.quotations.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.quotations.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers()])
})
</script>

<template>
  <PageHeader title="報價管理" subtitle="多版本報價 / 系統自動產生單號 PO-yyyyMMdd-NNN">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增報價</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無報價'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as Quotation).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'totalAmount'">{{ formatMoney((row as Quotation).totalAmount) }}</template>
      <template v-else-if="col.key === 'status'">
        <Badge :variant="(variant[(row as Quotation).status] as any) ?? 'default'">{{ quotationStatusLabel[(row as Quotation).status] }}</Badge>
      </template>
      <template v-else-if="col.key === 'validUntil'">{{ formatDate((row as Quotation).validUntil) }}</template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Quotation)" @remove="askRemove(row as Quotation)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯報價' : '新增報價'" max-width="36rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div>
        <Label>報價單號（自動產生，不可編輯）</Label>
        <Input :model-value="editing.quotationNo as string" readonly />
      </div>
      <div>
        <Label>版本</Label>
        <Input v-model="editing.version as any" type="number" />
      </div>
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label required>總金額</Label>
        <Input v-model="editing.totalAmount as any" type="number" :error="errors.totalAmount" />
      </div>
      <div>
        <Label required>狀態</Label>
        <Select v-model="editing.status as any" :options="quotationStatusOptions" :error="errors.status" />
      </div>
      <div class="sm:col-span-2">
        <Label>有效期限</Label>
        <Input v-model="editing.validUntil" type="date" />
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

  <ConfirmDialog v-model="confirmOpen" title="刪除報價"
    :message="`確定要刪除「${pendingDelete?.quotationNo}」？`" @confirm="doRemove" />
</template>
