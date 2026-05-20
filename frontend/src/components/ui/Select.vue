<script setup lang="ts">
import { cn } from '@/lib/utils'

interface Option { label: string; value: string | number }

defineProps<{
  modelValue: string | number | null | undefined
  options: Option[]
  placeholder?: string
  disabled?: boolean
  error?: string
}>()
defineEmits<{ (e: 'update:modelValue', v: string): void }>()
</script>

<template>
  <div>
    <select
      :value="(modelValue ?? '') as any"
      :disabled="disabled"
      @change="$emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
      :class="cn(
        'flex h-9 w-full rounded-md border bg-background px-3 py-1 text-sm shadow-sm',
        'focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
        'disabled:cursor-not-allowed disabled:opacity-50',
        error ? 'border-rose-500 focus-visible:ring-rose-500' : 'border-input',
      )"
    >
      <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
      <option v-for="o in options" :key="o.value" :value="o.value">{{ o.label }}</option>
    </select>
    <p v-if="error" class="text-xs text-rose-600 mt-1">{{ error }}</p>
  </div>
</template>
