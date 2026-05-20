<script setup lang="ts">
import Dialog from './Dialog.vue'
import Button from './Button.vue'
import { AlertTriangle } from 'lucide-vue-next'

const props = withDefaults(defineProps<{
  modelValue: boolean
  title?: string
  message?: string
  confirmText?: string
  cancelText?: string
  variant?: 'default' | 'destructive'
}>(), {
  title: '確認操作',
  confirmText: '確認',
  cancelText: '取消',
  variant: 'destructive',
})

const emit = defineEmits<{
  (e: 'update:modelValue', v: boolean): void
  (e: 'confirm'): void
}>()

function cancel() { emit('update:modelValue', false) }
function ok() { emit('confirm'); emit('update:modelValue', false) }
</script>

<template>
  <Dialog :model-value="modelValue" @update:model-value="cancel" :title="title" :max-width="'24rem'">
    <div class="flex items-start gap-3">
      <AlertTriangle :class="['size-5 mt-0.5 shrink-0', variant === 'destructive' ? 'text-rose-500' : 'text-amber-500']" />
      <p class="text-sm text-muted-foreground">{{ message }}</p>
    </div>
    <template #footer>
      <Button variant="outline" @click="cancel">{{ cancelText }}</Button>
      <Button :variant="variant === 'destructive' ? 'destructive' : 'default'" @click="ok">{{ confirmText }}</Button>
    </template>
  </Dialog>
</template>
