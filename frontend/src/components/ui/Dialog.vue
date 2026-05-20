<script setup lang="ts">
import { watch } from 'vue'
import { motion, AnimatePresence } from 'motion-v'
import { X } from 'lucide-vue-next'

const props = defineProps<{
  modelValue: boolean
  title?: string
  description?: string
  maxWidth?: string
  hideClose?: boolean
}>()
const emit = defineEmits<{ (e: 'update:modelValue', v: boolean): void }>()

function close() { emit('update:modelValue', false) }

// 不支援 ESC 或背景點擊關閉 —— 只能按取消 / X 按鈕關閉，避免誤觸丟失輸入

watch(() => props.modelValue, (v) => {
  document.body.style.overflow = v ? 'hidden' : ''
})
</script>

<template>
  <Teleport to="body">
    <AnimatePresence>
      <motion.div
        v-if="modelValue"
        :initial="{ opacity: 0 }" :animate="{ opacity: 1 }" :exit="{ opacity: 0 }"
        :transition="{ duration: 0.18 }"
        class="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm flex items-start justify-center p-4 overflow-y-auto"
      >
        <motion.div
          :initial="{ opacity: 0, scale: 0.96, y: 10 }"
          :animate="{ opacity: 1, scale: 1, y: 0 }"
          :exit="{ opacity: 0, scale: 0.96, y: 10 }"
          :transition="{ duration: 0.2 }"
          class="bg-card border rounded-xl shadow-xl w-full mt-16 mb-8 relative"
          :style="{ maxWidth: maxWidth || '32rem' }"
        >
          <div v-if="title || !hideClose" class="flex items-start justify-between gap-4 px-5 pt-5 pb-3">
            <div>
              <h3 v-if="title" class="text-lg font-semibold tracking-tight">{{ title }}</h3>
              <p v-if="description" class="text-sm text-muted-foreground mt-1">{{ description }}</p>
            </div>
            <button v-if="!hideClose" type="button"
                    class="rounded-md p-1 text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                    @click="close">
              <X class="size-4" />
            </button>
          </div>
          <div class="px-5 pb-5">
            <slot />
          </div>
          <div v-if="$slots.footer" class="px-5 py-3 border-t bg-muted/30 rounded-b-xl flex justify-end gap-2">
            <slot name="footer" />
          </div>
        </motion.div>
      </motion.div>
    </AnimatePresence>
  </Teleport>
</template>
