import { zodResolver } from '@hookform/resolvers/zod'
import { AlertTriangle, Plus, Trash2 } from 'lucide-react'
import { useState } from 'react'
import { type UseFormReturn, useFieldArray, useForm, useWatch } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '@/components/ui/Button'
import { Dialog, DialogBody, DialogFooter } from '@/components/ui/Dialog'
import { EmptyState } from '@/components/ui/EmptyState'
import { Field } from '@/components/ui/Field'
import { Input, Select } from '@/components/ui/FormControls'
import { FormError } from '@/components/ui/FormError'
import { Skeleton } from '@/components/ui/Skeleton'
import { describeError } from '@/lib/api/errors'
import type { Channel, Product } from '@/lib/api/types'
import { formatCurrency, formatNumber } from '@/lib/format'
import { useChannels } from '@/lib/query/channels'
import { useProducts } from '@/lib/query/products'
import { useCreateOrder } from './api'
import { ORDER_LIMITS, type OrderFormInput, type OrderFormValues, emptyOrderLine, orderSchema } from './schemas'

const PRODUCT_PICKER_SIZE = 100

type CreateOrderDialogProps = { open: boolean; onClose: () => void; onCreated: (orderId: string) => void }

export function CreateOrderDialog({ open, onClose, onCreated }: CreateOrderDialogProps) {
  return (
    <Dialog
      open={open}
      onOpenChange={(next) => !next && onClose()}
      title="New order"
      description="For phone, in-store or marketplace orders you enter by hand. Stock is reserved immediately."
      size="lg"
    >
      {open && <CreateOrderLoader onClose={onClose} onCreated={onCreated} />}
    </Dialog>
  )
}

function CreateOrderLoader({ onClose, onCreated }: Omit<CreateOrderDialogProps, 'open'>) {
  const channels = useChannels()
  const products = useProducts({ pageSize: PRODUCT_PICKER_SIZE, sort: 'Name' })

  if (channels.isError || products.isError) {
    return (
      <DialogBody>
        <EmptyState icon={<AlertTriangle />} title="Couldn't load channels and products" />
      </DialogBody>
    )
  }

  if (channels.isPending || products.isPending) {
    return (
      <DialogBody>
        <Skeleton className="h-40" />
      </DialogBody>
    )
  }

  return <CreateOrderForm channels={channels.data} products={products.data.items} onClose={onClose} onCreated={onCreated} />
}

type CreateOrderFormProps = Omit<CreateOrderDialogProps, 'open'> & { channels: Channel[]; products: Product[] }

