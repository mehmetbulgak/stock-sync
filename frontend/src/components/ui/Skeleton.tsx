import { cn } from '@/lib/cn'

export function Skeleton({ className }: { className?: string }) {
  return <div className={cn('animate-pulse rounded-md bg-slate-200/70', className)} aria-hidden />
}

export function SkeletonRows({ count = 6 }: { count?: number }) {
  return (
    <div className="grid gap-3 p-4">
      {Array.from({ length: count }, (_, i) => (
        <Skeleton key={i} className="h-12" />
      ))}
    </div>
  )
}
