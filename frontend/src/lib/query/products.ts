import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { api } from '../api/client'
import type { Paged, Product } from '../api/types'
import { queryKeys } from './keys'

export type ProductSort = 'Name' | 'Sku' | 'Available' | 'UpdatedAt'

export type ProductQuery = {
  search?: string
  lowStockOnly?: boolean
  includeInactive?: boolean
  sort?: ProductSort
  descending?: boolean
  page?: number
  pageSize?: number
}

export const useProducts = (query: ProductQuery) =>
  useQuery({
    queryKey: queryKeys.productList(query),
    queryFn: ({ signal }) => api<Paged<Product>>('/api/products', { query, signal }),
    placeholderData: keepPreviousData,
  })
