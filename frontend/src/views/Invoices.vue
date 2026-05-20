<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRepos } from '@/repositories'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Badge, Button, Dialog, Input, Label, Select, Textarea, ConfirmDialog } from '@/components/ui'
import { Plus } from 'lucide-vue-next'
import type { Invoice, PaymentStatus } from '@/types/models'
import { formatDate, formatMoney } from '@/lib/utils'
import { paymentStatusLabel, paymentStatusOptions } from '@/lib/labels'

const repos = useRepos()
const items = ref<Invoice[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers } = useLookups()

const variant: Record<PaymentStatus, string> = {
  Pending: 'info', PartialPaid: 'warning', Paid: 'success', Overdue: 'danger',
}

const columns = [
  { key: 'invoiceNo',    label: '發票編號' },
  { key: 'customer',     label: '客戶' },
  { key: 'amount',       label: '金額', align: 'right' as const },
  { key: 'paidAmount',   label: '已收', align: 'right' as const },
  { key: 'issuedDate',   label: '開立日' },
  { key: 'dueDate',      label: '到期日' },
  { key: 'paymentStatus',label: '收款狀態' },
  { key: '_actions',     label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<Invoice> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Invoice | null>(null)

function openCreate() {
  editing.value = {
    invoiceNo: '', amount: 0, paidAmount: 0, paymentStatus: 'Pending',
    issuedDate: new Date().toISOString().slice(0, 10),
    customerId: '',
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Invoice) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.invoiceNo?.trim()) errors.value.invoiceNo  = '此欄位必填'
  if (!editing.value?.customerId)        errors.value.customerId = '請選擇客戶'
  if (editing.value?.amount == null || Number(editing.value.amount) <= 0) errors.value.amount = '請輸入金額'
  if (!editing.value?.paymentStatus)     errors.value.paymentStatus = '請選擇收款狀態'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const customer = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const dto: Partial<Invoice> = { ...editing.value, customer }
  if (dto.id) await repos.value.invoices.update(dto.id, dto)
  else        await repos.value.invoices.create(dto)
  editorOpen.value = false
  await load()
}
function askRemove(row: Invoice) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.invoices.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.invoices.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers()])
})
</script>

<template>
  <PageHeader title="發票 / 收款管理" subtitle="收款狀態：待收款 · 部分付款 · 已付清 · 已逾期">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增發票</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無發票'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as Invoice).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'amount'">{{ formatMoney((row as Invoice).amount) }}</template>
      <template v-else-if="col.key === 'paidAmount'">{{ formatMoney((row as Invoice).paidAmount) }}</template>
      <template v-else-if="col.key === 'issuedDate'">{{ formatDate((row as Invoice).issuedDate) }}</template>
      <template v-else-if="col.key === 'dueDate'">{{ formatDate((row as Invoice).dueDate) }}</template>
      <template v-else-if="col.key === 'paymentStatus'">
        <Badge :variant="(variant[(row as Invoice).paymentStatus] as any)">{{ paymentStatusLabel[(row as Invoice).paymentStatus] }}</Badge>
      </template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Invoice)" @remove="askRemove(row as Invoice)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯發票' : '新增發票'" max-width="34rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>發票編號</Label>
        <Input v-model="editing.invoiceNo" placeholder="INV-YYYYMM-0001" :error="errors.invoiceNo" />
      </div>
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label required>金額</Label>
        <Input v-model="editing.amount as any" type="number" :error="errors.amount" />
      </div>
      <div>
        <Label>已收金額</Label>
        <Input v-model="editing.paidAmount as any" type="number" />
      </div>
      <div>
        <Label>開立日</Label>
        <Input v-model="editing.issuedDate" type="date" />
      </div>
      <div>
        <Label>到期日</Label>
        <Input v-model="editing.dueDate" type="date" />
      </div>
      <div class="sm:col-span-2">
        <Label required>收款狀態</Label>
        <Select v-model="editing.paymentStatus as any" :options="paymentStatusOptions" :error="errors.paymentStatus" />
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

  <ConfirmDialog v-model="confirmOpen" title="刪除發票"
    :message="`確定要刪除「${pendingDelete?.invoiceNo}」？`" @confirm="doRemove" />
</template>
