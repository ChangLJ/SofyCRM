<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { customersApi } from '@/api/endpoints'
import { Card, Badge, Button } from '@/components/ui'
import PageHeader from '@/components/common/PageHeader.vue'
import { ArrowLeft, Building2, Phone, Mail, MapPin, User2 } from 'lucide-vue-next'
import type { Customer, CustomerContact } from '@/types/models'
import { formatDate, formatMoney } from '@/lib/utils'

const route   = useRoute()
const router  = useRouter()
const customer = ref<Customer | null>(null)
const contacts = ref<CustomerContact[]>([])

onMounted(async () => {
  const id = String(route.params.id)
  customer.value = (await customersApi.get(id)) ?? null
  contacts.value = await customersApi.contacts(id)
})
</script>

<template>
  <Button variant="ghost" size="sm" class="mb-3" @click="router.back()">
    <ArrowLeft class="size-4" /> 返回
  </Button>

  <template v-if="customer">
    <PageHeader :title="customer.companyName" :subtitle="customer.industry ?? '—'">
      <template #actions>
        <Badge variant="info">{{ customer.status }}</Badge>
      </template>
    </PageHeader>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-3">
      <Card class="p-4 lg:col-span-2 space-y-3">
        <h3 class="font-semibold flex items-center gap-2"><Building2 class="size-4" /> 基本資訊</h3>
        <div class="grid grid-cols-2 gap-3 text-sm">
          <div><span class="text-muted-foreground">統編</span><div>{{ customer.taxId || '-' }}</div></div>
          <div><span class="text-muted-foreground">產業</span><div>{{ customer.industry || '-' }}</div></div>
          <div class="col-span-2 flex items-start gap-2">
            <MapPin class="size-4 text-muted-foreground mt-1" />
            <div>{{ customer.address || '-' }}</div>
          </div>
          <div class="col-span-2">
            <span class="text-muted-foreground">標籤</span>
            <div class="flex gap-1 mt-1 flex-wrap">
              <Badge v-for="t in customer.tags" :key="t" variant="outline">{{ t }}</Badge>
            </div>
          </div>
          <div class="col-span-2">
            <span class="text-muted-foreground">備註</span>
            <div class="mt-1 whitespace-pre-line">{{ customer.notes || '-' }}</div>
          </div>
        </div>
      </Card>

      <Card class="p-4">
        <h3 class="font-semibold mb-3">負責人</h3>
        <div class="text-sm flex items-center gap-2">
          <User2 class="size-4 text-muted-foreground" />
          {{ customer.ownerUser?.name ?? '-' }}
        </div>
        <div class="text-xs text-muted-foreground mt-1">{{ customer.ownerUser?.email ?? '' }}</div>
        <div class="mt-4 text-xs text-muted-foreground">
          建立：{{ formatDate(customer.createdAt) }}<br />
          更新：{{ formatDate(customer.updatedAt) }}
        </div>
      </Card>
    </div>

    <Card class="p-4 mt-3">
      <h3 class="font-semibold mb-3">聯絡人</h3>
      <ul class="divide-y">
        <li v-for="c in contacts" :key="c.id" class="py-2.5 flex flex-wrap items-center gap-3 text-sm">
          <span class="font-medium">{{ c.name }}</span>
          <Badge v-if="c.isPrimary" variant="success">主要</Badge>
          <span class="text-muted-foreground">{{ c.title }}</span>
          <span class="text-muted-foreground flex items-center gap-1"><Phone class="size-3.5" /> {{ c.phone || '-' }}</span>
          <span class="text-muted-foreground flex items-center gap-1"><Mail class="size-3.5" /> {{ c.email || '-' }}</span>
        </li>
        <li v-if="!contacts.length" class="py-6 text-center text-muted-foreground text-sm">尚無聯絡人</li>
      </ul>
    </Card>

    <Card class="p-4 mt-3">
      <h3 class="font-semibold mb-3">商機</h3>
      <ul class="divide-y">
        <li v-for="o in customer.opportunities ?? []" :key="o.id" class="py-2.5 flex justify-between items-center text-sm">
          <div>
            <div class="font-medium">{{ o.title }}</div>
            <div class="text-xs text-muted-foreground">{{ o.status }} · 預計成交 {{ formatDate(o.expectedCloseDate) }}</div>
          </div>
          <div class="font-semibold">{{ formatMoney(o.amount) }}</div>
        </li>
        <li v-if="!(customer.opportunities ?? []).length" class="py-6 text-center text-muted-foreground text-sm">尚無商機</li>
      </ul>
    </Card>
  </template>
</template>
