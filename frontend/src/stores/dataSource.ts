import { defineStore } from 'pinia'

const KEY = 'sofycrm.dataSource'

/** 固定 Mock Data 模式（不再提供 Real API 切換） */
export const useDataSourceStore = defineStore('dataSource', {
  state: () => ({
    mode: 'mock' as const,
  }),
  getters: {
    isMock: () => true,
    isReal: () => false,
  },
  actions: {
    /** 保留介面相容；固定 mock，不執行任何切換 */
    async toggle() { /* noop */ },
    async set(_mode: 'mock' | 'real') {
      localStorage.setItem(KEY, 'mock')
    },
    init() {
      localStorage.setItem(KEY, 'mock')
    },
  },
})
