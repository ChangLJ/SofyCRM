/**
 * Repository Factory — view 只需呼叫 `useRepos()` 取得抽象介面實例。
 * 固定使用 Mock Data 實作（不再切換 Real API）。
 */
import { computed } from 'vue'
import type { IRepos } from './types'
import { mockRepos } from './mock'

export type { IRepos, Page, QueryParams } from './types'

export function useRepos() {
  return computed<IRepos>(() => mockRepos)
}

/** 在 store / interceptor 等非 component 場景使用 */
export function getRepos(): IRepos {
  return mockRepos
}
