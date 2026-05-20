<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { Button, Input, Card, Badge } from '@/components/ui'
import { Building2, LogIn, ShieldCheck, Briefcase, Headset } from 'lucide-vue-next'
import { motion } from 'motion-v'
import type { UserRole } from '@/types/models'

const router = useRouter()
const route  = useRoute()
const auth   = useAuthStore()

const email    = ref('Admin')
const password = ref('Admin@123')
const loading  = ref(false)
const error    = ref<string | null>(null)

interface MockIdentity {
  role: UserRole
  label: string
  email: string
  desc: string
  icon: any
}

const mockIdentities: MockIdentity[] = [
  { role: 'Admin',   label: 'Admin 管理員', email: 'admin@sofycrm.local',   desc: '全部資料 + 使用者管理',  icon: ShieldCheck },
  { role: 'Sales',   label: 'Sales 業務',   email: 'sales@sofycrm.local',   desc: '自己客戶 / 商機 / 報價', icon: Briefcase },
  { role: 'Service', label: 'Service 客服', email: 'service@sofycrm.local', desc: '被指派客戶 / 案件 / 工單', icon: Headset },
]

const mockUserIdByRole: Record<UserRole, string> = {
  Admin:   '11111111-1111-1111-1111-111111111111',
  Sales:   '22222222-2222-2222-2222-222222222222',
  Service: '33333333-3333-3333-3333-333333333333',
}

function loginAsMock(identity: MockIdentity) {
  auth.setSession({
    accessToken:  'mock-access-token',
    refreshToken: 'mock-refresh-token',
    expiresAt:    new Date(Date.now() + 3600_000).toISOString(),
    user: {
      id: mockUserIdByRole[identity.role],
      name: identity.label + ' (Mock)',
      email: identity.email,
      role: identity.role,
      status: 'Active',
    },
  })
  router.push((route.query.redirect as string) || '/dashboard')
}

async function onLogin() {
  error.value = null
  loading.value = true
  try {
    const role: UserRole =
      email.value.toLowerCase().startsWith('sales')   ? 'Sales'   :
      email.value.toLowerCase().startsWith('service') ? 'Service' : 'Admin'
    loginAsMock({
      role,
      label: role,
      email: email.value,
      desc: '',
      icon: ShieldCheck,
    })
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <motion.div class="min-h-screen w-full flex items-center justify-center bg-gradient-to-br from-background via-muted/30 to-background p-4">
    <motion.div
      class="w-full max-w-md"
      :initial="{ opacity: 0, y: 16 }" :animate="{ opacity: 1, y: 0 }" :transition="{ duration: 0.35 }"
    >
      <Card class="p-6 sm:p-8">
        <motion.div class="flex items-center gap-2 mb-6" :initial="{ opacity: 0, y: -8 }" :animate="{ opacity: 1, y: 0 }">
          <Building2 class="size-6 text-primary" />
          <span class="text-xl font-semibold tracking-tight">SofyCRM</span>
        </motion.div>

        <h2 class="text-lg font-semibold mb-1">登入您的帳號</h2>
        <p class="text-sm text-muted-foreground mb-6">中小型軟體專案公司 CRM · Mock Data 示範模式</p>

        <motion.div
          :initial="{ opacity: 0, y: -8 }" :animate="{ opacity: 1, y: 0 }"
          class="mb-5"
        >
          <div class="flex items-center gap-2 mb-2">
            <Badge variant="info">Mock 模式</Badge>
            <span class="text-xs text-muted-foreground">點擊以該身份直接登入</span>
          </div>
          <div class="space-y-2">
            <button
              v-for="m in mockIdentities" :key="m.role"
              type="button"
              class="w-full flex items-center gap-3 rounded-md border p-3 bg-card hover:bg-accent hover:text-accent-foreground transition-colors text-left"
              @click="loginAsMock(m)"
            >
              <component :is="m.icon" class="size-5 text-primary shrink-0" />
              <div class="flex-1 min-w-0">
                <div class="text-sm font-medium">{{ m.label }}</div>
                <div class="text-xs text-muted-foreground truncate">{{ m.desc }}</div>
              </div>
              <LogIn class="size-4 text-muted-foreground" />
            </button>
          </div>

          <div class="my-4 flex items-center gap-3">
            <div class="flex-1 h-px bg-border" />
            <span class="text-xs text-muted-foreground">或自訂帳號</span>
            <div class="flex-1 h-px bg-border" />
          </div>
        </motion.div>

        <form class="space-y-4" @submit.prevent="onLogin">
          <div>
            <label class="text-sm font-medium">帳號</label>
            <Input v-model="email" type="text" placeholder="Admin / sales / service" class="mt-1" />
          </div>
          <div>
            <label class="text-sm font-medium">密碼</label>
            <Input v-model="password" type="password" placeholder="（Mock 模式任意密碼）" class="mt-1" />
          </div>

          <div v-if="error" class="text-sm text-rose-600">{{ error }}</div>

          <Button type="submit" class="w-full" :disabled="loading">
            <LogIn class="size-4" /> {{ loading ? '登入中…' : '登入' }}
          </Button>
        </form>
      </Card>
    </motion.div>
  </motion.div>
</template>
