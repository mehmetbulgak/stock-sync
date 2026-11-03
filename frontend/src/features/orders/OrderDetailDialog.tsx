import { AlertTriangle, Truck, X } from 'lucide-react'
import { type ReactNode, useState } from 'react'
import { toast } from 'sonner'
import { ChannelBadge } from '@/components/ChannelBadge'
import { OrderStatusBadge } from '@/components/OrderStatusBadge'
import { Button } from '@/components/ui/Button'
import { Dialog, DialogBody, DialogFooter } from '@/components/ui/Dialog'
import { EmptyState } from '@/components/ui/EmptyState'
import { Skeleton } from '@/components/ui/Skeleton'
import { describeError } from '@/lib/api/errors'
import type { Order } from '@/lib/api/types'
import { cn } from '@/lib/cn'
import { formatCurrency, formatDateTime, formatNumber } from '@/lib/format'
import { type OrderTransition, useOrder, useOrderTransition } from './api'

const SELLER_CANCELLATION_REASON = 'Cancelled by seller'

type OrderDetailDialogProps = { orderId: string | null; onClose: () => void }

export function OrderDetailDialog({ orderId, onClose }: OrderDetailDialogProps) {
  const { data: order, isPending, isError } = useOrder(orderId)

  return (
    <Dialog open={orderId !== null} onOpenChange={(next) => !next && onClose()} title={order ? `Order ${order.number}` : 'Order'} size="lg">
      {isError ? (
        <DialogBody>
          <EmptyState icon={<AlertTriangle />} title="Couldn't load this order" />
        </DialogBody>
      ) : isPending ? (
        <DialogBody>
          <div className="grid gap-3">
            <Skeleton className="h-16" />
            <Skeleton className="h-40" />
          </div>
        </DialogBody>
      ) : (
        <OrderDetail key={order.id} order={order} />
      )}
    </Dialog>
  )
}

function OrderDetail({ order }: { order: Order }) {
  const transition = useOrderTransition(order.id)
  const [confirmCancel, setConfirmCancel] = useState(false)
  const pending = order.status === 'Pending'

  const run = async (request: OrderTransition) => {
    try {
      await transition.mutateAsync(request)
      toast.success(request.action === 'ship' ? `${order.number} marked as shipped` : `${order.number} cancelled, stock released`)
      setConfirmCancel(false)
    } catch (e) {
      toast.error(describeError(e))
    }
  }

  return (
    <>
      <DialogBody>
        <div className="grid gap-5">
          <dl className="grid grid-cols-2 gap-4 text-sm sm:grid-cols-4">
            <Meta label="Status">
              <OrderStatusBadge status={order.status} />
            </Meta>
            <Meta label="Channel">
              <ChannelBadge type={order.channelType} name={order.channelName} />
            </Meta>
            <Meta label="Customer">{order.customerName}</Meta>
            <Meta label="Placed">{formatDateTime(order.createdAt)}</Meta>
            {order.shippedAt && <Meta label="Shipped">{formatDateTime(order.shippedAt)}</Meta>}
            {order.cancelledAt && <Meta label="Cancelled">{formatDateTime(order.cancelledAt)}</Meta>}
            {order.cancellationReason && <Meta label="Reason">{order.cancellationReason}</Meta>}
          </dl>

          <OrderLinesTable order={order} />

          {pending && (
            <p className="rounded-lg bg-amber-50 px-3 py-2 text-sm text-amber-800">
              These units are reserved: they no longer count as available on any channel until this order ships or is cancelled.
            </p>
          )}
        </div>
      </DialogBody>

      {pending && (
        <DialogFooter className={cn(confirmCancel && 'justify-between')}>
          {confirmCancel ? (
            <>
              <p className="self-center text-sm text-slate-600">Cancel this order and return its units to stock?</p>
              <div className="flex gap-2">
                <Button variant="secondary" onClick={() => setConfirmCancel(false)}>
                  Keep order
                </Button>
                <Button
                  variant="danger"
                  loading={transition.isPending}
                  onClick={() => void run({ action: 'cancel', reason: SELLER_CANCELLATION_REASON })}
                >
                  Cancel order
                </Button>
              </div>
            </>
          ) : (
            <>
              <Button variant="danger-outline" onClick={() => setConfirmCancel(true)}>
                <X /> Cancel order
              </Button>
              <Button loading={transition.isPending} onClick={() => void run({ action: 'ship' })}>
                <Truck /> Mark as shipped
              </Button>
            </>
          )}
        </DialogFooter>
      )}
    </>
  )
}

function OrderLinesTable({ order }: { order: Order }) {
  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200">
      <table className="w-full text-sm">
        <thead className="bg-slate-50 text-left text-xs text-slate-500 uppercase">
          <tr>
            <th className="px-4 py-2 font-medium">Product</th>
            <th className="px-4 py-2 text-right font-medium">Qty</th>
            <th className="px-4 py-2 text-right font-medium">Unit price</th>
            <th className="px-4 py-2 text-right font-medium">Total</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {order.lines.map((line) => (
            <tr key={line.productId}>
              <td className="px-4 py-2.5">
                <p className="font-medium text-slate-900">{line.productName}</p>
                <p className="font-mono text-xs text-slate-500">{line.sku}</p>
              </td>
              <td className="tabular px-4 py-2.5 text-right">{formatNumber(line.quantity)}</td>
              <td className="tabular px-4 py-2.5 text-right text-slate-600">{formatCurrency(line.unitPrice)}</td>
              <td className="tabular px-4 py-2.5 text-right font-medium">{formatCurrency(line.lineTotal)}</td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr className="bg-slate-50">
            <td colSpan={3} className="px-4 py-2.5 text-right text-sm font-medium text-slate-600">
              Order total
            </td>
            <td className="tabular px-4 py-2.5 text-right text-base font-semibold text-slate-900">{formatCurrency(order.total)}</td>
          </tr>
        </tfoot>
      </table>
    </div>
  )
}

function Meta({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className="mt-1 text-slate-900">{children}</dd>
    </div>
  )
}
