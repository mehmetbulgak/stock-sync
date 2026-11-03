import { Boxes } from 'lucide-react'
import { cn } from '@/lib/cn'

export function Logo({ inverted = false }: { inverted?: boolean }) {
  return (
    <div className="flex items-center gap-2.5">
      <div className={cn('flex size-8 items-center justify-center rounded-lg', inverted ? 'bg-white/15' : 'bg-brand-700')}>
        <Boxes className="size-4.5 text-white" aria-hidden />
      </div>
      <span className={cn('text-[15px] font-semibold tracking-tight', inverted ? 'text-white' : 'text-slate-900')}>StockSync</span>
    </div>
  )
}
