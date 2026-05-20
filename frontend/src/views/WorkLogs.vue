<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRepos } from '@/repositories'
import { useAuthStore } from '@/stores/auth'
import { useLookups } from '@/composables/useLookups'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Button, Dialog, Input, Label, Select, Textarea, ConfirmDialog } from '@/components/ui'
import { Plus } from 'lucide-vue-next'
import type { WorkLog } from '@/types/models'
import { formatDate } from '@/lib/utils'

const repos = useRepos()
const auth  = useAuthStore()
const items = ref<WorkLog[]>([])
const loading = ref(false)

const { projects, projectOptions, loadProjects, users, userOptions, loadUsers } = useLookups()

const columns = [
  { key: 'workDate',    label: '日期' },
  { key: 'project',     label: '專案' },
  { key: 'user',        label: '人員' },
  { key: 'hours',       label: '時數', align: 'right' as const },
  { key: 'description', label: '說明' },
  { key: '_actions',    label: '', width: '90px', align: 'right' as const },
]

const editorOpen = ref(false)
const editing = ref<Partial<WorkLog> | null>(null)
const errors  = ref<Record<string, string>>({})
const confirmOpen = ref(false)
const pendingDelete = ref<WorkLog | null>(null)

function openCreate() {
  editing.value = {
    workDate: new Date().toISOString().slice(0, 10),
    hours: 8,
    projectId: '',
    userId: auth.user?.id ?? '',
  }
  errors.value = {}
  editorOpen.value = true
}
function openEdit(row: WorkLog) {
  editing.value = { ...row }
  errors.value = {}
  editorOpen.value = true
}

function validate(): boolean {
  errors.value = {}
  if (!editing.value?.projectId) errors.value.projectId = '請選擇專案'
  if (!editing.value?.userId)    errors.value.userId    = '請選擇人員'
  if (!editing.value?.workDate)  errors.value.workDate  = '請選擇日期'
  if (editing.value?.hours == null || Number(editing.value.hours) <= 0) errors.value.hours = '請輸入時數'
  return Object.keys(errors.value).length === 0
}

async function save() {
  if (!editing.value) return
  if (!validate()) return
  const project = projects.value.find(p => p.id === editing.value!.projectId) ?? null
  const user    = users.value.find(u => u.id === editing.value!.userId) ?? null
  const dto: Partial<WorkLog> = { ...editing.value, project, user }
  if (dto.id) await repos.value.worklogs.update(dto.id, dto)
  else        await repos.value.worklogs.create(dto)
  editorOpen.value = false
  await load()
}
function askRemove(row: WorkLog) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.worklogs.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.worklogs.list()).items }
  finally { loading.value = false }
}
onMounted(async () => {
  await Promise.all([load(), loadProjects(), loadUsers()])
})
</script>

<template>
  <PageHeader title="工時紀錄" subtitle="專案工時 / 個人工時統計">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增工時</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無工時'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'workDate'">{{ formatDate((row as WorkLog).workDate) }}</template>
      <template v-else-if="col.key === 'project'">{{ (row as WorkLog).project?.projectName ?? '-' }}</template>
      <template v-else-if="col.key === 'user'">{{ (row as WorkLog).user?.name ?? '-' }}</template>
      <template v-else-if="col.key === '_actions'">
        <RowActions @edit="openEdit(row as WorkLog)" @remove="askRemove(row as WorkLog)" />
      </template>
      <template v-else>{{ (row as any)[col.key] }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯工時' : '新增工時'" max-width="32rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>專案</Label>
        <Select v-model="editing.projectId as any" :options="projectOptions" placeholder="選擇專案" :error="errors.projectId" />
      </div>
      <div class="sm:col-span-2">
        <Label required>人員</Label>
        <Select v-model="editing.userId as any" :options="userOptions" placeholder="選擇人員" :error="errors.userId" />
      </div>
      <div>
        <Label required>日期</Label>
        <Input v-model="editing.workDate" type="date" :error="errors.workDate" />
      </div>
      <div>
        <Label required>時數</Label>
        <Input v-model="editing.hours as any" type="number" step="0.5" :error="errors.hours" />
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

  <ConfirmDialog v-model="confirmOpen" title="刪除工時"
    :message="`確定要刪除這筆工時？`" @confirm="doRemove" />
</template>
