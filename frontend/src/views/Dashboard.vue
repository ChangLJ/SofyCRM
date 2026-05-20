<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { dashboardApi } from '@/api/endpoints'
import { Card, Badge } from '@/components/ui'
import PageHeader from '@/components/common/PageHeader.vue'
import { formatMoney } from '@/lib/utils'
import { TrendingUp, Users, Briefcase, Ticket as TicketIcon, Award } from 'lucide-vue-next'
import type { DashboardSummary, OpportunityStatus } from '@/types/models'
import { motion } from 'motion-v'
import { opportunityStatusLabel } from '@/lib/labels'

function statusLabel(s: string): string {
  return opportunityStatusLabel[s as OpportunityStatus] ?? s
}

const data    = ref<DashboardSummary | null>(null)
const loading = ref(true)

onMounted(async () => {
  try { data.value = await dashboardApi.summary() }
  finally { loading.value = false }
})

const cards = [
  { key: 'customerCount',     label: '客戶總數',   icon: Users,       color: 'text-sky-500' },
  { key: 'openOpportunities', label: '進行中商機', icon: Briefcase,   color: 'text-violet-500' },
  { key: 'monthlyRevenue',    label: '本月營收',   icon: TrendingUp,  color: 'text-emerald-500', money: true },
  { key: 'openTickets',       label: '待處理 Ticket', icon: TicketIcon, color: 'text-amber-500' },
  { key: 'winRate',           label: '成交率',     icon: Award,       color: 'text-rose-500', suffix: '%' },
] as const

function maxTrend(arr?: { amount: number }[]) {
  if (!arr || !arr.length) return 1
  return Math.max(...arr.map(x => x.amount)) || 1
}
</script>

<template>
  <PageHeader title="Dashboard" subtitle="總覽 — 客戶 / 商機 / 工單 / 營收" />

  <div v-if="loading" class="text-muted-foreground">載入中…</div>

  <template v-else-if="data">
    <div class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-5 gap-3">
      <motion.div
        v-for="(c, idx) in cards" :key="c.key"
        :initial="{ opacity: 0, y: 8 }" :animate="{ opacity: 1, y: 0 }" :transition="{ delay: idx * 0.05 }"
      >
        <Card class="p-4">
          <div class="flex items-center justify-between">
            <span class="text-xs text-muted-foreground">{{ c.label }}</span>
            <component :is="c.icon" :class="['size-5', c.color]" />
          </div>
          <div class="mt-2 text-2xl font-semibold tracking-tight">
            <template v-if="(c as any).money">{{ formatMoney((data as any)[c.key]) }}</template>
            <template v-else>{{ (data as any)[c.key] }}<span v-if="(c as any).suffix">{{ (c as any).suffix }}</span></template>
          </div>
        </Card>
      </motion.div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-3 mt-4">
      <Card class="p-4 lg:col-span-2">
        <div class="flex items-center justify-between mb-3">
          <h3 class="font-semibold">月度營收趨勢</h3>
          <Badge variant="info">{{ new Date().getFullYear() }}</Badge>
        </div>
        <div class="flex items-end gap-2 h-40">
          <motion.div
            v-for="(p, i) in data.revenueTrend" :key="p.month"
            class="flex-1 bg-gradient-to-t from-primary/70 to-primary rounded-t-md min-w-[14px]"
            :initial="{ height: 0 }"
            :animate="{ height: ((p.amount / maxTrend(data.revenueTrend)) * 90 + 8) + '%' }"
            :transition="{ delay: i * 0.05, duration: 0.4 }"
            :title="`${p.month} 月: ${formatMoney(p.amount)}`"
          />
        </div>
        <div class="flex justify-between text-xs text-muted-foreground mt-2">
          <span v-for="p in data.revenueTrend" :key="'lab-' + p.month">{{ p.month }}月</span>
        </div>
      </Card>

      <Card class="p-4">
        <h3 class="font-semibold mb-3">商機 Pipeline</h3>
        <ul class="space-y-2">
          <li v-for="row in data.pipeline" :key="row.status" class="flex items-center justify-between text-sm">
            <span class="text-muted-foreground">{{ statusLabel(row.status) }}</span>
            <span class="font-medium">{{ row.count }} 案 · {{ formatMoney(row.amount) }}</span>
          </li>
        </ul>
      </Card>
    </div>
  </template>
</template>
