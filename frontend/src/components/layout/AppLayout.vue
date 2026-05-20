<script setup lang="ts">
import Sidebar from './Sidebar.vue'
import Navbar  from './Navbar.vue'
import { motion } from 'motion-v'
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <div class="flex min-h-screen w-full">
    <Sidebar />
    <div class="flex-1 flex flex-col min-w-0">
      <Navbar />
      <main class="flex-1 p-4 md:p-6 bg-muted/30 min-w-0">
        <motion.div
          :key="route.fullPath"
          :initial="{ opacity: 0, y: 12 }"
          :animate="{ opacity: 1, y: 0 }"
          :transition="{ duration: 0.25 }"
        >
          <RouterView />
        </motion.div>
      </main>
    </div>
  </div>
</template>
