import { describe, expect, it } from 'vitest'
import { niceTicks } from './chart'
import { formatCompactCurrency, formatCurrency, formatShare, pluralize } from './format'

describe('formatting', () => {
  it('formats Turkish lira', () => {
    expect(formatCurrency(1234.5).replace(/\s/g, ' ')).toBe('₺1.234,50')
  })

  it('uses English compact notation on chart axes', () => {
    expect(formatCompactCurrency(8000)).toBe('₺8K')
    expect(formatCompactCurrency(12500)).toBe('₺12.5K')
  })

  it('pluralizes counts', () => {
    expect(pluralize(1, 'order')).toBe('1 order')
    expect(pluralize(3, 'order')).toBe('3 orders')
  })

  it('never rounds a real share down to zero', () => {
    expect(formatShare(0, 100)).toBe('0%')
    expect(formatShare(0.3, 100)).toBe('<1%')
    expect(formatShare(47, 100)).toBe('47%')
  })
})

describe('niceTicks', () => {
  it('picks round steps that cover the maximum', () => {
    expect(niceTicks(21546)).toEqual([0, 5000, 10000, 15000, 20000, 25000])
    expect(niceTicks(7750)).toEqual([0, 2000, 4000, 6000, 8000])
    expect(niceTicks(0)).toEqual([0, 1])
  })
})
