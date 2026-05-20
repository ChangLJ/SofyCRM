<script setup lang="ts">
import { cn } from '@/lib/utils'

defineProps<{
  modelValue?: string | null
  placeholder?: string
  rows?: number
  disabled?: boolean
  error?: string
}>()
defineEmits<{ (e: 'update:modelValue', v: string): void }>()
</script>

<template>
  <div>
    <textarea
      :value="modelValue ?? ''"
      :placeholder="placeholder"
      :rows="rows ?? 3"
      :disabled="disabled"
      @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
      :class="cn(
        'flex min-h-[60px] w-full rounded-md border bg-background px-3 py-2 text-sm shadow-sm',
        'placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring',
        'disabled:cursor-not-allowed disabled:opacity-50',
        error ? 'border-rose-500 focus-visible:ring-rose-500' : 'border-input',
      )"
    />
    <p v-if="error" class="text-xs text-rose-600 mt-1">{{ error }}</p>
  </div>
</template>
