import { History, Pencil } from 'lucide-react'
import { StockLevel } from '@/components/StockLevel'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Table, Td, Th } from '@/components/ui/Table'
import type { Product } from '@/lib/api/types'
import type { ProductSort } from '@/lib/query/products'
import { formatCurrency, formatNumber } from '@/lib/format'
import { SortButton } from './SortButton'

export type ProductAction = 'edit' | 'adjust' | 'history'

type ProductsTableProps = {
  products: Product[]
  sort: ProductSort
  descending: boolean
  onSort: (sort: ProductSort) => void
  onAction: (action: ProductAction, product: Product) => void
}

export function ProductsTable({ products, sort, descending, onSort, onAction }: ProductsTableProps) {
  const sortProps = (key: ProductSort) => ({ active: sort === key, descending, onClick: () => onSort(key) })

  return (
    <Table>
      <thead>
        <tr>
          <Th aria-sort={ariaSort(sort === 'Name', descending)}>
            <SortButton label="Product" {...sortProps('Name')} />
          </Th>
          <Th className="text-right">Price</Th>
          <Th className="text-right">On hand</Th>
          <Th aria-sort={ariaSort(sort === 'Available', descending)}>
            <SortButton label="Stock" {...sortProps('Available')} />
          </Th>
          <Th className="text-right">
            <span className="sr-only">Actions</span>
          </Th>
        </tr>
      </thead>
      <tbody>
        {products.map((product) => (
          <tr key={product.id} className="hover:bg-slate-50/60">
            <Td>
              <div className="flex items-center gap-2">
                <p className="font-medium text-slate-900">{product.name}</p>
                {!product.isActive && <Badge>Inactive</Badge>}
              </div>
              <p className="font-mono text-xs text-slate-500">{product.sku}</p>
            </Td>
            <Td className="tabular text-right text-slate-700">{formatCurrency(product.price)}</Td>
            <Td className="tabular text-right text-slate-700">{formatNumber(product.onHand)}</Td>
            <Td className="w-64">
              <StockLevel product={product} />
            </Td>
            <Td>
              <div className="flex justify-end gap-1">
                <Button variant="secondary" size="sm" onClick={() => onAction('adjust', product)}>
                  Adjust stock
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label={`Stock history for ${product.name}`}
                  onClick={() => onAction('history', product)}
                >
                  <History />
                </Button>
                <Button variant="ghost" size="icon" aria-label={`Edit ${product.name}`} onClick={() => onAction('edit', product)}>
                  <Pencil />
                </Button>
              </div>
            </Td>
          </tr>
        ))}
      </tbody>
    </Table>
  )
}

const ariaSort = (active: boolean, descending: boolean) => (active ? (descending ? 'descending' : 'ascending') : undefined)
