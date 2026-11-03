import { LoaderCircle } from 'lucide-react'
import { cn } from '@/lib/cn'

export function Spinner({ fullPage = false }: { fullPage?: boolean }) {
  return (
    <div className={cn('flex items-center justify-center text-slate-400', fullPage ? 'min-h-dvh' : 'h-64')}>
      <LoaderCircle className="size-6 animate-spin" aria-label="Loading" />
    </div>
  )
}
