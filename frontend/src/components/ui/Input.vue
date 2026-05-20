<script setup lang="ts">
import { cn } from '@/lib/utils'

defineProps<{
  modelValue?: string | number | null
  placeholder?: string
  type?: string
  disabled?: boolean
  readonly?: boolean
  error?: string
}>()
defineEmits<{ (e: 'update:modelValue', v: string): void }>()
</script>

<template>
  <div>
    <input
      :type="type ?? 'text'"
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :disabled="disabled"
      :readonly="readonly"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
      :class="cn(
        'flex h-9 w-full rounded-md border bg-background px-3 py-1 text-sm shadow-sm transition-colors',
        'placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
        'disabled:cursor-not-allowed disabled:opacity-50',
        readonly && 'bg-muted text-muted-foreground cursor-not-allowed',
        error ? 'border-rose-500 focus-visible:ring-rose-500' : 'border-input',
      )"
    />
    <p v-if="error" class="text-xs text-rose-600 mt-1">{{ error }}</p>
  </div>
</template>
