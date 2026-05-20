<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import { Button, Badge } from '@/components/ui'
import { LogOut, ShieldCheck, Briefcase, Headset } from 'lucide-vue-next'
import NotificationBell from './NotificationBell.vue'
import { useRouter } from 'vue-router'
import { motion } from 'motion-v'
import type { UserRole } from '@/types/models'

const auth   = useAuthStore()
const router = useRouter()

interface MockIdentity { role: UserRole; label: string; email: string; icon: any; id: string }
const mockIdentities: MockIdentity[] = [
  { role: 'Admin',   id: '11111111-1111-1111-1111-111111111111', label: 'Admin',   email: 'admin@sofycrm.local',   icon: ShieldCheck },
  { role: 'Sales',   id: '22222222-2222-2222-2222-222222222222', label: 'Sales',   email: 'sales@sofycrm.local',   icon: Briefcase },
  { role: 'Service', id: '33333333-3333-3333-3333-333333333333', label: 'Service', email: 'service@sofycrm.local', icon: Headset },
]

function switchMockIdentity(m: MockIdentity) {
  auth.setSession({
    accessToken:  'mock-access-token',
    refreshToken: 'mock-refresh-token',
    expiresAt:    new Date(Date.now() + 3600_000).toISOString(),
    user: { id: m.id, name: m.label + ' (Mock)', email: m.email, role: m.role, status: 'Active' },
  })
  router.replace('/dashboard')
}

async function logout() {
  await auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="h-14 border-b bg-background/80 backdrop-blur flex items-center px-4 gap-3 sticky top-0 z-10">
    <div class="flex-1" />

    <!-- 快速切換身份 -->
    <motion.div
      :initial="{ opacity: 0, x: 8 }" :animate="{ opacity: 1, x: 0 }"
      class="hidden md:flex items-center gap-1 rounded-md border bg-card p-1"
    >
      <Badge variant="info" class="mr-1">Mock 身份</Badge>
      <button
        v-for="m in mockIdentities" :key="m.role"
        type="button"
        :class="[
          'inline-flex items-center gap-1 rounded-md px-2 py-1 text-xs font-medium transition-colors',
          auth.user?.role === m.role
            ? 'bg-primary text-primary-foreground'
            : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
        ]"
        @click="switchMockIdentity(m)"
      >
        <component :is="m.icon" class="size-3.5" />
        {{ m.label }}
      </button>
    </motion.div>

    <NotificationBell />

    <div class="hidden sm:flex flex-col items-end leading-tight">
      <span class="text-sm font-medium">{{ auth.user?.name }}</span>
      <span class="text-xs text-muted-foreground">{{ auth.user?.role }}</span>
    </div>

    <Button variant="ghost" size="icon" @click="logout" title="登出">
      <LogOut class="size-4" />
    </Button>
  </header>
</template>