function CreateOrderForm({ channels, products, onClose, onCreated }: CreateOrderFormProps) {
  const create = useCreateOrder()
  const [error, setError] = useState<string | null>(null)
  const activeChannels = channels.filter((c) => c.isActive)

  const form = useForm<OrderFormInput, unknown, OrderFormValues>({
    resolver: zodResolver(orderSchema),
    defaultValues: {
      channelId: activeChannels.find((c) => c.type === 'Manual')?.id ?? '',
      number: '',
      customerName: '',
      items: [emptyOrderLine],
    },
  })
  const lines = useFieldArray({ control: form.control, name: 'items' })
  const items = useWatch({ control: form.control, name: 'items' })
  const { errors, isSubmitting } = form.formState

  const productById = new Map(products.map((p) => [p.id, p]))
  const requestedByProduct = new Map<string, number>()
  for (const item of items) {
    requestedByProduct.set(item.productId, (requestedByProduct.get(item.productId) ?? 0) + (Number(item.quantity) || 0))
  }
  const total = items.reduce((sum, item) => sum + (productById.get(item.productId)?.price ?? 0) * (Number(item.quantity) || 0), 0)

  const submit = async (values: OrderFormValues) => {
    setError(null)
    try {
      const order = await create.mutateAsync({ ...values, number: values.number || null })
      toast.success(`Order ${order.number} created and stock reserved`)
      onClose()
      onCreated(order.id)
    } catch (e) {
      setError(describeError(e))
    }
  }

  return (
    <form className="contents" onSubmit={form.handleSubmit(submit)} noValidate>
      <DialogBody>
        <div className="grid gap-4">
          <FormError message={error} />
          <div className="grid gap-4 sm:grid-cols-3">
            <Field label="Channel" error={errors.channelId?.message}>
              {(props) => (
                <Select {...props} {...form.register('channelId')}>
                  <option value="">Choose…</option>
                  {activeChannels.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </Select>
              )}
            </Field>
            <Field label="Order number" error={errors.number?.message} hint="Leave empty to generate one">
              {(props) => <Input className="font-mono" placeholder="e.g. TY-10234" {...props} {...form.register('number')} />}
            </Field>
            <Field label="Customer" error={errors.customerName?.message}>
              {(props) => <Input autoComplete="off" {...props} {...form.register('customerName')} />}
            </Field>
          </div>

          <fieldset className="grid gap-2">
            <legend className="mb-1 text-sm font-medium text-slate-700">Products</legend>
            {lines.fields.map((field, index) => {
              const productId = items[index]?.productId ?? ''
              return (
                <OrderLineRow
                  key={field.id}
                  form={form}
                  index={index}
                  products={products}
                  selected={productById.get(productId)}
                  requested={requestedByProduct.get(productId) ?? 0}
                  canRemove={lines.fields.length > 1}
                  onRemove={() => lines.remove(index)}
                />
              )
            })}
            {errors.items?.root?.message && <p className="text-xs text-rose-600">{errors.items.root.message}</p>}
            {errors.items?.message && <p className="text-xs text-rose-600">{errors.items.message}</p>}
            <div>
              <Button
                type="button"
                variant="secondary"
                size="sm"
                disabled={lines.fields.length >= ORDER_LIMITS.maxLines}
                onClick={() => lines.append(emptyOrderLine)}
              >
                <Plus /> Add another product
              </Button>
            </div>
          </fieldset>
        </div>
      </DialogBody>
      <DialogFooter className="justify-between">
        <p className="self-center text-sm text-slate-600">
          Total <span className="tabular ml-1 text-base font-semibold text-slate-900">{formatCurrency(total)}</span>
        </p>
        <div className="flex gap-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" loading={isSubmitting}>
            Create order
          </Button>
        </div>
      </DialogFooter>
    </form>
  )
}

type OrderLineRowProps = {
  form: UseFormReturn<OrderFormInput, unknown, OrderFormValues>
  index: number
  products: Product[]
  selected: Product | undefined
  requested: number
  canRemove: boolean
  onRemove: () => void
}

function OrderLineRow({ form, index, products, selected, requested, canRemove, onRemove }: OrderLineRowProps) {
  const lineError = form.formState.errors.items?.[index]
  const short = selected !== undefined && requested > selected.available
  const position = index + 1

  return (
    <div className="grid grid-cols-[1fr_6rem_auto] items-start gap-2">
      <div>
        <Select
          aria-label={`Product ${position}`}
          aria-invalid={lineError?.productId ? true : undefined}
          {...form.register(`items.${index}.productId`)}
        >
          <option value="">Choose a product…</option>
          {products.map((p) => (
            <option key={p.id} value={p.id} disabled={p.available <= 0}>
              {p.name} ({p.sku}) · {formatNumber(p.available)} available
            </option>
          ))}
        </Select>
        {lineError?.productId && <p className="mt-1 text-xs text-rose-600">{lineError.productId.message}</p>}
        {short && (
          <p className="mt-1 text-xs text-rose-600">Only {formatNumber(selected.available)} available. The order would be rejected.</p>
        )}
      </div>
      <div>
        <Input
          type="number"
          min="1"
          inputMode="numeric"
          aria-label={`Quantity ${position}`}
          aria-invalid={short || lineError?.quantity ? true : undefined}
          {...form.register(`items.${index}.quantity`)}
        />
        {lineError?.quantity && <p className="mt-1 text-xs text-rose-600">{lineError.quantity.message}</p>}
      </div>
      <Button type="button" variant="ghost" size="icon" aria-label={`Remove product ${position}`} disabled={!canRemove} onClick={onRemove}>
        <Trash2 />
      </Button>
    </div>
  )
}
