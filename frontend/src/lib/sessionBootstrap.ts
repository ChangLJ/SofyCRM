/**
 * 啟動時整理登入狀態（固定 Mock 模式）。
 */
import { useAuthStore } from '@/stores/auth'
import { useDataSourceStore } from '@/stores/dataSource'

const AUTH_KEY = 'sofycrm.auth'

function isMockToken(token: string | null | undefined): boolean {
  return !!token && token.startsWith('mock-')
}

export function clearAuthStorage() {
  const auth = useAuthStore()
  auth.accessToken  = null
  auth.refreshToken = null
  auth.user         = null
  auth.expiresAt    = null
  localStorage.removeItem(AUTH_KEY)
}

export function bootstrapSession() {
  const ds = useDataSourceStore()
  ds.init()

  const auth = useAuthStore()
  // 僅允許 Mock token；殘留 Real token 一律清除
  if (auth.accessToken && !isMockToken(auth.accessToken)) {
    clearAuthStorage()
  }
}
