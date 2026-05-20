<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRepos } from '@/repositories'
import { useAuthStore } from '@/stores/auth'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Badge, Button, Dialog, Input, Label, Select, Textarea, ConfirmDialog } from '@/components/ui'
import { Plus, Check, X, Send } from 'lucide-vue-next'
import type { Expense, ExpenseStatus } from '@/types/models'
import { formatDate, formatMoney } from '@/lib/utils'
import {
  expenseCategoryLabel, expenseCategoryOptions,
  expenseStatusLabel,   expenseStatusOptions,
} from '@/lib/labels'

const repos = useRepos()
const auth  = useAuthStore()
const items = ref<Expense[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers, users, loadUsers } = useLookups()

const variant: Record<ExpenseStatus, string> = {
  Draft: 'secondary', Submitted: 'info', Approved: 'success', Rejected: 'danger', Paid: 'success',
}

const columns = [
  { key: 'expenseDate', label: '日期' },
  { key: 'user',        label: '申請人' },
  { key: 'customer',    label: '客戶' },
  { key: 'category',    label: '類別' },
  { key: 'amount',      label: '金額', align: 'right' as const },
  { key: 'status',      label: '狀態' },
  { key: 'description', label: '說明' },
  { key: '_actions',    label: '', width: '160px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<Expense> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<Expense | null>(null)

const isCreating = computed(() => !editing.value?.id)
// 編輯模式可變更狀態；新增時強制 Draft，欄位顯示但 disabled
const statusOptionsAllowed = computed(() => {
  if (isCreating.value) return expenseStatusOptions
  if (auth.isAdmin) return expenseStatusOptions
  // Sales 只能 Draft / Submitted（送出申請）
  return expenseStatusOptions.filter(o => ['Draft', 'Submitted'].includes(o.value))
})

function openCreate() {
  editing.value = {
    category: 'Meal', amount: 0,
    expenseDate: new Date().toISOString().slice(0, 10),
    status: 'Draft',                              // 新增固定 Draft
    userId: auth.user?.id ?? '',                  // 自動帶入申請人
    customerId: undefined,
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: Expense) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.category)    errors.value.category    = '請選擇類別'
  if (!editing.value?.expenseDate) errors.value.expenseDate = '請選擇日期'
  if (editing.value?.amount == null || Number(editing.value.amount) <= 0) errors.value.amount = '請輸入金額（>0）'
  if (!editing.value?.userId)      errors.value.userId      = '請選擇申請人'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  // 新增 → 永遠為 Draft（不受表單值影響）
  if (isCreating.value) editing.value.status = 'Draft'

  const customer = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const user     = users.value.find(u => u.id === editing.value!.userId) ?? null
  const dto: Partial<Expense> = { ...editing.value, customer, user }

  if (dto.id) await repos.value.expenses.update(dto.id, dto)
  else        await repos.value.expenses.create(dto)
  editorOpen.value = false
  await load()
}
async function approve(row: Expense) { await repos.value.expenses.approve(row.id); await load() }
async function reject(row: Expense)  { await repos.value.expenses.reject(row.id);  await load() }

// 草稿 → 送出（任何申請人或 Admin 皆可送出自己的草稿）
async function submitDraft(row: Expense) {
  await repos.value.expenses.update(row.id, { ...row, status: 'Submitted' })
  await load()
}
function canSubmit(row: Expense): boolean {
  if (row.status !== 'Draft') return false
  return auth.isAdmin || row.userId === auth.user?.id
}
function askRemove(row: Expense) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.expenses.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.expenses.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers(), loadUsers()])
})
</script>

<template>
  <PageHeader title="報銷管理" subtitle="餐飲 / 交通 / 停車 / 禮品 / 住宿 / 其他 — 審核流程：草稿 → 已送出 → 已核准/已退回">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增報銷</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無報銷'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'expenseDate'">{{ formatDate((row as Expense).expenseDate) }}</template>
      <template v-else-if="col.key === 'user'">{{ (row as Expense).user?.name ?? '-' }}</template>
      <template v-else-if="col.key === 'customer'">{{ (row as Expense).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'amount'">{{ formatMoney((row as Expense).amount) }}</template>
      <template v-else-if="col.key === 'category'">{{ expenseCategoryLabel[(row as Expense).category] }}</template>
      <template v-else-if="col.key === 'status'">
        <Badge :variant="(variant[(row as Expense).status] as any)">{{ expenseStatusLabel[(row as Expense).status] }}</Badge>
      </template>
      <template v-else-if="col.key === '_actions'">
        <div class="flex items-center justify-end gap-1">
          <Button
            v-if="canSubmit(row as Expense)"
            variant="ghost" size="icon" title="送出申請"
            class="text-sky-600 hover:bg-sky-500/10"
            @click="submitDraft(row as Expense)"
          ><Send class="size-4" /></Button>
          <Button
            v-if="auth.isAdmin && (row as Expense).status === 'Submitted'"
            variant="ghost" size="icon" title="核准"
            class="text-emerald-600 hover:bg-emerald-500/10"
            @click="approve(row as Expense)"
          ><Check class="size-4" /></Button>
          <Button
            v-if="auth.isAdmin && (row as Expense).status === 'Submitted'"
            variant="ghost" size="icon" title="退回"
            class="text-amber-600 hover:bg-amber-500/10"
            @click="reject(row as Expense)"
          ><X class="size-4" /></Button>
          <RowActions @edit="openEdit(row as Expense)" @remove="askRemove(row as Expense)" />
        </div>
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯報銷' : '新增報銷'" max-width="34rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div>
        <Label required>申請人</Label>
        <Input :model-value="(users.find(u => u.id === (editing?.userId ?? ''))?.name) ?? auth.user?.name ?? '—'" readonly />
      </div>
      <div>
        <Label>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="（選填）" />
      </div>
      <div>
        <Label required>日期</Label>
        <Input v-model="editing.expenseDate" type="date" :error="errors.expenseDate" />
      </div>
      <div>
        <Label required>類別</Label>
        <Select v-model="editing.category as any" :options="expenseCategoryOptions" :error="errors.category" />
      </div>
      <div>
        <Label required>金額</Label>
        <Input v-model="editing.amount as any" type="number" :error="errors.amount" />
      </div>
      <div>
        <Label>狀態</Label>
        <!-- 新增固定 Draft 不可改；編輯時依角色可變更 -->
        <Select
          v-model="editing.status as any"
          :options="statusOptionsAllowed"
          :disabled="isCreating"
        />
        <p v-if="isCreating" class="text-xs text-muted-foreground mt-1">
          新增報銷一律為「草稿」，建立後可送出 / 核准 / 退回。
        </p>
      </div>
      <div class="sm:col-span-2">
        <Label>收據網址</Label>
        <Input v-model="editing.receiptUrl" placeholder="https://…" />
      </div>
      <div class="sm:col-span-2">
        <Label>說明</Label>
        <Textarea v-model="editing.description" :rows="3" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen" title="刪除報銷"
    :message="`確定要刪除這筆報銷？`" @confirm="doRemove" />
</template>
