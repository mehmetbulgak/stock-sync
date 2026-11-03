import type { OrderStatus } from '@/lib/api/types'
import { Badge, type BadgeTone } from './ui/Badge'

const statusTone: Record<OrderStatus, BadgeTone> = { Pending: 'warning', Shipped: 'success', Cancelled: 'neutral' }

export function OrderStatusBadge({ status }: { status: OrderStatus }) {
  return <Badge tone={statusTone[status]}>{status}</Badge>
}
