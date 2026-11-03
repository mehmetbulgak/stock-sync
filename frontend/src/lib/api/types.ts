export type User = {
  id: string
  email: string
  displayName: string
  role: 'Owner' | 'Staff'
  tenantId: string
  storeName: string
}

export type AuthResponse = {
  accessToken: string
  expiresAt: string
  user: User
}

export type Paged<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export type Product = {
  id: string
  sku: string
  name: string
  description: string | null
  price: number
  onHand: number
  reserved: number
  available: number
  lowStockThreshold: number
  isLowStock: boolean
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export type StockMovementType = 'InitialStock' | 'Adjustment' | 'Reservation' | 'ReservationReleased' | 'Shipment'

export type StockMovement = {
  id: string
  type: StockMovementType
  quantity: number
  onHandAfter: number
  reservedAfter: number
  note: string | null
  orderId: string | null
  createdAt: string
}

export type ChannelType = 'Trendyol' | 'Hepsiburada' | 'WebStore' | 'Manual'

export type Channel = {
  id: string
  name: string
  type: ChannelType
  isActive: boolean
}

export type OrderStatus = 'Pending' | 'Shipped' | 'Cancelled'

export type OrderSummary = {
  id: string
  number: string
  channelId: string
  channelName: string
  channelType: ChannelType
  customerName: string
  status: OrderStatus
  total: number
  itemCount: number
  createdAt: string
}

export type OrderLine = {
  productId: string
  sku: string
  productName: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export type Order = Omit<OrderSummary, 'itemCount'> & {
  shippedAt: string | null
  cancelledAt: string | null
  cancellationReason: string | null
  lines: OrderLine[]
}

export type DailySales = { date: string; orders: number; revenue: number }

export type ChannelSales = { channelName: string; channelType: ChannelType; orders: number; revenue: number }

export type Dashboard = {
  activeProducts: number
  lowStockProducts: number
  pendingOrders: number
  ordersToday: number
  revenueLast7Days: number
  dailySales: DailySales[]
  salesByChannel: ChannelSales[]
  lowStock: Product[]
  recentOrders: OrderSummary[]
}
