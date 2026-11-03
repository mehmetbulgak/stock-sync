import { AlertTriangle, Inbox, Plus } from 'lucide-react'
import { useState } from 'react'
import { SearchInput } from '@/components/SearchInput'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { Select } from '@/components/ui/FormControls'
import { PageHeader } from '@/components/ui/PageHeader'
import { Pagination } from '@/components/ui/Pagination'
import { SkeletonRows } from '@/components/ui/Skeleton'
import type { OrderStatus } from '@/lib/api/types'
import { cn } from '@/lib/cn'
import { useDebouncedValue } from '@/lib/hooks/useDebouncedValue'
import { useUrlFilters } from '@/lib/hooks/useUrlFilters'
import { useChannels } from '@/lib/query/channels'
import { useOrders } from './api'
import { CreateOrderDialog } from './CreateOrderDialog'
import { OrderDetailDialog } from './OrderDetailDialog'
import { OrderStatusTabs } from './OrderStatusTabs'
import { OrdersTable } from './OrdersTable'
import { ORDER_STATUSES } from './schemas'

const PAGE_SIZE = 15

const parseStatus = (value: string | null): OrderStatus | null => ORDER_STATUSES.find((s) => s === value) ?? null

export default function OrdersPage() {
  const { params, update, page } = useUrlFilters()
  const [search, setSearch] = useState(params.get('search') ?? '')
  const debouncedSearch = useDebouncedValue(search.trim())
  const [creating, setCreating] = useState(false)
  const [openOrderId, setOpenOrderId] = useState<string | null>(null)
  const channels = useChannels()

  const status = parseStatus(params.get('status'))
  const channelId = params.get('channel')
  const { data, isPending, isError, isFetching } = useOrders({
    status: status ?? undefined,
    channelId: channelId ?? undefined,
    search: debouncedSearch || undefined,
    page,
    pageSize: PAGE_SIZE,
  })

  return (
    <>
      <PageHeader
        title="Orders"
        description="Every channel in one queue. Stock is reserved as soon as an order arrives."
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus /> New order
          </Button>
        }
      />

      <Card>
        <div className="flex flex-wrap items-center gap-3 border-b border-slate-100 p-4">
          <OrderStatusTabs value={status} onChange={(next) => update({ status: next })} />
          <Select
            aria-label="Channel"
            className="w-44"
            value={channelId ?? ''}
            onChange={(e) => update({ channel: e.target.value || null })}
          >
            <option value="">All channels</option>
            {channels.data?.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
          <SearchInput
            label="Search orders"
            placeholder="Order number or customer"
            className="min-w-56 flex-1"
            maxLength={100}
            value={search}
            onChange={(e) => {
              setSearch(e.target.value)
              update({ search: e.target.value || null })
            }}
          />
        </div>

        {isError ? (
          <EmptyState icon={<AlertTriangle />} title="Couldn't load orders" description="Check your connection and try again." />
        ) : isPending ? (
          <SkeletonRows />
        ) : data.items.length === 0 ? (
          <EmptyState icon={<Inbox />} title="No orders here" description="Try another status or channel, or create a manual order." />
        ) : (
          <div className={cn('transition-opacity', isFetching && 'opacity-60')}>
            <OrdersTable orders={data.items} onOpen={setOpenOrderId} />
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              onPageChange={(next) => update({ page: String(next) })}
            />
          </div>
        )}
      </Card>

      <CreateOrderDialog open={creating} onClose={() => setCreating(false)} onCreated={setOpenOrderId} />
      <OrderDetailDialog orderId={openOrderId} onClose={() => setOpenOrderId(null)} />
    </>
  )
}
