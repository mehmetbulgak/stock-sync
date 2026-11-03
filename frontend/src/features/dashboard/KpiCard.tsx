import { ArrowRight } from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { Card } from '@/components/ui/Card'
import { cn } from '@/lib/cn'

type KpiCardProps = {
  icon: ReactNode
  label: string
  value: string
  sub?: string
  tone?: 'default' | 'warning'
  href?: string
}

export function KpiCard({ icon, label, value, sub, tone = 'default', href }: KpiCardProps) {
  const body = (
    <Card className={cn('h-full p-4 transition-colors sm:p-5', href && 'hover:border-brand-200')}>
      <div className="flex items-center justify-between">
        <p className="text-sm leading-snug text-slate-500">{label}</p>
        <span
          className={cn(
            'shrink-0 rounded-lg p-1.5 [&_svg]:size-4',
            tone === 'warning' ? 'bg-amber-50 text-amber-700' : 'bg-brand-50 text-brand-700',
          )}
        >
          {icon}
        </span>
      </div>
      <p className="tabular mt-3 text-xl font-semibold tracking-tight text-slate-900 sm:text-2xl">{value}</p>
      {sub && <p className="mt-0.5 text-xs text-slate-500">{sub}</p>}
      {href && (
        <p className="mt-2 inline-flex items-center gap-1 text-xs font-medium text-brand-700">
          Review <ArrowRight className="size-3" aria-hidden />
        </p>
      )}
    </Card>
  )

  return href ? (
    <Link to={href} className="block rounded-xl">
      {body}
    </Link>
  ) : (
    body
  )
}
