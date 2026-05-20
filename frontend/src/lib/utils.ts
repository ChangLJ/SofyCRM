import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatMoney(n: number | string | null | undefined) {
  if (n === null || n === undefined || n === '') return '-'
  const v = typeof n === 'string' ? parseFloat(n) : n
  if (Number.isNaN(v)) return '-'
  return new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', maximumFractionDigits: 0 }).format(v)
}

export function formatDate(d: string | Date | null | undefined) {
  if (!d) return '-'
  const dt = typeof d === 'string' ? new Date(d) : d
  if (Number.isNaN(dt.getTime())) return '-'
  return dt.toISOString().slice(0, 10)
}

export function formatDateTime(d: string | Date | null | undefined) {
  if (!d) return '-'
  const dt = typeof d === 'string' ? new Date(d) : d
  if (Number.isNaN(dt.getTime())) return '-'
  return dt.toLocaleString('zh-TW', { hour12: false })
}
