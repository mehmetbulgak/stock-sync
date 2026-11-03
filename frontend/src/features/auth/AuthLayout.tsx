import { BellRing, RefreshCcw, ShieldCheck } from 'lucide-react'
import type { ReactNode } from 'react'
import { Logo } from '@/components/layout/Logo'

const highlights = [
  {
    icon: RefreshCcw,
    title: 'One stock count, every channel',
    text: 'Trendyol, Hepsiburada, your web store and phone orders draw from the same numbers.',
  },
  {
    icon: ShieldCheck,
    title: 'No more overselling',
    text: 'Stock is reserved the moment an order lands, even when two arrive at the same second.',
  },
  {
    icon: BellRing,
    title: 'Know before you run out',
    text: 'Low-stock alerts and a full ledger of every unit that came in or went out.',
  },
]

type AuthLayoutProps = { title: string; subtitle: ReactNode; children: ReactNode }

export function AuthLayout({ title, subtitle, children }: AuthLayoutProps) {
  return (
    <div className="grid min-h-dvh lg:grid-cols-[minmax(0,5fr)_minmax(0,6fr)]">
      <aside className="relative hidden overflow-hidden bg-brand-950 p-10 text-white lg:flex lg:flex-col">
        <div className="absolute -top-32 -right-32 size-96 rounded-full bg-brand-700/30 blur-3xl" aria-hidden />
        <Logo inverted />
        <div className="relative my-auto max-w-md">
          <p className="text-sm font-medium tracking-wide text-brand-200 uppercase">For marketplace sellers</p>
          <h2 className="mt-3 text-3xl leading-tight font-semibold tracking-tight">Sell everywhere. Keep one stock count.</h2>
          <ul className="mt-10 grid gap-6">
            {highlights.map(({ icon: Icon, title: heading, text }) => (
              <li key={heading} className="flex gap-4">
                <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-white/10">
                  <Icon className="size-4.5 text-brand-200" aria-hidden />
                </div>
                <div>
                  <p className="font-medium">{heading}</p>
                  <p className="mt-0.5 text-sm text-brand-100/70">{text}</p>
                </div>
              </li>
            ))}
          </ul>
        </div>
        <p className="relative text-xs text-brand-100/50">Portfolio project · ASP.NET Core 10 + React</p>
      </aside>

      <main className="flex items-center justify-center px-6 py-12">
        <div className="w-full max-w-sm">
          <div className="mb-8 lg:hidden">
            <Logo />
          </div>
          <h1 className="text-2xl font-semibold tracking-tight text-slate-900">{title}</h1>
          <p className="mt-1.5 text-sm text-slate-500">{subtitle}</p>
          <div className="mt-8">{children}</div>
        </div>
      </main>
    </div>
  )
}
