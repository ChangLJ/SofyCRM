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
import type { CustomerFollowup, FollowupType } from '@/types/models'
import { formatDateTime } from '@/lib/utils'
import { followupTypeLabel, followupTypeOptions } from '@/lib/labels'

const repos = useRepos()
const auth  = useAuthStore()
const items = ref<CustomerFollowup[]>([])
const loading = ref(false)

const { customers, customerOptions, loadCustomers, users, userOptions, loadUsers } = useLookups()

const typeColor: Record<string, string> = {
  Call: 'info', Email: 'secondary', Meeting: 'success', Visit: 'warning', Line: 'info',
}

const columns = [
  { key: 'createdAt',         label: '時間' },
  { key: 'customer',          label: '客戶' },
  { key: 'followupType',      label: '類型' },
  { key: 'content',           label: '內容' },
  { key: 'user',              label: '經辦' },
  { key: 'nextFollowupDate',  label: '下次跟進' },
  { key: '_actions',          label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<CustomerFollowup> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<CustomerFollowup | null>(null)

function openCreate() {
  editing.value = {
    followupType: 'Call' as FollowupType,
    content: '',
    userId: auth.user?.id ?? '',
    customerId: '',
    createdAt: new Date().toISOString(),
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: CustomerFollowup) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.customerId)   errors.value.customerId   = '請選擇客戶'
  if (!editing.value?.followupType) errors.value.followupType = '請選擇類型'
  if (!editing.value?.content?.trim()) errors.value.content   = '此欄位必填'
  if (!editing.value?.userId)       errors.value.userId       = '請選擇經辦'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  // 帶入 customer / user 物件以便 list 即時顯示
  const customer = customers.value.find(c => c.id === editing.value!.customerId) ?? null
  const user     = users.value.find(u => u.id === editing.value!.userId) ?? null
  const dto: Partial<CustomerFollowup> = { ...editing.value, customer, user }
  if (dto.id) await repos.value.followups.update(dto.id, dto)
  else        await repos.value.followups.create(dto)
  editorOpen.value = false
  await load()
}
function askRemove(row: CustomerFollowup) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.followups.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.followups.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadCustomers(), loadUsers()])
})
</script>

<template>
  <PageHeader title="聯絡紀錄" subtitle="Call · Email · Meeting · Visit · LINE">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增紀錄</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無聯絡紀錄'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'customer'">{{ (row as CustomerFollowup).customer?.companyName ?? '-' }}</template>
      <template v-else-if="col.key === 'user'">{{ (row as CustomerFollowup).user?.name ?? '-' }}</template>
      <template v-else-if="col.key === 'followupType'">
        <Badge :variant="(typeColor[(row as CustomerFollowup).followupType] as any) ?? 'default'">
          {{ followupTypeLabel[(row as CustomerFollowup).followupType] }}
        </Badge>
      </template>
      <template v-else-if="col.key === 'createdAt'">{{ formatDateTime((row as CustomerFollowup).createdAt) }}</template>
      <template v-else-if="col.key === 'nextFollowupDate'">{{ formatDateTime((row as CustomerFollowup).nextFollowupDate) }}</template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as CustomerFollowup)" @remove="askRemove(row as CustomerFollowup)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯聯絡紀錄' : '新增聯絡紀錄'" max-width="34rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>客戶</Label>
        <Select v-model="editing.customerId as any" :options="customerOptions" placeholder="選擇客戶" :error="errors.customerId" />
      </div>
      <div>
        <Label required>類型</Label>
        <Select v-model="editing.followupType as any" :options="followupTypeOptions" :error="errors.followupType" />
      </div>
      <div>
        <Label required>經辦</Label>
        <Select v-model="editing.userId as any" :options="userOptions" placeholder="選擇經辦" :error="errors.userId" />
      </div>
      <div>
        <Label>時間</Label>
        <Input :model-value="editing.createdAt ? formatDateTime(editing.createdAt) : '—'" readonly />
      </div>
      <div>
        <Label>下次跟進</Label>
        <Input v-model="editing.nextFollowupDate" type="datetime-local" />
      </div>
      <div class="sm:col-span-2">
        <Label required>內容</Label>
        <Textarea v-model="editing.content" :rows="4" :error="errors.content" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen" title="刪除聯絡紀錄"
    :message="`確定要刪除這筆聯絡紀錄？`" @confirm="doRemove" />
</template>
