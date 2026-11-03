import { ShoppingCart } from 'lucide-react'
import { EmptyState } from '@/components/ui/EmptyState'
import type { ChannelSales } from '@/lib/api/types'
import { channelStyles } from '@/lib/channels'
import { cn } from '@/lib/cn'
import { formatCurrency, formatShare, pluralize } from '@/lib/format'

const MIN_BAR_PERCENT = 2

export function ChannelBreakdown({ channels }: { channels: ChannelSales[] }) {
  if (channels.length === 0) {
    return <EmptyState icon={<ShoppingCart />} title="No sales in the last 7 days" />
  }

  const max = Math.max(...channels.map((c) => c.revenue))
  const total = channels.reduce((sum, c) => sum + c.revenue, 0)

  return (
    <ul className="grid gap-4 px-5 py-5">
      {channels.map((channel) => (
        <li key={channel.channelName}>
          <div className="flex items-baseline justify-between gap-3 text-sm">
            <span className="font-medium text-slate-800">{channel.channelName}</span>
            <span className="tabular text-slate-900">{formatCurrency(channel.revenue)}</span>
          </div>
          <div className="mt-1.5 h-2 rounded-full bg-slate-100">
            <div
              className={cn('h-full rounded-full', channelStyles[channel.channelType].dot)}
              style={{ width: `${max === 0 ? 0 : Math.max((channel.revenue / max) * 100, MIN_BAR_PERCENT)}%` }}
            />
          </div>
          <p className="tabular mt-1 text-xs text-slate-500">
            {pluralize(channel.orders, 'order')} · {formatShare(channel.revenue, total)} of revenue
          </p>
        </li>
      ))}
    </ul>
  )
}
