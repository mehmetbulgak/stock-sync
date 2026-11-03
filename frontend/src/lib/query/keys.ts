export const queryKeys = {
  dashboard: ['dashboard'] as const,
  channels: ['channels'] as const,
  products: ['products'] as const,
  productList: (params: object) => ['products', 'list', params] as const,
  movements: (productId: string) => ['products', 'movements', productId] as const,
  orders: ['orders'] as const,
  orderList: (params: object) => ['orders', 'list', params] as const,
  order: (id: string) => ['orders', 'detail', id] as const,
}
