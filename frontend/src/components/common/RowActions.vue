<script setup lang="ts">
import { Button } from '@/components/ui'
import { Pencil, Trash2 } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'

defineProps<{ canDelete?: boolean }>()
const emit = defineEmits<{ (e: 'edit'): void; (e: 'remove'): void }>()

const auth = useAuthStore()
</script>

<template>
  <div class="flex items-center justify-end gap-1">
    <Button variant="ghost" size="icon" title="編輯" @click="emit('edit')">
      <Pencil class="size-4" />
    </Button>
    <Button
      v-if="auth.isAdmin && canDelete !== false"
      variant="ghost" size="icon" title="刪除（Admin）"
      class="text-rose-600 hover:text-rose-700 hover:bg-rose-500/10"
      @click="emit('remove')"
    >
      <Trash2 class="size-4" />
    </Button>
  </div>
</template>
