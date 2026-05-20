import { defineStore } from 'pinia'
import { getRepos } from '@/repositories'
import type { Notification } from '@/types/models'

export const useNotificationsStore = defineStore('notifications', {
  state: () => ({
    items: [] as Notification[],
    loading: false,
    lastFetched: 0,
  }),
  getters: {
    unreadCount: (s) => s.items.filter(n => !n.isRead).length,
    hasUnread:   (s) => s.items.some(n => !n.isRead),
  },
  actions: {
    async refresh() {
      if (this.loading) return
      this.loading = true
      try {
        const r = await getRepos().notifications.list({ pageSize: 100 })
        this.items = r.items
        this.lastFetched = Date.now()
      } catch {
        // 靜默失敗
      } finally {
        this.loading = false
      }
    },
    async markRead(id: string) {
      await getRepos().notifications.markRead(id)
      const n = this.items.find(x => x.id === id)
      if (n) n.isRead = true
    },
    async markAllRead() {
      await getRepos().notifications.markAllRead()
      this.items.forEach(n => n.isRead = true)
    },
  },
})
