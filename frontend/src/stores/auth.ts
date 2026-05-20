import { defineStore } from 'pinia'
import { http } from '@/api/http'
import { getRepos } from '@/repositories'
import { useDataSourceStore } from './dataSource'
import type { User } from '@/types/models'

const KEY = 'sofycrm.auth'

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  user: User | null
  expiresAt: string | null
}

function load(): AuthState {
  try {
    const raw = localStorage.getItem(KEY)
    if (raw) return JSON.parse(raw)
  } catch { /* ignore */ }
  return { accessToken: null, refreshToken: null, user: null, expiresAt: null }
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => load(),
  getters: {
    isAuthenticated:  (s) => !!s.accessToken && !!s.user,
    isAdmin:          (s) => s.user?.role === 'Admin',
    isSales:          (s) => s.user?.role === 'Sales',
    isService:        (s) => s.user?.role === 'Service',
    displayName:      (s) => s.user?.name ?? 'Guest',
    isExpired:        (s) => !s.expiresAt || new Date(s.expiresAt).getTime() < Date.now(),
  },
  actions: {
    persist() {
      localStorage.setItem(KEY, JSON.stringify(this.$state))
    },
    setSession(data: { accessToken: string; refreshToken: string; user: User; expiresAt: string }) {
      this.accessToken  = data.accessToken
      this.refreshToken = data.refreshToken
      this.user         = data.user
      this.expiresAt    = data.expiresAt
      this.persist()
    },
    async login(email: string, password: string) {
      const data = await getRepos().auth.login(email, password)
      this.setSession(data)
    },
    async refresh(): Promise<string | null> {
      if (!this.refreshToken) return null
      const ds = useDataSourceStore()
      if (ds.isMock) {
        // Mock 模式不需 refresh
        this.expiresAt = new Date(Date.now() + 3600_000).toISOString()
        this.persist()
        return this.accessToken
      }
      try {
        const { data } = await http.post('/auth/refresh', { refreshToken: this.refreshToken })
        this.setSession(data)
        return data.accessToken
      } catch {
        await this.logout()
        return null
      }
    },
    async logout() {
      try {
        const ds = useDataSourceStore()
        if (!ds.isMock && this.refreshToken) {
          await http.post('/auth/logout', { refreshToken: this.refreshToken })
        }
      } catch { /* ignore */ }
      this.$reset()
      localStorage.removeItem(KEY)
    },
    /**
     * Real 模式下檢查 token 是否仍可用：
     *  1. 已過期 → 直接登出
     *  2. 後端 401 → 自動登出
     * Mock 模式則跳過檢查（Demo 用）。
     */
    async verifyToken(): Promise<boolean> {
      const ds = useDataSourceStore()
      if (ds.isMock) return true
      if (!this.accessToken) return false
      if (this.isExpired) {
        await this.logout()
        return false
      }
      try {
        await getRepos().auth.me()
        return true
      } catch {
        await this.logout()
        return false
      }
    },
  },
})
