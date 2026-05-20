<script setup lang="ts">
import { RouterLink, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  LayoutDashboard, Users, Briefcase, FileText, MessageCircle,
  FolderKanban, Ticket as TicketIcon, Clock, Receipt, FileSignature,
  CreditCard, ShieldCheck, Building2, ScrollText,
} from 'lucide-vue-next'
import { computed } from 'vue'
import { cn } from '@/lib/utils'

const route = useRoute()
const auth  = useAuthStore()

interface NavItem { to: string; label: string; icon: any; roles?: string[] }

const items: NavItem[] = [
  { to: '/dashboard',     label: 'Dashboard',  icon: LayoutDashboard },
  { to: '/customers',     label: '客戶管理',    icon: Users },
  { to: '/followups',     label: '聯絡紀錄',    icon: MessageCircle, roles: ['Admin', 'Sales'] },
  { to: '/opportunities', label: '商機管理',    icon: Briefcase,     roles: ['Admin', 'Sales'] },
  { to: '/quotations',    label: '報價管理',    icon: FileText,      roles: ['Admin', 'Sales'] },
  { to: '/projects',      label: '專案管理',    icon: FolderKanban },
  { to: '/tickets',       label: 'Ticket',     icon: TicketIcon },
  { to: '/worklogs',      label: '工時紀錄',    icon: Clock,         roles: ['Admin', 'Service'] },
  { to: '/expenses',      label: '報銷管理',    icon: Receipt },
  { to: '/contracts',     label: '合約管理',    icon: FileSignature, roles: ['Admin', 'Sales'] },
  { to: '/invoices',      label: '發票/收款',   icon: CreditCard,    roles: ['Admin', 'Sales'] },
  { to: '/admin/users',      label: '使用者管理', icon: ShieldCheck, roles: ['Admin'] },
  { to: '/admin/audit-logs', label: '稽核紀錄',   icon: ScrollText,  roles: ['Admin'] },
]

const visible = computed(() =>
  items.filter(i => !i.roles || i.roles.includes(auth.user?.role ?? '')))
</script>

<template>
  <aside class="hidden md:flex md:flex-col w-64 border-r bg-card/40 backdrop-blur">
    <div class="h-14 flex items-center gap-2 px-5 border-b">
      <Building2 class="size-5 text-primary" />
      <span class="font-semibold tracking-tight">SofyCRM</span>
    </div>
    <nav class="flex-1 overflow-y-auto p-3 space-y-1">
      <RouterLink
        v-for="item in visible" :key="item.to"
        :to="item.to"
        :class="cn(
          'group flex items-center gap-2.5 rounded-md px-3 py-2 text-sm font-medium transition-colors',
          route.path === item.to || route.path.startsWith(item.to + '/')
            ? 'bg-accent text-accent-foreground'
            : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
        )"
      >
        <component :is="item.icon" class="size-4" />
        {{ item.label }}
      </RouterLink>
    </nav>
    <div class="p-4 border-t text-xs text-muted-foreground">
      <div>角色：<span class="text-foreground font-medium">{{ auth.user?.role ?? '-' }}</span></div>
      <div class="truncate">{{ auth.user?.email }}</div>
    </div>
  </aside>
</template>
