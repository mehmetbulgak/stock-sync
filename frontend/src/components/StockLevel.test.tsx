import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { StockLevel } from './StockLevel'

describe('StockLevel', () => {
  it('shows available and reserved units', () => {
    render(<StockLevel product={{ onHand: 10, reserved: 3, available: 7, isLowStock: false }} />)

    expect(screen.getByText('7 available')).toBeInTheDocument()
    expect(screen.getByText('3 reserved')).toBeInTheDocument()
    expect(screen.queryByText('Low')).not.toBeInTheDocument()
  })

  it('flags low stock with a text badge, not colour alone', () => {
    render(<StockLevel product={{ onHand: 5, reserved: 3, available: 2, isLowStock: true }} />)

    expect(screen.getByText('Low')).toBeInTheDocument()
  })

  it('flags sold-out products', () => {
    render(<StockLevel product={{ onHand: 4, reserved: 4, available: 0, isLowStock: true }} />)

    expect(screen.getByText('Out')).toBeInTheDocument()
  })
})
