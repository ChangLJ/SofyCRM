<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRepos } from '@/repositories'
import PageHeader from '@/components/common/PageHeader.vue'
import DataTable  from '@/components/common/DataTable.vue'
import {
  Card, Input, Select, Badge, Button, Dialog,
} from '@/components/ui'
import { Search, RefreshCw, FileSearch } from 'lucide-vue-next'
import type { AuditLog } from '@/types/models'
import { formatDateTime } from '@/lib/utils'

const repos = useRepos()
const items = ref<AuditLog[]>([])
const total = ref(0)
const loading = ref(false)

const filterModule  = ref('')
const filterAction  = ref('')
const keyword       = ref('')
const page          = ref(1)
const pageSize      = ref(50)

// --- 中文化 對照 ---
const moduleLabel: Record<string, string> = {
  Auth:        '身份驗證',
  User:        '使用者',
  Customer:    '客戶',
  Followup:    '聯絡紀錄',
  Opportunity: '商機',
  Quotation:   '報價',
  Project:     '專案',
  Ticket:      'Ticket',
  WorkLog:     '工時',
  Expense:     '報銷',
  Contract:    '合約',
  Invoice:     '發票',
  Notification:'通知',
}
const actionLabel: Record<string, string> = {
  Create:        '建立',
  Update:        '更新',
  Delete:        '刪除',
  Login:         '登入',
  Logout:        '登出',
  Submit:        '送出',
  Approve:       '核准',
  Reject:        '退回',
  ResetPassword: '重設密碼',
  Read:          '讀取',
}
const actionVariant: Record<string, string> = {
  Create:        'success',
  Update:        'info',
  Delete:        'danger',
  Login:         'secondary',
  Logout:        'secondary',
  Submit:        'info',
  Approve:       'success',
  Reject:        'warning',
  ResetPassword: 'warning',
}

const moduleOptions = computed(() => [
  { label: '全部模組', value: '' },
  ...Object.entries(moduleLabel).map(([value, label]) => ({ value, label })),
])
const actionOptions = computed(() => [
  { label: '全部動作', value: '' },
  ...Object.entries(actionLabel).map(([value, label]) => ({ value, label })),
])

const columns = [
  { key: 'createdAt', label: '時間',    width: '170px' },
  { key: 'user',      label: '操作者',  width: '140px' },
  { key: 'module',    label: '模組',    width: '110px' },
  { key: 'action',    label: '動作',    width: '110px' },
  { key: 'entityId',  label: '對象 ID' },
  { key: 'ipAddress', label: 'IP',     width: '120px' },
  { key: '_diff',     label: '變更',    width: '110px', align: 'right' as const },
]

async function load() {
  loading.value = true
  try {
    const r = await repos.value.auditLogs.list({
      module:   filterModule.value || undefined,
      action:   filterAction.value || undefined,
      keyword:  keyword.value || undefined,
      page:     page.value,
      pageSize: pageSize.value,
    })
    items.value = r.items
    total.value = r.total
  } finally {
    loading.value = false
  }
}
onMounted(load)

// --- 變更內容檢視 ---
const diffOpen = ref(false)
const diffRow  = ref<AuditLog | null>(null)
function showDiff(row: AuditLog) {
  diffRow.value = row
  diffOpen.value = true
}
function pretty(s?: string | null): string {
  if (!s) return ''
  try { return JSON.stringify(JSON.parse(s), null, 2) }
  catch { return s }
}
</script>

