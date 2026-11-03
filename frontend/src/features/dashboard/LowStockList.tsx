import { PackageCheck } from 'lucide-react'
import { StockLevel } from '@/components/StockLevel'
import { EmptyState } from '@/components/ui/EmptyState'
import type { Product } from '@/lib/api/types'

export function LowStockList({ products }: { products: Product[] }) {
  if (products.length === 0) {
    return <EmptyState icon={<PackageCheck />} title="Everything is well stocked" />
  }

  return (
    <ul className="divide-y divide-slate-100">
      {products.map((product) => (
        <li key={product.id} className="flex flex-wrap items-center justify-between gap-x-6 gap-y-2 px-5 py-3">
          <div className="min-w-0">
            <p className="truncate text-sm font-medium text-slate-900">{product.name}</p>
            <p className="font-mono text-xs text-slate-500">{product.sku}</p>
          </div>
          <div className="w-52">
            <StockLevel product={product} />
          </div>
        </li>
      ))}
    </ul>
  )
}
