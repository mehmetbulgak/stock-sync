import { z } from 'zod'

export const PRODUCT_LIMITS = {
  skuMaxLength: 64,
  nameMaxLength: 200,
  descriptionMaxLength: 2000,
  maxPrice: 1_000_000,
  maxStock: 10_000_000,
  reasonMaxLength: 300,
} as const

const SKU_PATTERN = /^[A-Za-z0-9._-]+$/

const wholeUnits = (minimum: number, minimumMessage: string) =>
  z.coerce
    .number<string>()
    .int('Whole units only')
    .min(minimum, minimumMessage)
    .max(PRODUCT_LIMITS.maxStock, `At most ${PRODUCT_LIMITS.maxStock.toLocaleString('en-US')} units`)

export const productSchema = z.object({
  sku: z
    .string()
    .trim()
    .min(1, 'SKU is required')
    .max(PRODUCT_LIMITS.skuMaxLength)
    .regex(SKU_PATTERN, "Use letters, digits, '.', '_' or '-'"),
  name: z.string().trim().min(1, 'Name is required').max(PRODUCT_LIMITS.nameMaxLength),
  description: z.string().max(PRODUCT_LIMITS.descriptionMaxLength),
  price: z.coerce
    .number<string>()
    .min(0, 'Price cannot be negative')
    .max(PRODUCT_LIMITS.maxPrice, 'Price is too high')
    .multipleOf(0.01, 'Use at most two decimals'),
  initialStock: wholeUnits(0, 'Cannot be negative'),
  lowStockThreshold: wholeUnits(0, 'Cannot be negative'),
  isActive: z.boolean(),
})

export type ProductFormInput = z.input<typeof productSchema>
export type ProductFormValues = z.output<typeof productSchema>

export const ADJUST_DIRECTIONS = ['add', 'remove'] as const

export const adjustStockSchema = z.object({
  direction: z.enum(ADJUST_DIRECTIONS),
  quantity: wholeUnits(1, 'Enter at least 1'),
  reason: z.string().trim().min(1, 'Say why stock is changing').max(PRODUCT_LIMITS.reasonMaxLength),
})

export type AdjustStockInput = z.input<typeof adjustStockSchema>
export type AdjustStockValues = z.output<typeof adjustStockSchema>
