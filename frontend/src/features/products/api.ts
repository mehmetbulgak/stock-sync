import { useMutation, useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api/client'
import type { Product, StockMovement } from '@/lib/api/types'
import { queryKeys } from '@/lib/query/keys'
import { useInvalidateInventory } from '@/lib/query/useInvalidateInventory'

export type NewProduct = {
  sku: string
  name: string
  description: string | null
  price: number
  initialStock: number
  lowStockThreshold: number
}

export type ProductChanges = Omit<NewProduct, 'sku' | 'initialStock'> & { isActive: boolean }

export type StockAdjustment = { quantity: number; reason: string }

export const useMovements = (productId: string) =>
  useQuery({
    queryKey: queryKeys.movements(productId),
    queryFn: ({ signal }) => api<StockMovement[]>(`/api/products/${encodeURIComponent(productId)}/movements`, { signal }),
  })

export function useCreateProduct() {
  const invalidate = useInvalidateInventory()

  return useMutation({
    mutationFn: (input: NewProduct) => api<Product>('/api/products', { method: 'POST', body: input }),
    onSuccess: invalidate,
  })
}

export function useUpdateProduct(productId: string) {
  const invalidate = useInvalidateInventory()

  return useMutation({
    mutationFn: (changes: ProductChanges) =>
      api<Product>(`/api/products/${encodeURIComponent(productId)}`, { method: 'PUT', body: changes }),
    onSuccess: invalidate,
  })
}

export function useAdjustStock(productId: string) {
  const invalidate = useInvalidateInventory()

  return useMutation({
    mutationFn: (adjustment: StockAdjustment) =>
      api<Product>(`/api/products/${encodeURIComponent(productId)}/stock-adjustments`, { method: 'POST', body: adjustment }),
    onSuccess: invalidate,
  })
}
