<script setup lang="ts">
import { computed } from 'vue'
import { cn } from '@/lib/utils'

const props = withDefaults(defineProps<{
  variant?: 'default' | 'secondary' | 'outline' | 'success' | 'warning' | 'danger' | 'info'
}>(), { variant: 'default' })

const variants: Record<string, string> = {
  default:   'border-transparent bg-primary text-primary-foreground',
  secondary: 'border-transparent bg-secondary text-secondary-foreground',
  outline:   'text-foreground',
  success:   'border-transparent bg-emerald-500/15 text-emerald-700 dark:text-emerald-300',
  warning:   'border-transparent bg-amber-500/15 text-amber-700 dark:text-amber-300',
  danger:    'border-transparent bg-rose-500/15 text-rose-700 dark:text-rose-300',
  info:      'border-transparent bg-sky-500/15 text-sky-700 dark:text-sky-300',
}

const cls = computed(() => cn(
  'inline-flex items-center rounded-md border px-2 py-0.5 text-xs font-semibold',
  variants[props.variant]
))
</script>

<template><span :class="cls"><slot /></span></template>
