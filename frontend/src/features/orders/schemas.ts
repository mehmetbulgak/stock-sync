import { z } from 'zod'
import type { OrderStatus } from '@/lib/api/types'

export const ORDER_STATUSES: readonly OrderStatus[] = ['Pending', 'Shipped', 'Cancelled']

export const ORDER_LIMITS = {
  numberMaxLength: 64,
  customerNameMaxLength: 150,
  maxLines: 50,
  maxQuantityPerLine: 10_000,
} as const

const ORDER_NUMBER_PATTERN = /^[A-Za-z0-9._-]*$/

export const orderItemSchema = z.object({
  productId: z.string().min(1, 'Choose a product'),
  quantity: z.coerce
    .number<string>()
    .int('Whole units only')
    .min(1, 'At least 1')
    .max(ORDER_LIMITS.maxQuantityPerLine, `At most ${ORDER_LIMITS.maxQuantityPerLine.toLocaleString('en-US')}`),
})

export const orderSchema = z.object({
  channelId: z.string().min(1, 'Choose a channel'),
  number: z.string().trim().max(ORDER_LIMITS.numberMaxLength).regex(ORDER_NUMBER_PATTERN, "Use letters, digits, '.', '_' or '-'"),
  customerName: z.string().trim().min(1, 'Enter the customer name').max(ORDER_LIMITS.customerNameMaxLength),
  items: z
    .array(orderItemSchema)
    .min(1, 'Add at least one product')
    .max(ORDER_LIMITS.maxLines, `An order can have at most ${ORDER_LIMITS.maxLines} lines`),
})

export type OrderFormInput = z.input<typeof orderSchema>
export type OrderFormValues = z.output<typeof orderSchema>

export const emptyOrderLine: OrderFormInput['items'][number] = { productId: '', quantity: '1' }
