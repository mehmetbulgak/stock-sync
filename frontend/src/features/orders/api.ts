import { keepPreviousData, useMutation, useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api/client'
import type { Order, OrderStatus, OrderSummary, Paged } from '@/lib/api/types'
import { queryKeys } from '@/lib/query/keys'
import { useInvalidateInventory } from '@/lib/query/useInvalidateInventory'

export type OrderQuery = {
  status?: OrderStatus
  channelId?: string
  search?: string
  page?: number
  pageSize?: number
}

export type NewOrder = {
  channelId: string
  number: string | null
  customerName: string
  items: { productId: string; quantity: number }[]
}

export type OrderTransition = { action: 'ship' } | { action: 'cancel'; reason: string | null }

export const useOrders = (query: OrderQuery) =>
  useQuery({
    queryKey: queryKeys.orderList(query),
    queryFn: ({ signal }) => api<Paged<OrderSummary>>('/api/orders', { query, signal }),
    placeholderData: keepPreviousData,
  })

export const useOrder = (orderId: string | null) =>
  useQuery({
    queryKey: queryKeys.order(orderId ?? ''),
    queryFn: ({ signal }) => api<Order>(`/api/orders/${encodeURIComponent(orderId ?? '')}`, { signal }),
    enabled: orderId !== null,
  })

export function useCreateOrder() {
  const invalidate = useInvalidateInventory()

  return useMutation({
    mutationFn: (input: NewOrder) => api<Order>('/api/orders', { method: 'POST', body: input }),
    onSuccess: invalidate,
  })
}

export function useOrderTransition(orderId: string) {
  const invalidate = useInvalidateInventory()

  return useMutation({
    mutationFn: (transition: OrderTransition) =>
      api<Order>(`/api/orders/${encodeURIComponent(orderId)}/${transition.action}`, {
        method: 'POST',
        body: transition.action === 'cancel' ? { reason: transition.reason } : undefined,
      }),
    onSuccess: invalidate,
  })
}
