import type { Product } from '@/lib/api/types'
import { cn } from '@/lib/cn'
import { formatNumber } from '@/lib/format'
import { Badge } from './ui/Badge'

type StockLevelProps = { product: Pick<Product, 'onHand' | 'reserved' | 'available' | 'isLowStock'> }

export function StockLevel({ product }: StockLevelProps) {
  const out = product.available <= 0
  const availableShare = product.onHand === 0 ? 0 : Math.round((product.available / product.onHand) * 100)
  const barColor = out ? 'bg-rose-500' : product.isLowStock ? 'bg-amber-500' : 'bg-brand-600'

  return (
    <div className="flex min-w-40 items-center gap-3">
      <div className="w-full">
        <div className="flex items-baseline justify-between gap-2 text-xs">
          <span className="tabular font-semibold text-slate-900">{formatNumber(product.available)} available</span>
          <span className="tabular text-slate-500">{formatNumber(product.reserved)} reserved</span>
        </div>
        <div className="mt-1.5 h-1.5 overflow-hidden rounded-full bg-slate-100">
          <div className={cn('h-full rounded-full', barColor)} style={{ width: `${availableShare}%` }} />
        </div>
      </div>
      <div className="w-10 shrink-0">
        {out ? <Badge tone="danger">Out</Badge> : product.isLowStock ? <Badge tone="warning">Low</Badge> : null}
      </div>
    </div>
  )
}
