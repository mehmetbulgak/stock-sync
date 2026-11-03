import type { ChannelType } from '@/lib/api/types'
import { channelStyles } from '@/lib/channels'
import { cn } from '@/lib/cn'

export function ChannelBadge({ type, name }: { type: ChannelType; name?: string }) {
  const style = channelStyles[type]

  return (
    <span className="inline-flex items-center gap-1.5 text-sm text-slate-700">
      <span className={cn('size-2 rounded-full', style.dot)} aria-hidden />
      {name ?? style.label}
    </span>
  )
}
