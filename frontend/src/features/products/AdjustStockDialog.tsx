import { zodResolver } from '@hookform/resolvers/zod'
import { useId, useState } from 'react'
import { useForm, useWatch } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '@/components/ui/Button'
import { Dialog, DialogBody, DialogFooter } from '@/components/ui/Dialog'
import { Field } from '@/components/ui/Field'
import { Input } from '@/components/ui/FormControls'
import { FormError } from '@/components/ui/FormError'
import { describeError } from '@/lib/api/errors'
import type { Product } from '@/lib/api/types'
import { cn } from '@/lib/cn'
import { formatNumber } from '@/lib/format'
import { useAdjustStock } from './api'
import { ADJUST_DIRECTIONS, type AdjustStockInput, type AdjustStockValues, adjustStockSchema } from './schemas'

const SUGGESTED_REASONS = ['Supplier delivery', 'Stock count correction', 'Damaged', 'Customer return']

const directionLabels: Record<(typeof ADJUST_DIRECTIONS)[number], string> = {
  add: 'Add units',
  remove: 'Remove units',
}

type AdjustStockDialogProps = { product: Product | null; onClose: () => void }

export function AdjustStockDialog({ product, onClose }: AdjustStockDialogProps) {
  return (
    <Dialog
      open={product !== null}
      onOpenChange={(next) => !next && onClose()}
      title="Adjust stock"
      description={product ? `${product.name} · ${product.sku}` : undefined}
    >
      {product && <AdjustStockForm product={product} onDone={onClose} />}
    </Dialog>
  )
}

function AdjustStockForm({ product, onDone }: { product: Product; onDone: () => void }) {
  const adjust = useAdjustStock(product.id)
  const reasonsId = useId()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<AdjustStockInput, unknown, AdjustStockValues>({
    resolver: zodResolver(adjustStockSchema),
    defaultValues: { direction: 'add', quantity: '', reason: '' },
  })
  const { errors, isSubmitting } = form.formState
  const [direction, quantity] = useWatch({ control: form.control, name: ['direction', 'quantity'] })

  const delta = (direction === 'add' ? 1 : -1) * (Number(quantity) || 0)
  const nextOnHand = product.onHand + delta
  const belowReserved = nextOnHand < product.reserved

  const submit = async (values: AdjustStockValues) => {
    setError(null)
    try {
      await adjust.mutateAsync({
        quantity: values.direction === 'add' ? values.quantity : -values.quantity,
        reason: values.reason,
      })
      toast.success(`Stock for ${product.sku} updated`)
      onDone()
    } catch (e) {
      setError(describeError(e))
    }
  }

  return (
    <form className="contents" onSubmit={form.handleSubmit(submit)} noValidate>
      <DialogBody>
        <div className="grid gap-4">
          <FormError message={error} />
          <div className="grid grid-cols-2 gap-2" role="radiogroup" aria-label="Direction">
            {ADJUST_DIRECTIONS.map((value) => (
              <label
                key={value}
                className="flex cursor-pointer items-center justify-center gap-2 rounded-lg border border-slate-200 px-3 py-2 text-sm font-medium text-slate-700 has-checked:border-brand-600 has-checked:bg-brand-50 has-checked:text-brand-800 has-focus-visible:outline-2 has-focus-visible:outline-brand-600"
              >
                <input type="radio" value={value} className="sr-only" {...form.register('direction')} />
                {directionLabels[value]}
              </label>
            ))}
          </div>
          <Field label="Quantity" error={errors.quantity?.message}>
            {(props) => <Input type="number" inputMode="numeric" min="1" {...props} {...form.register('quantity')} />}
          </Field>
          <Field label="Reason" error={errors.reason?.message}>
            {(props) => <Input list={reasonsId} placeholder="e.g. Supplier delivery" {...props} {...form.register('reason')} />}
          </Field>
          <datalist id={reasonsId}>
            {SUGGESTED_REASONS.map((reason) => (
              <option key={reason} value={reason} />
            ))}
          </datalist>

          <dl className="grid grid-cols-3 gap-3 rounded-xl bg-slate-50 p-4 text-center">
            <Stat label="On hand now" value={formatNumber(product.onHand)} />
            <Stat label="Reserved" value={formatNumber(product.reserved)} />
            <Stat label="On hand after" value={formatNumber(nextOnHand)} className={cn(belowReserved && 'text-rose-600')} />
          </dl>
          {belowReserved && (
            <p className="text-xs text-rose-600">
              {formatNumber(product.reserved)} units are reserved for open orders, so on-hand stock can't go below that.
            </p>
          )}
        </div>
      </DialogBody>
      <DialogFooter>
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" loading={isSubmitting}>
          Save adjustment
        </Button>
      </DialogFooter>
    </form>
  )
}

function Stat({ label, value, className }: { label: string; value: string; className?: string }) {
  return (
    <div>
      <dt className="text-xs text-slate-500">{label}</dt>
      <dd className={cn('tabular mt-0.5 text-lg font-semibold text-slate-900', className)}>{value}</dd>
    </div>
  )
}
