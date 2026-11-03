import { ChevronLeft, ChevronRight } from 'lucide-react'
import { formatNumber } from '@/lib/format'
import { Button } from './Button'

type PaginationProps = { page: number; totalPages: number; totalCount: number; onPageChange: (page: number) => void }

export function Pagination({ page, totalPages, totalCount, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  return (
    <div className="flex items-center justify-between gap-4 px-4 py-3 text-sm text-slate-500">
      <span>
        Page {page} of {totalPages} · {formatNumber(totalCount)} total
      </span>
      <div className="flex gap-2">
        <Button variant="secondary" size="sm" onClick={() => onPageChange(page - 1)} disabled={page <= 1}>
          <ChevronLeft /> Previous
        </Button>
        <Button variant="secondary" size="sm" onClick={() => onPageChange(page + 1)} disabled={page >= totalPages}>
          Next <ChevronRight />
        </Button>
      </div>
    </div>
  )
}
