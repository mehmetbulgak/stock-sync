import { LayoutDashboard, Package, ShoppingCart, type LucideIcon } from 'lucide-react'

export type NavItem = { to: string; label: string; icon: LucideIcon; end: boolean }

export const navigation: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: LayoutDashboard, end: true },
  { to: '/products', label: 'Products', icon: Package, end: false },
  { to: '/orders', label: 'Orders', icon: ShoppingCart, end: false },
]