<template>
  <PageHeader title="稽核紀錄 / Audit Log" subtitle="所有資料變更與重要動作皆會留下紀錄（僅 Admin 可檢視）" />

  <Card class="p-3 mb-3 grid grid-cols-1 md:grid-cols-12 gap-2">
    <div class="md:col-span-4 flex items-center gap-2 border rounded-md px-2">
      <Search class="size-4 text-muted-foreground" />
      <Input
        v-model="keyword"
        placeholder="搜尋 操作者 / 對象 ID / 模組 …"
        class="border-0 shadow-none focus-visible:ring-0"
        @keyup.enter="page = 1; load()"
      />
    </div>
    <Select v-model="filterModule" :options="moduleOptions" class="md:col-span-3" />
    <Select v-model="filterAction" :options="actionOptions" class="md:col-span-3" />
    <Button variant="outline" class="md:col-span-2" @click="page = 1; load()">
      <RefreshCw class="size-4" /> 套用篩選
    </Button>
  </Card>

  <DataTable :columns="columns as any" :rows="items" :empty="loading ? '載入中…' : '尚無稽核紀錄'">
    <template #cell="{ row, col }">
      <template v-if="col.key === 'createdAt'">
        <span class="font-mono text-xs">{{ formatDateTime((row as AuditLog).createdAt) }}</span>
      </template>
      <template v-else-if="col.key === 'user'">
        <span class="text-sm">{{ (row as AuditLog).user?.name ?? '系統' }}</span>
      </template>
      <template v-else-if="col.key === 'module'">
        <Badge variant="outline">{{ moduleLabel[(row as AuditLog).module] ?? (row as AuditLog).module }}</Badge>
      </template>
      <template v-else-if="col.key === 'action'">
        <Badge :variant="(actionVariant[(row as AuditLog).action] as any) ?? 'default'">
          {{ actionLabel[(row as AuditLog).action] ?? (row as AuditLog).action }}
        </Badge>
      </template>
      <template v-else-if="col.key === 'entityId'">
        <span class="font-mono text-xs text-muted-foreground">{{ (row as AuditLog).entityId ?? '-' }}</span>
      </template>
      <template v-else-if="col.key === 'ipAddress'">
        <span class="font-mono text-xs text-muted-foreground">{{ (row as AuditLog).ipAddress ?? '-' }}</span>
      </template>
      <template v-else-if="col.key === '_diff'">
        <Button
          v-if="(row as AuditLog).beforeData || (row as AuditLog).afterData"
          variant="ghost" size="sm" @click="showDiff(row as AuditLog)"
        >
          <FileSearch class="size-3.5" /> 檢視
        </Button>
      </template>
      <template v-else>{{ (row as any)[col.key] ?? '-' }}</template>
    </template>
  </DataTable>

  <div class="flex items-center justify-between mt-3 text-sm text-muted-foreground">
    <div>共 {{ total }} 筆，目前顯示第 {{ items.length === 0 ? 0 : (page - 1) * pageSize + 1 }} – {{ (page - 1) * pageSize + items.length }} 筆</div>
    <div class="flex items-center gap-2">
      <Button variant="outline" size="sm" :disabled="page <= 1" @click="page--; load()">上一頁</Button>
      <span class="font-mono">{{ page }}</span>
      <Button variant="outline" size="sm" :disabled="page * pageSize >= total" @click="page++; load()">下一頁</Button>
    </div>
  </div>

  <Dialog v-model="diffOpen" title="變更內容" :description="diffRow ? `${moduleLabel[diffRow.module] ?? diffRow.module} · ${actionLabel[diffRow.action] ?? diffRow.action} · ${formatDateTime(diffRow.createdAt)}` : ''" max-width="44rem">
    <div v-if="diffRow" class="grid grid-cols-1 md:grid-cols-2 gap-3">
      <div>
        <div class="text-xs font-medium mb-1 text-muted-foreground">變更前 (before)</div>
        <pre class="text-xs bg-muted/40 border rounded-md p-3 overflow-auto max-h-72 whitespace-pre-wrap break-all">{{ pretty(diffRow.beforeData) || '—' }}</pre>
      </div>
      <div>
        <div class="text-xs font-medium mb-1 text-muted-foreground">變更後 (after)</div>
        <pre class="text-xs bg-muted/40 border rounded-md p-3 overflow-auto max-h-72 whitespace-pre-wrap break-all">{{ pretty(diffRow.afterData) || '—' }}</pre>
      </div>
    </div>
    <template #footer>
      <Button variant="outline" @click="diffOpen = false">關閉</Button>
    </template>
  </Dialog>
</template>
