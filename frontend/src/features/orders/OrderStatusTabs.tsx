import type { OrderStatus } from '@/lib/api/types'
import { cn } from '@/lib/cn'

const tabs: { label: string; value: OrderStatus | null }[] = [
  { label: 'All', value: null },
  { label: 'Waiting to ship', value: 'Pending' },
  { label: 'Shipped', value: 'Shipped' },
  { label: 'Cancelled', value: 'Cancelled' },
]

type OrderStatusTabsProps = { value: OrderStatus | null; onChange: (status: OrderStatus | null) => void }

export function OrderStatusTabs({ value, onChange }: OrderStatusTabsProps) {
  return (
    <div className="flex flex-wrap rounded-lg bg-slate-100 p-1" role="group" aria-label="Order status">
      {tabs.map((tab) => {
        const selected = value === tab.value
        return (
          <button
            key={tab.label}
            type="button"
            aria-pressed={selected}
            onClick={() => onChange(tab.value)}
            className={cn(
              'cursor-pointer rounded-md px-3 py-1.5 text-sm font-medium transition-colors',
              selected ? 'bg-white text-slate-900 shadow-xs' : 'text-slate-600 hover:text-slate-900',
            )}
          >
            {tab.label}
          </button>
        )
      })}
    </div>
  )
}
