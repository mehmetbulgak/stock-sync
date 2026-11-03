import { ChannelBadge } from '@/components/ChannelBadge'
import { OrderStatusBadge } from '@/components/OrderStatusBadge'
import { Table, Td, Th } from '@/components/ui/Table'
import type { OrderSummary } from '@/lib/api/types'
import { formatCurrency, formatDateTime, formatNumber } from '@/lib/format'

type OrdersTableProps = { orders: OrderSummary[]; onOpen: (orderId: string) => void }

export function OrdersTable({ orders, onOpen }: OrdersTableProps) {
  return (
    <Table>
      <thead>
        <tr>
          <Th>Order</Th>
          <Th>Channel</Th>
          <Th>Customer</Th>
          <Th className="text-right">Items</Th>
          <Th className="text-right">Total</Th>
          <Th>Status</Th>
          <Th>Placed</Th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <tr key={order.id} className="cursor-pointer hover:bg-slate-50/80" onClick={() => onOpen(order.id)}>
            <Td>
              <button
                type="button"
                className="cursor-pointer font-mono text-sm font-medium text-brand-800 hover:underline"
                onClick={(e) => {
                  e.stopPropagation()
                  onOpen(order.id)
                }}
              >
                {order.number}
              </button>
            </Td>
            <Td>
              <ChannelBadge type={order.channelType} name={order.channelName} />
            </Td>
            <Td className="text-slate-700">{order.customerName}</Td>
            <Td className="tabular text-right text-slate-700">{formatNumber(order.itemCount)}</Td>
            <Td className="tabular text-right font-medium text-slate-900">{formatCurrency(order.total)}</Td>
            <Td>
              <OrderStatusBadge status={order.status} />
            </Td>
            <Td className="whitespace-nowrap text-slate-500">{formatDateTime(order.createdAt)}</Td>
          </tr>
        ))}
      </tbody>
    </Table>
  )
}
