import { describe, expect, it } from 'vitest'
import { adjustStockSchema, productSchema } from './schemas'

const validProduct = {
  sku: 'TSHIRT-01',
  name: 'Basic tee',
  description: '',
  price: '199.90',
  initialStock: '10',
  lowStockThreshold: '2',
  isActive: true,
}

describe('productSchema', () => {
  it('coerces numeric fields from form strings', () => {
    const result = productSchema.parse(validProduct)

    expect(result).toMatchObject({ price: 199.9, initialStock: 10, lowStockThreshold: 2 })
  })

  it.each([
    ['sku', 'has spaces'],
    ['sku', '<script>'],
    ['price', '-1'],
    ['price', '1000000.01'],
    ['price', '10.999'],
    ['initialStock', '1.5'],
    ['initialStock', '10000001'],
    ['lowStockThreshold', '-3'],
  ])('rejects %s = %s', (field, value) => {
    const result = productSchema.safeParse({ ...validProduct, [field]: value })

    expect(result.success).toBe(false)
    expect(result.error?.issues[0]?.path).toEqual([field])
  })
})

describe('adjustStockSchema', () => {
  it('requires a positive whole quantity and a reason', () => {
    expect(adjustStockSchema.safeParse({ direction: 'add', quantity: '0', reason: 'Count' }).success).toBe(false)
    expect(adjustStockSchema.safeParse({ direction: 'remove', quantity: '3', reason: '   ' }).success).toBe(false)
    expect(adjustStockSchema.parse({ direction: 'remove', quantity: '3', reason: ' Damaged ' })).toEqual({
      direction: 'remove',
      quantity: 3,
      reason: 'Damaged',
    })
  })
})
