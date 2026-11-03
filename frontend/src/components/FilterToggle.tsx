type FilterToggleProps = { label: string; checked: boolean; onChange: (checked: boolean) => void }

export function FilterToggle({ label, checked, onChange }: FilterToggleProps) {
  return (
    <label className="inline-flex cursor-pointer items-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm text-slate-700 select-none has-checked:border-brand-200 has-checked:bg-brand-50 has-checked:text-brand-800">
      <input type="checkbox" className="size-4 accent-brand-700" checked={checked} onChange={(e) => onChange(e.target.checked)} />
      {label}
    </label>
  )
}
