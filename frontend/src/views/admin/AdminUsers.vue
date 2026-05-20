<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRepos } from '@/repositories'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import RowActions from '@/components/common/RowActions.vue'
import { Badge, Button, Dialog, Input, Label, Select, ConfirmDialog } from '@/components/ui'
import { Plus, KeyRound } from 'lucide-vue-next'
import type { User, UserRole } from '@/types/models'
import { formatDate } from '@/lib/utils'

const repos = useRepos()
const items = ref<User[]>([])
const loading = ref(false)

const roleVariant: Record<UserRole, string> = { Admin: 'danger', Sales: 'info', Service: 'success' }
const roleOptions = [
  { label: 'Admin',   value: 'Admin' },
  { label: 'Sales',   value: 'Sales' },
  { label: 'Service', value: 'Service' },
]
const statusOptions = [
  { label: 'Active',   value: 'Active' },
  { label: 'Disabled', value: 'Disabled' },
]

const columns = [
  { key: 'name',      label: '姓名' },
  { key: 'email',     label: 'Email' },
  { key: 'role',      label: '角色' },
  { key: 'phone',     label: '電話' },
  { key: 'status',    label: '狀態' },
  { key: 'createdAt', label: '建立' },
  { key: '_actions',  label: '', width: '120px', align: 'right' as const },
]

interface UserForm extends Partial<User> { password?: string }
const editorOpen = ref(false)
const editing = ref<UserForm | null>(null)

const pwdOpen = ref(false)
const pwdUser = ref<User | null>(null)
const newPassword = ref('')

const confirmOpen = ref(false)
const pendingDelete = ref<User | null>(null)

function openCreate() {
  editing.value = { name: '', email: '', role: 'Sales', password: '', status: 'Active' }
  editorOpen.value = true
}
function openEdit(row: User) {
  editing.value = { ...row, password: undefined }
  editorOpen.value = true
}
async function save() {
  if (!editing.value) return
  const dto = editing.value
  if (dto.id) {
    await repos.value.users.update(dto.id, dto)
  } else {
    await repos.value.users.create(dto)
  }
  editorOpen.value = false
  await load()
}

function openResetPassword(row: User) { pwdUser.value = row; newPassword.value = ''; pwdOpen.value = true }
async function doResetPassword() {
  if (pwdUser.value && newPassword.value.length >= 6) {
    await repos.value.users.resetPassword(pwdUser.value.id, newPassword.value)
    pwdOpen.value = false
  }
}

function askRemove(row: User) { pendingDelete.value = row; confirmOpen.value = true }
async function doRemove() {
  if (pendingDelete.value) {
    await repos.value.users.remove(pendingDelete.value.id)
    pendingDelete.value = null
    await load()
  }
}

async function load() {
  loading.value = true
  try { items.value = (await repos.value.users.list()).items }
  finally { loading.value = false }
}
onMounted(load)
</script>

<template>
  <PageHeader title="使用者管理" subtitle="Admin 專用 — 建立 / 停用 / 重設密碼 / 指派角色">
    <template #actions>
      <Button @click="openCreate"><Plus class="size-4" /> 新增使用者</Button>
    </template>
  </PageHeader>
  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無使用者'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'role'">
        <Badge :variant="(roleVariant[(row as User).role] as any)">{{ (row as User).role }}</Badge>
      </template>
      <template v-else-if="col.key === 'status'">
        <Badge :variant="((row as User).status === 'Active' ? 'success' : 'secondary') as any">{{ (row as User).status }}</Badge>
      </template>
      <template v-else-if="col.key === 'createdAt'">{{ formatDate((row as User).createdAt) }}</template>
      <template v-else-if="col.key === '_actions'">
        <div class="flex items-center justify-end gap-1">
          <Button variant="ghost" size="icon" title="重設密碼" @click="openResetPassword(row as User)">
            <KeyRound class="size-4" />
          </Button>
          <RowActions @edit="openEdit(row as User)" @remove="askRemove(row as User)" />
        </div>
      </template>
      <template v-else>{{ (row as any)[col.key] ?? '-' }}</template>
    </template>
  </DataTable>

  <Dialog v-model="editorOpen" :title="editing?.id ? '編輯使用者' : '新增使用者'" max-width="32rem">
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3" v-if="editing">
      <div class="sm:col-span-2">
        <Label required>姓名</Label>
        <Input v-model="editing.name" />
      </div>
      <div class="sm:col-span-2">
        <Label required>Email / 帳號</Label>
        <Input v-model="editing.email" type="text" />
      </div>
      <div v-if="!editing.id" class="sm:col-span-2">
        <Label required>密碼</Label>
        <Input v-model="editing.password" type="password" />
      </div>
      <div>
        <Label required>角色</Label>
        <Select v-model="editing.role as any" :options="roleOptions" />
      </div>
      <div>
        <Label>狀態</Label>
        <Select v-model="editing.status as any" :options="statusOptions" />
      </div>
      <div class="sm:col-span-2">
        <Label>電話</Label>
        <Input v-model="editing.phone" />
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="editorOpen = false">取消</Button>
      <Button @click="save">儲存</Button>
    </template>
  </Dialog>

  <Dialog v-model="pwdOpen" :title="`重設密碼 — ${pwdUser?.name ?? ''}`" max-width="24rem">
    <div>
      <Label required>新密碼（至少 6 碼）</Label>
      <Input v-model="newPassword" type="password" />
    </div>
    <template #footer>
      <Button variant="outline" @click="pwdOpen = false">取消</Button>
      <Button @click="doResetPassword" :disabled="newPassword.length < 6">送出</Button>
    </template>
  </Dialog>

  <ConfirmDialog v-model="confirmOpen" title="停用使用者"
    :message="`確定要停用「${pendingDelete?.name}」？`" @confirm="doRemove" />
</template>
