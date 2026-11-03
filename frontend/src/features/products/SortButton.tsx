import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react'
import { cn } from '@/lib/cn'

type SortButtonProps = { label: string; active: boolean; descending: boolean; onClick: () => void }

export function SortButton({ label, active, descending, onClick }: SortButtonProps) {
  const Icon = !active ? ArrowUpDown : descending ? ArrowDown : ArrowUp

  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={`Sort by ${label.toLowerCase()}`}
      className={cn('inline-flex cursor-pointer items-center gap-1 uppercase', active && 'text-slate-900')}
    >
      {label} <Icon className="size-3" aria-hidden />
    </button>
  )
}
