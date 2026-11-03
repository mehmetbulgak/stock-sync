import { Search } from 'lucide-react'
import type { ComponentProps } from 'react'
import { Input } from '@/components/ui/FormControls'
import { cn } from '@/lib/cn'

type SearchInputProps = Omit<ComponentProps<'input'>, 'type'> & { label: string }

export function SearchInput({ label, className, ...props }: SearchInputProps) {
  return (
    <div className={cn('relative', className)}>
      <Search className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-slate-400" aria-hidden />
      <Input type="search" aria-label={label} className="pl-9" {...props} />
    </div>
  )
}
