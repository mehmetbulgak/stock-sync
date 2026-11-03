import { cva, type VariantProps } from 'class-variance-authority'
import type { ComponentProps } from 'react'
import { cn } from '@/lib/cn'

const badgeVariants = cva('inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium whitespace-nowrap', {
  variants: {
    tone: {
      neutral: 'bg-slate-100 text-slate-700',
      success: 'bg-emerald-50 text-emerald-700 ring-1 ring-emerald-600/15',
      warning: 'bg-amber-50 text-amber-800 ring-1 ring-amber-600/20',
      danger: 'bg-rose-50 text-rose-700 ring-1 ring-rose-600/15',
      info: 'bg-sky-50 text-sky-700 ring-1 ring-sky-600/15',
    },
  },
  defaultVariants: { tone: 'neutral' },
})

export type BadgeTone = NonNullable<VariantProps<typeof badgeVariants>['tone']>

export function Badge({ className, tone, ...props }: ComponentProps<'span'> & VariantProps<typeof badgeVariants>) {
  return <span className={cn(badgeVariants({ tone }), className)} {...props} />
}
