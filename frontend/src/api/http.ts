import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api/v1'

export const http = axios.create({
  baseURL,
  timeout: 30000,
})

http.interceptors.request.use((cfg) => {
  const auth = useAuthStore()
  if (auth.accessToken) {
    cfg.headers.Authorization = `Bearer ${auth.accessToken}`
  }
  return cfg
})

let refreshing: Promise<string | null> | null = null

http.interceptors.response.use(
  (r) => r,
  async (err) => {
    const original = err.config
    const auth = useAuthStore()
    if (err.response?.status === 401 && !original._retry && auth.refreshToken) {
      original._retry = true
      refreshing ||= auth.refresh()
      const newToken = await refreshing
      refreshing = null
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`
        return http(original)
      }
    }
    return Promise.reject(err)
  },
)
