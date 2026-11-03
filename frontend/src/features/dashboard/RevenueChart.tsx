import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { DailySales } from '@/lib/api/types'
import { niceTicks } from '@/lib/chart'
import { formatCompactCurrency, formatCurrency, formatDay, pluralize } from '@/lib/format'

const BAR_COLOR = '#0f766e'
const GRID_COLOR = '#e2e8f0'
const AXIS_COLOR = '#cbd5e1'
const TICK_STYLE = { fill: '#64748b', fontSize: 12 }

function RevenueTooltip({ active, day }: { active?: boolean; day?: DailySales }) {
  if (!active || !day) return null

  return (
    <div className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs shadow-lg">
      <p className="font-medium text-slate-900">{formatDay(day.date)}</p>
      <p className="tabular mt-1 text-slate-700">{formatCurrency(day.revenue)}</p>
      <p className="tabular text-slate-500">{pluralize(day.orders, 'order')}</p>
    </div>
  )
}

export function RevenueChart({ data }: { data: DailySales[] }) {
  const ticks = niceTicks(Math.max(...data.map((d) => d.revenue)))

  return (
    <div className="px-2 pt-4 pb-2">
      <div className="h-64" aria-hidden>
        <ResponsiveContainer width="100%" height="100%">
          <BarChart data={data} margin={{ top: 8, right: 16, left: 8, bottom: 0 }}>
            <CartesianGrid vertical={false} stroke={GRID_COLOR} strokeWidth={1} />
            <XAxis dataKey="date" tickFormatter={formatDay} tickLine={false} axisLine={{ stroke: AXIS_COLOR }} tick={TICK_STYLE} />
            <YAxis
              ticks={ticks}
              domain={[0, ticks[ticks.length - 1] ?? 0]}
              tickFormatter={(v: number) => formatCompactCurrency(v)}
              tickLine={false}
              axisLine={false}
              width={64}
              tick={TICK_STYLE}
            />
            <Tooltip
              cursor={{ fill: '#f1f5f9' }}
              content={({ active, payload }) => <RevenueTooltip active={active} day={payload?.[0]?.payload as DailySales | undefined} />}
            />
            <Bar dataKey="revenue" fill={BAR_COLOR} radius={[4, 4, 0, 0]} maxBarSize={24} />
          </BarChart>
        </ResponsiveContainer>
      </div>
      <table className="sr-only">
        <caption>Revenue per day, last 7 days</caption>
        <thead>
          <tr>
            <th>Day</th>
            <th>Orders</th>
            <th>Revenue</th>
          </tr>
        </thead>
        <tbody>
          {data.map((d) => (
            <tr key={d.date}>
              <td>{formatDay(d.date)}</td>
              <td>{d.orders}</td>
              <td>{formatCurrency(d.revenue)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
