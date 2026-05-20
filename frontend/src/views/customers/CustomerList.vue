<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useRepos } from '@/repositories'
import { useAuthStore } from '@/stores/auth'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import {
  Button, Input, Badge, Card, Dialog, Label, Textarea, Select, ConfirmDialog,
} from '@/components/ui'
import { Plus, Search } from 'lucide-vue-next'
import type { Customer, CustomerStatus } from '@/types/models'
import { formatDate, formatDateTime } from '@/lib/utils'
import { customerStatusLabel, customerStatusOptions } from '@/lib/labels'

const router  = useRouter()
const repos   = useRepos()
const auth    = useAuthStore()
const items   = ref<Customer[]>([])
const loading = ref(false)
const keyword = ref('')

const { userOptions, loadUsers } = useLookups()

const columns = [
  { key: 'companyName', label: '公司名稱' },
  { key: 'taxId',       label: '統編' },
  { key: 'industry',    label: '產業' },
  { key: 'status',      label: '狀態' },
  { key: 'tags',        label: '標籤' },
  { key: 'owner',       label: '負責人' },
  { key: 'createdAt',   label: '建立時間' },
  { key: '_actions',    label: '', width: '90px', align: 'right' as const },
]

const statusVariant: Record<CustomerStatus, string> = {
  Potential: 'info', Contacting: 'info', Quoting: 'warning',
  Won: 'success',     Lost: 'danger',     Maintenance: 'secondary',
}

const filtered = computed(() => {
  const k = keyword.value.trim().toLowerCase()
  if (!k) return items.value
  return items.value.filter(c =>
    c.companyName.toLowerCase().includes(k)
    || (c.taxId ?? '').toLowerCase().includes(k)
    || (c.industry ?? '').toLowerCase().includes(k))
})

// -------- CRUD Dialog 狀態 --------
const editorOpen = ref(false)
const editing    = ref<Partial<Customer> | null>(null)
const tagsInput  = ref('')
const errors     = ref<Record<string, string>>({})

function openCreate() {
  editing.value = {
    companyName: '', status: 'Potential', tags: [],
    ownerUserId: auth.user?.id ?? null,
    createdAt: new Date().toISOString(), // 自動帶入建立時間
  }
  tagsInput.value = ''
  errors.value = {}
  editorOpen.value = true
}

function openEdit(row: Customer) {
  editing.value = { ...row }
  tagsInput.value = (row.tags ?? []).join(', ')
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.companyName?.trim()) errors.value.companyName = '此欄位必填'
  if (!editing.value?.status)              errors.value.status      = '請選擇狀態'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const payload: Partial<Customer> = {
    ...editing.value,
    tags: tagsInput.value.split(',').map(t => t.trim()).filter(Boolean),
  }
  if (payload.id) {
    await repos.value.customers.update(payload.id, payload)
  } else {
    await repos.value.customers.create(payload)
  }
  editorOpen.value = false
  await load()
}

// -------- 刪除 --------
const confirmOpen = ref(false)
const pendingDelete = ref<Customer | null>(null)
function askRemove(row: Customer) {
  pendingDelete.value = row
  confirmOpen.value = true
}
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.customers.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try {
    const r = await repos.value.customers.list()
    items.value = r.items
  } finally { loading.value = false }
}

onMounted(async () => {
  await Promise.all([load(), loadUsers()])
})
</script>

<template>
  <PageHeader title="客戶管理" subtitle="搜尋、建立、編輯客戶資料">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增客戶</Button>
    </template>
  </PageHeader>

  <Card class="p-3 mb-3 flex items-center gap-2">
    <Search class="size-4 text-muted-foreground" />
    <Input v-model="keyword" placeholder="搜尋公司名稱 / 統編 / 產業"
           class="border-0 shadow-none focus-visible:ring-0" />
  </Card>

  <DataTable :columns="(columns as any)" :rows="filtered" :empty="loading ? '載入中…' : '尚無客戶資料'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'status'">
        <Badge :variant="(statusVariant[(row as Customer).status] ?? 'default') as any">{{ customerStatusLabel[(row as Customer).status] }}</Badge>
      </template>
      <template v-else-if="col.key === 'tags'">
        <div class="flex flex-wrap gap-1">
          <Badge v-for="t in (row as Customer).tags" :key="t" variant="outline">{{ t }}</Badge>
        </div>
      </template>
      <template v-else-if="col.key === 'owner'">
        {{ (row as Customer).ownerUser?.name ?? '-' }}
      </template>
      <template v-else-if="col.key === 'createdAt'">
        {{ formatDate((row as Customer).createdAt) }}
      </template>
      <template v-else-if="col.key === 'companyName'">
        <a class="text-primary hover:underline cursor-pointer"
           @click="router.push('/customers/' + (row as Customer).id)">{{ (row as Customer).companyName }}</a>
      </template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as Customer)" @remove="askRemove(row as Customer)" />
      </template>
      <template v-else>
        {{ (row as any)[col.key] ?? '-' }}
      </template>
    </template>
  </DataTable>

  <Dialog
    v-model="editorOpen"
    :title="editing?.id ? '編輯客戶' : '新增客戶'"
    max-width="36rem"
  >
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>公司名稱</Label>
        <Input v-model="editing.companyName" placeholder="例：Acme 股份有限公司" :error="errors.companyName" />
      </div>
      <div>
        <Label>統一編號</Label>
        <Input v-model="editing.taxId" />
      </div>
      <div>
        <Label>產業</Label>
        <Input v-model="editing.industry" />
      </div>
      <div>
        <Label required>狀態</Label>
        <Select v-model="editing.status as any" :options="customerStatusOptions" :error="errors.status" />
      </div>
      <div>
        <Label>負責人</Label>
        <Select v-model="editing.ownerUserId as any" :options="userOptions" placeholder="選擇負責人" />
      </div>
      <div>
        <Label>標籤（以逗號分隔）</Label>
        <Input v-model="tagsInput" placeholder="VIP, 長期" />
      </div>
      <div>
        <Label>建立時間</Label>
        <Input :model-value="editing.createdAt ? formatDateTime(editing.createdAt) : '—'" readonly />
      </div>
      <div class="sm:col-span-2">
        <Label>地址</Label>
        <Input v-model="editing.address" />
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

  <ConfirmDialog
    v-model="confirmOpen"
    title="刪除客戶"
    :message="`確定要刪除「${pendingDelete?.companyName}」？此動作不可復原。`"
    @confirm="doRemove"
  />
</template>
