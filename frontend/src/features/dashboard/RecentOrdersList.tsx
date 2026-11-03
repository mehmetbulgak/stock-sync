import { ShoppingCart } from 'lucide-react'
import { ChannelBadge } from '@/components/ChannelBadge'
import { OrderStatusBadge } from '@/components/OrderStatusBadge'
import { EmptyState } from '@/components/ui/EmptyState'
import type { OrderSummary } from '@/lib/api/types'
import { formatCurrency, formatDateTime } from '@/lib/format'

export function RecentOrdersList({ orders }: { orders: OrderSummary[] }) {
  if (orders.length === 0) {
    return <EmptyState icon={<ShoppingCart />} title="No orders yet" description="Orders from every channel will appear here." />
  }

  return (
    <ul className="divide-y divide-slate-100">
      {orders.map((order) => (
        <li key={order.id} className="flex items-center justify-between gap-4 px-5 py-3">
          <div className="min-w-0">
            <p className="truncate text-sm font-medium text-slate-900">{order.customerName}</p>
            <div className="mt-0.5 flex items-center gap-2 text-xs text-slate-500">
              <ChannelBadge type={order.channelType} name={order.channelName} />
              <span aria-hidden>·</span>
              <span>{formatDateTime(order.createdAt)}</span>
            </div>
          </div>
          <div className="flex shrink-0 flex-col items-end gap-1">
            <span className="tabular text-sm font-semibold text-slate-900">{formatCurrency(order.total)}</span>
            <OrderStatusBadge status={order.status} />
          </div>
        </li>
      ))}
    </ul>
  )
}
