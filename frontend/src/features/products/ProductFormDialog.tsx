import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { Button } from '@/components/ui/Button'
import { Dialog, DialogBody, DialogFooter } from '@/components/ui/Dialog'
import { Field } from '@/components/ui/Field'
import { Input, Textarea } from '@/components/ui/FormControls'
import { FormError } from '@/components/ui/FormError'
import { describeError } from '@/lib/api/errors'
import type { Product } from '@/lib/api/types'
import { useCreateProduct, useUpdateProduct } from './api'
import { type ProductFormInput, type ProductFormValues, productSchema } from './schemas'

type ProductFormDialogProps = { open: boolean; product: Product | null; onClose: () => void }

export function ProductFormDialog({ open, product, onClose }: ProductFormDialogProps) {
  return (
    <Dialog
      open={open}
      onOpenChange={(next) => !next && onClose()}
      title={product ? 'Edit product' : 'New product'}
      description={product ? 'Stock is changed with "Adjust stock", so every change is recorded.' : undefined}
    >
      {open && (product ? <EditProductForm product={product} onDone={onClose} /> : <CreateProductForm onDone={onClose} />)}
    </Dialog>
  )
}

const emptyProduct: ProductFormInput = {
  sku: '',
  name: '',
  description: '',
  price: '',
  initialStock: '0',
  lowStockThreshold: '5',
  isActive: true,
}

const toFormInput = (product: Product): ProductFormInput => ({
  sku: product.sku,
  name: product.name,
  description: product.description ?? '',
  price: String(product.price),
  initialStock: String(product.onHand),
  lowStockThreshold: String(product.lowStockThreshold),
  isActive: product.isActive,
})

const optionalText = (value: string) => value.trim() || null

function CreateProductForm({ onDone }: { onDone: () => void }) {
  const create = useCreateProduct()

  const save = async (values: ProductFormValues) => {
    await create.mutateAsync({
      sku: values.sku,
      name: values.name,
      description: optionalText(values.description),
      price: values.price,
      initialStock: values.initialStock,
      lowStockThreshold: values.lowStockThreshold,
    })
    toast.success(`${values.name} added`)
  }

  return <ProductForm defaultValues={emptyProduct} editing={false} onSave={save} onDone={onDone} />
}

function EditProductForm({ product, onDone }: { product: Product; onDone: () => void }) {
  const update = useUpdateProduct(product.id)

  const save = async (values: ProductFormValues) => {
    await update.mutateAsync({
      name: values.name,
      description: optionalText(values.description),
      price: values.price,
      lowStockThreshold: values.lowStockThreshold,
      isActive: values.isActive,
    })
    toast.success(`${values.name} updated`)
  }

  return <ProductForm defaultValues={toFormInput(product)} editing onSave={save} onDone={onDone} />
}

type ProductFormProps = {
  defaultValues: ProductFormInput
  editing: boolean
  onSave: (values: ProductFormValues) => Promise<void>
  onDone: () => void
}

function ProductForm({ defaultValues, editing, onSave, onDone }: ProductFormProps) {
  const [error, setError] = useState<string | null>(null)
  const form = useForm<ProductFormInput, unknown, ProductFormValues>({
    resolver: zodResolver(productSchema),
    defaultValues,
  })
  const { errors, isSubmitting } = form.formState

  const submit = async (values: ProductFormValues) => {
    setError(null)
    try {
      await onSave(values)
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
          <div className="grid gap-4 sm:grid-cols-[1fr_2fr]">
            <Field label="SKU" error={errors.sku?.message} hint={editing ? 'SKUs cannot change.' : undefined}>
              {(props) => <Input className="font-mono uppercase" disabled={editing} {...props} {...form.register('sku')} />}
            </Field>
            <Field label="Name" error={errors.name?.message}>
              {(props) => <Input {...props} {...form.register('name')} />}
            </Field>
          </div>
          <Field label="Description (optional)" error={errors.description?.message}>
            {(props) => <Textarea rows={2} {...props} {...form.register('description')} />}
          </Field>
          <div className="grid gap-4 sm:grid-cols-3">
            <Field label="Price (₺)" error={errors.price?.message}>
              {(props) => <Input type="number" inputMode="decimal" step="0.01" min="0" {...props} {...form.register('price')} />}
            </Field>
            {!editing && (
              <Field label="Opening stock" error={errors.initialStock?.message}>
                {(props) => <Input type="number" inputMode="numeric" min="0" {...props} {...form.register('initialStock')} />}
              </Field>
            )}
            <Field label="Low-stock alert at" error={errors.lowStockThreshold?.message} hint="Units available">
              {(props) => <Input type="number" inputMode="numeric" min="0" {...props} {...form.register('lowStockThreshold')} />}
            </Field>
          </div>
          {editing && (
            <label className="flex items-center gap-2 text-sm text-slate-700">
              <input type="checkbox" className="size-4 accent-brand-700" {...form.register('isActive')} />
              Active (inactive products cannot be ordered)
            </label>
          )}
        </div>
      </DialogBody>
      <DialogFooter>
        <Button type="button" variant="secondary" onClick={onDone}>
          Cancel
        </Button>
        <Button type="submit" loading={isSubmitting}>
          {editing ? 'Save changes' : 'Add product'}
        </Button>
      </DialogFooter>
    </form>
  )
}
