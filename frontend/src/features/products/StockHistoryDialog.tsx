import { AlertTriangle, History } from 'lucide-react'
import { Badge, type BadgeTone } from '@/components/ui/Badge'
import { Dialog, DialogBody } from '@/components/ui/Dialog'
import { EmptyState } from '@/components/ui/EmptyState'
import { Skeleton } from '@/components/ui/Skeleton'
import type { Product, StockMovement, StockMovementType } from '@/lib/api/types'
import { cn } from '@/lib/cn'
import { formatDateTime, formatNumber } from '@/lib/format'
import { useMovements } from './api'

const movementStyles: Record<StockMovementType, { label: string; tone: BadgeTone }> = {
  InitialStock: { label: 'Opening stock', tone: 'neutral' },
  Adjustment: { label: 'Adjustment', tone: 'info' },
  Reservation: { label: 'Reserved', tone: 'warning' },
  ReservationReleased: { label: 'Released', tone: 'neutral' },
  Shipment: { label: 'Shipped', tone: 'success' },
}

const INCOMING_TYPES: ReadonlySet<StockMovementType> = new Set(['Adjustment', 'InitialStock'])

type StockHistoryDialogProps = { product: Product | null; onClose: () => void }

export function StockHistoryDialog({ product, onClose }: StockHistoryDialogProps) {
  return (
    <Dialog
      open={product !== null}
      onOpenChange={(next) => !next && onClose()}
      title="Stock history"
      description={product ? `${product.name} · every change to on-hand and reserved units` : undefined}
      size="lg"
    >
      <DialogBody>{product && <MovementLedger productId={product.id} />}</DialogBody>
    </Dialog>
  )
}

function MovementLedger({ productId }: { productId: string }) {
  const { data, isPending, isError } = useMovements(productId)

  if (isError) return <EmptyState icon={<AlertTriangle />} title="Couldn't load the stock history" />

  if (isPending) {
    return (
      <div className="grid gap-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-10" />
        ))}
      </div>
    )
  }

  if (data.length === 0) return <EmptyState icon={<History />} title="No stock changes yet" />

  return (
    <table className="w-full text-sm">
      <thead>
        <tr className="text-left text-xs text-slate-500 uppercase">
          <th className="pb-2 font-medium">When</th>
          <th className="pb-2 font-medium">Change</th>
          <th className="pb-2 text-right font-medium">Qty</th>
          <th className="pb-2 text-right font-medium">On hand</th>
          <th className="pb-2 text-right font-medium">Reserved</th>
        </tr>
      </thead>
      <tbody className="divide-y divide-slate-100">
        {data.map((movement) => (
          <MovementRow key={movement.id} movement={movement} />
        ))}
      </tbody>
    </table>
  )
}

function MovementRow({ movement }: { movement: StockMovement }) {
  const style = movementStyles[movement.type]
  const incoming = movement.quantity > 0 && INCOMING_TYPES.has(movement.type)

  return (
    <tr>
      <td className="py-2 pr-3 whitespace-nowrap text-slate-500">{formatDateTime(movement.createdAt)}</td>
      <td className="py-2 pr-3">
        <Badge tone={style.tone}>{style.label}</Badge>
        {movement.note && <span className="ml-2 text-slate-600">{movement.note}</span>}
      </td>
      <td className={cn('tabular py-2 text-right font-medium', incoming ? 'text-emerald-700' : 'text-slate-700')}>
        {movement.quantity > 0 ? `+${formatNumber(movement.quantity)}` : formatNumber(movement.quantity)}
      </td>
      <td className="tabular py-2 text-right text-slate-700">{formatNumber(movement.onHandAfter)}</td>
      <td className="tabular py-2 text-right text-slate-700">{formatNumber(movement.reservedAfter)}</td>
    </tr>
  )
}
