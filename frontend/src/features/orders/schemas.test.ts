import { describe, expect, it } from 'vitest'
import { ORDER_LIMITS, emptyOrderLine, orderSchema } from './schemas'

const validOrder = {
  channelId: 'channel-1',
  number: '',
  customerName: 'Ayşe Yılmaz',
  items: [{ productId: 'product-1', quantity: '2' }],
}

describe('orderSchema', () => {
  it('accepts an order without a number so the server can generate one', () => {
    expect(orderSchema.parse(validOrder)).toEqual({ ...validOrder, items: [{ productId: 'product-1', quantity: 2 }] })
  })

  it.each(['TY 1', 'TY/1', "1'; DROP TABLE orders"])('rejects order number %s', (number) => {
    expect(orderSchema.safeParse({ ...validOrder, number }).success).toBe(false)
  })

  it('rejects empty and oversized orders', () => {
    const tooMany = Array.from({ length: ORDER_LIMITS.maxLines + 1 }, () => ({ productId: 'product-1', quantity: '1' }))

    expect(orderSchema.safeParse({ ...validOrder, items: [] }).success).toBe(false)
    expect(orderSchema.safeParse({ ...validOrder, items: tooMany }).success).toBe(false)
  })

  it('rejects lines without a product or with an invalid quantity', () => {
    expect(orderSchema.safeParse({ ...validOrder, items: [emptyOrderLine] }).success).toBe(false)
    expect(orderSchema.safeParse({ ...validOrder, items: [{ productId: 'product-1', quantity: '0' }] }).success).toBe(false)
    expect(orderSchema.safeParse({ ...validOrder, items: [{ productId: 'product-1', quantity: '10001' }] }).success).toBe(false)
  })
})
