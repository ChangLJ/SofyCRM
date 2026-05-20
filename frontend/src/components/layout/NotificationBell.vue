<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useNotificationsStore } from '@/stores/notifications'
import { Button, Badge } from '@/components/ui'
import { Bell, BellRing, Check, CalendarClock, AlertOctagon, Info } from 'lucide-vue-next'
import { motion, AnimatePresence } from 'motion-v'
import type { Notification } from '@/types/models'

const router = useRouter()
const store  = useNotificationsStore()
const open   = ref(false)

const containerRef = ref<HTMLElement | null>(null)

function toggle() { open.value = !open.value; if (open.value) store.refresh() }
function closeIfOutside(e: MouseEvent) {
  if (containerRef.value && !containerRef.value.contains(e.target as Node)) open.value = false
}

let timer: number | null = null
onMounted(async () => {
  await store.refresh()
  // 每 60 秒輪詢一次新通知
  timer = window.setInterval(() => store.refresh(), 60_000)
  document.addEventListener('click', closeIfOutside)
})
onUnmounted(() => {
  if (timer) window.clearInterval(timer)
  document.removeEventListener('click', closeIfOutside)
})

function iconOf(n: Notification) {
  switch (n.type) {
    case 'ContractExpiring': return CalendarClock
    case 'ContractOverdue':  return AlertOctagon
    case 'SlaWarning':       return AlertOctagon
    case 'FollowupReminder': return Bell
    default:                 return Info
  }
}
function colorOf(n: Notification) {
  switch (n.type) {
    case 'ContractOverdue':
    case 'SlaWarning':       return 'text-rose-500'
    case 'ContractExpiring': return 'text-amber-500'
    case 'FollowupReminder': return 'text-sky-500'
    default:                 return 'text-muted-foreground'
  }
}

async function clickNotification(n: Notification) {
  if (!n.isRead) await store.markRead(n.id)
  if (n.link) {
    open.value = false
    router.push(n.link)
  }
}
async function markAll() { await store.markAllRead() }

const top = computed(() => store.items.slice(0, 12))
</script>

<template>
  <div ref="containerRef" class="relative">
    <Button variant="ghost" size="icon" :title="`${store.unreadCount} 則未讀`" @click.stop="toggle">
      <motion.span
        v-if="store.hasUnread"
        :animate="{ scale: [1, 1.12, 1] }"
        :transition="{ duration: 1.4, repeat: Infinity }"
        class="relative inline-flex"
      >
        <BellRing class="size-4 text-rose-500" />
        <span class="absolute -top-1 -right-1 inline-flex h-2 w-2">
          <span class="absolute inline-flex h-full w-full rounded-full bg-rose-500 opacity-75 animate-ping" />
          <span class="relative inline-flex h-2 w-2 rounded-full bg-rose-500" />
        </span>
      </motion.span>
      <Bell v-else class="size-4" />

      <span
        v-if="store.unreadCount > 0"
        class="absolute -top-0.5 -right-0.5 inline-flex items-center justify-center rounded-full bg-rose-500 text-[10px] font-semibold text-white px-1.5 min-w-[18px] h-[18px] leading-none"
      >
        {{ store.unreadCount > 99 ? '99+' : store.unreadCount }}
      </span>
    </Button>

    <AnimatePresence>
      <motion.div
        v-if="open"
        :initial="{ opacity: 0, y: -4, scale: 0.98 }"
        :animate="{ opacity: 1, y: 0, scale: 1 }"
        :exit="{ opacity: 0, y: -4, scale: 0.98 }"
        :transition="{ duration: 0.15 }"
        class="absolute right-0 mt-2 w-[360px] max-h-[480px] overflow-hidden rounded-lg border bg-card shadow-xl z-50 flex flex-col"
        @click.stop
      >
        <div class="flex items-center justify-between px-4 py-3 border-b">
          <div class="flex items-center gap-2">
            <span class="text-sm font-semibold">通知</span>
            <Badge v-if="store.unreadCount > 0" variant="danger">{{ store.unreadCount }} 未讀</Badge>
          </div>
          <Button v-if="store.hasUnread" variant="ghost" size="sm" @click="markAll">
            <Check class="size-3.5" /> 全部已讀
          </Button>
        </div>

        <div class="flex-1 overflow-y-auto divide-y">
          <div v-if="top.length === 0" class="p-8 text-center text-sm text-muted-foreground">
            目前沒有通知
          </div>
          <button
            v-for="n in top" :key="n.id"
            type="button"
            class="w-full flex items-start gap-3 p-3 text-left hover:bg-accent transition-colors"
            :class="{ 'bg-rose-500/5': !n.isRead }"
            @click="clickNotification(n)"
          >
            <component :is="iconOf(n)" :class="['size-4 mt-0.5 shrink-0', colorOf(n)]" />
            <div class="flex-1 min-w-0">
              <div class="text-sm font-medium flex items-center gap-2">
                {{ n.title }}
                <span v-if="!n.isRead" class="size-1.5 rounded-full bg-rose-500" />
              </div>
              <div class="text-xs text-muted-foreground mt-0.5 line-clamp-2">{{ n.message }}</div>
              <div class="text-[10px] text-muted-foreground mt-1">
                {{ new Date(n.createdAt).toLocaleString('zh-TW', { hour12: false }) }}
              </div>
            </div>
          </button>
        </div>
      </motion.div>
    </AnimatePresence>
  </div>
</template>
