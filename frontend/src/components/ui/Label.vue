<script setup lang="ts">
defineProps<{ required?: boolean }>()
</script>

<template>
  <label class="text-sm font-medium block mb-1">
    <slot />
    <span v-if="required" class="text-rose-500 ml-0.5">*</span>
  </label>
</template>
