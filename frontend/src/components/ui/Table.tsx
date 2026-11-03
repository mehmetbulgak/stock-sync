import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'

export function Table({ className, ...props }: ComponentProps<'table'>) {
  return (
    <div className="overflow-x-auto">
      <table className={cn('w-full text-left text-sm', className)} {...props} />
    </div>
  )
}

export function Th({ className, ...props }: ComponentProps<'th'>) {
  return (
    <th
      className={cn(
        'border-b border-slate-200 bg-slate-50/80 px-4 py-2.5 text-xs font-medium tracking-wide text-slate-500 uppercase',
        className,
      )}
      {...props}
    />
  )
}

export function Td({ className, ...props }: ComponentProps<'td'>) {
  return <td className={cn('border-b border-slate-100 px-4 py-3 align-middle', className)} {...props} />
}
