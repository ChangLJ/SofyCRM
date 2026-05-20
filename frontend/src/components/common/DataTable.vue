<script setup lang="ts" generic="T extends Record<string, any>">
import { motion } from 'motion-v'

interface Column<TItem> {
  key: keyof TItem | string
  label: string
  width?: string
  align?: 'left' | 'center' | 'right'
}

defineProps<{
  columns: Column<T>[]
  rows: T[]
  empty?: string
  rowKey?: (row: T) => string | number
}>()

defineSlots<{
  cell(props: { row: T; col: Column<T> }): any
}>()
</script>

<template>
  <div class="overflow-x-auto rounded-lg border bg-card">
    <table class="w-full text-sm">
      <thead class="bg-muted/50">
        <tr>
          <th
            v-for="col in columns" :key="String(col.key)"
            :style="col.width ? { width: col.width } : undefined"
            :class="['px-3 py-2.5 text-left font-medium text-muted-foreground',
              col.align === 'right' ? 'text-right'
              : col.align === 'center' ? 'text-center' : '']"
          >{{ col.label }}</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="rows.length === 0">
          <td :colspan="columns.length" class="px-4 py-10 text-center text-muted-foreground">
            {{ empty || '無資料' }}
          </td>
        </tr>
        <motion.tr
          v-for="(row, idx) in rows" :key="rowKey ? rowKey(row) : idx"
          :initial="{ opacity: 0, y: 4 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ delay: idx * 0.015 }"
          class="border-t hover:bg-muted/40 transition-colors"
        >
          <td
            v-for="col in columns" :key="String(col.key)"
            :class="['px-3 py-2.5',
              col.align === 'right' ? 'text-right'
              : col.align === 'center' ? 'text-center' : '']"
          >
            <slot name="cell" :row="row" :col="col">
              {{ (row as any)[col.key] }}
            </slot>
          </td>
        </motion.tr>
      </tbody>
    </table>
  </div>
</template>
