import { AlertTriangle, PackagePlus, Plus, SlidersHorizontal } from 'lucide-react'
import { useState } from 'react'
import { FilterToggle } from '@/components/FilterToggle'
import { SearchInput } from '@/components/SearchInput'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { PageHeader } from '@/components/ui/PageHeader'
import { Pagination } from '@/components/ui/Pagination'
import { SkeletonRows } from '@/components/ui/Skeleton'
import type { Product } from '@/lib/api/types'
import { cn } from '@/lib/cn'
import { useDebouncedValue } from '@/lib/hooks/useDebouncedValue'
import { useUrlFilters } from '@/lib/hooks/useUrlFilters'
import { type ProductQuery, type ProductSort, useProducts } from '@/lib/query/products'
import { AdjustStockDialog } from './AdjustStockDialog'
import { ProductFormDialog } from './ProductFormDialog'
import { type ProductAction, ProductsTable } from './ProductsTable'
import { StockHistoryDialog } from './StockHistoryDialog'

const PAGE_SIZE = 15
const SORTABLE: readonly ProductSort[] = ['Name', 'Sku', 'Available', 'UpdatedAt']
const DEFAULT_SORT: ProductSort = 'Name'

type DialogState = { kind: 'create' } | { kind: ProductAction; product: Product } | null

const parseSort = (value: string | null): ProductSort => SORTABLE.find((s) => s === value) ?? DEFAULT_SORT

export default function ProductsPage() {
  const { params, update, page } = useUrlFilters()
  const [search, setSearch] = useState(params.get('search') ?? '')
  const debouncedSearch = useDebouncedValue(search.trim())
  const [dialog, setDialog] = useState<DialogState>(null)

  const query: ProductQuery = {
    search: debouncedSearch || undefined,
    lowStockOnly: params.get('lowStockOnly') === 'true' || undefined,
    includeInactive: params.get('includeInactive') === 'true' || undefined,
    sort: parseSort(params.get('sort')),
    descending: params.get('desc') === 'true' || undefined,
    page,
    pageSize: PAGE_SIZE,
  }
  const { data, isPending, isError, isFetching } = useProducts(query)

  const sort = query.sort ?? DEFAULT_SORT
  const descending = Boolean(query.descending)
  const filtered = Boolean(query.search || query.lowStockOnly)
  const openCreate = () => setDialog({ kind: 'create' })
  const closeDialog = () => setDialog(null)
  const toggleSort = (key: ProductSort) => update({ sort: key, desc: sort === key && !descending ? 'true' : null })
  const selected = (kind: ProductAction) => (dialog && dialog.kind === kind ? dialog.product : null)

  return (
    <>
      <PageHeader
        title="Products"
        description="One stock count shared by every sales channel."
        actions={
          <Button onClick={openCreate}>
            <Plus /> New product
          </Button>
        }
      />

      <Card>
        <div className="flex flex-wrap items-center gap-3 border-b border-slate-100 p-4">
          <SearchInput
            label="Search products"
            placeholder="Search by name or SKU"
            className="min-w-60 flex-1"
            maxLength={100}
            value={search}
            onChange={(e) => {
              setSearch(e.target.value)
              update({ search: e.target.value || null })
            }}
          />
          <FilterToggle
            label="Low stock only"
            checked={Boolean(query.lowStockOnly)}
            onChange={(checked) => update({ lowStockOnly: checked ? 'true' : null })}
          />
          <FilterToggle
            label="Show inactive"
            checked={Boolean(query.includeInactive)}
            onChange={(checked) => update({ includeInactive: checked ? 'true' : null })}
          />
        </div>

        {isError ? (
          <EmptyState icon={<AlertTriangle />} title="Couldn't load products" description="Check your connection and try again." />
        ) : isPending ? (
          <SkeletonRows />
        ) : data.items.length === 0 ? (
          filtered ? (
            <EmptyState icon={<SlidersHorizontal />} title="No products match these filters" />
          ) : (
            <EmptyState
              icon={<PackagePlus />}
              title="Add your first product"
              description="Products and their stock are shared across Trendyol, Hepsiburada, your web store and manual orders."
              action={
                <Button onClick={openCreate}>
                  <Plus /> New product
                </Button>
              }
            />
          )
        ) : (
          <div className={cn('transition-opacity', isFetching && 'opacity-60')}>
            <ProductsTable
              products={data.items}
              sort={sort}
              descending={descending}
              onSort={toggleSort}
              onAction={(kind, product) => setDialog({ kind, product })}
            />
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              onPageChange={(next) => update({ page: String(next) })}
            />
          </div>
        )}
      </Card>

      <ProductFormDialog open={dialog?.kind === 'create' || dialog?.kind === 'edit'} product={selected('edit')} onClose={closeDialog} />
      <AdjustStockDialog product={selected('adjust')} onClose={closeDialog} />
      <StockHistoryDialog product={selected('history')} onClose={closeDialog} />
    </>
  )
}
