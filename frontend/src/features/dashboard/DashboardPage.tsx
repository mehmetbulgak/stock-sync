import { AlertTriangle, Clock, ShoppingCart, Wallet } from 'lucide-react'
import { Link } from 'react-router'
import { Button } from '@/components/ui/Button'
import { Card, CardHeader } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { PageHeader } from '@/components/ui/PageHeader'
import type { Dashboard } from '@/lib/api/types'
import { useAuth } from '@/lib/auth/AuthContext'
import { formatCurrency, formatNumber } from '@/lib/format'
import { useDashboard } from './api'
import { ChannelBreakdown } from './ChannelBreakdown'
import { DashboardSkeleton } from './DashboardSkeleton'
import { KpiCard } from './KpiCard'
import { LowStockList } from './LowStockList'
import { RecentOrdersList } from './RecentOrdersList'
import { RevenueChart } from './RevenueChart'

const viewAllLink = 'text-sm font-medium text-brand-700 hover:underline'

export default function DashboardPage() {
  const { user } = useAuth()
  const { data, isPending, isError, refetch } = useDashboard()
  const firstName = user?.displayName.split(' ')[0] ?? ''

  return (
    <>
      <PageHeader title={`Good to see you, ${firstName}`} description="Here is how your store is doing across every channel." />
      {isError ? (
        <Card>
          <EmptyState
            icon={<AlertTriangle />}
            title="Couldn't load the dashboard"
            action={
              <Button variant="secondary" onClick={() => void refetch()}>
                Try again
              </Button>
            }
          />
        </Card>
      ) : isPending ? (
        <DashboardSkeleton />
      ) : (
        <DashboardContent data={data} />
      )}
    </>
  )
}

function DashboardContent({ data }: { data: Dashboard }) {
  return (
    <div className="grid gap-6">
      <div className="grid grid-cols-2 gap-3 sm:gap-4 xl:grid-cols-4">
        <KpiCard icon={<Wallet />} label="Revenue, last 7 days" value={formatCurrency(data.revenueLast7Days)} />
        <KpiCard icon={<ShoppingCart />} label="Orders today" value={formatNumber(data.ordersToday)} />
        <KpiCard icon={<Clock />} label="Waiting to ship" value={formatNumber(data.pendingOrders)} href="/orders?status=Pending" />
        <KpiCard
          icon={<AlertTriangle />}
          label="Low on stock"
          value={formatNumber(data.lowStockProducts)}
          sub={`of ${formatNumber(data.activeProducts)} products`}
          tone={data.lowStockProducts > 0 ? 'warning' : 'default'}
          href="/products?lowStockOnly=true"
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader title="Revenue per day" description="Last 7 days, excluding cancelled orders · Istanbul time" />
          <RevenueChart data={data.dailySales} />
        </Card>
        <Card>
          <CardHeader title="Sales by channel" description="Last 7 days" />
          <ChannelBreakdown channels={data.salesByChannel} />
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader
            title="Running low"
            description="Available stock at or below each product's threshold"
            action={
              <Link to="/products?lowStockOnly=true" className={viewAllLink}>
                View all
              </Link>
            }
          />
          <LowStockList products={data.lowStock} />
        </Card>
        <Card>
          <CardHeader
            title="Latest orders"
            action={
              <Link to="/orders" className={viewAllLink}>
                View all
              </Link>
            }
          />
          <RecentOrdersList orders={data.recentOrders} />
        </Card>
      </div>
    </div>
  )
}
