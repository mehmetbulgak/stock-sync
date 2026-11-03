import { LogOut } from 'lucide-react'
import { NavLink } from 'react-router'
import { useAuth } from '@/lib/auth/AuthContext'
import { cn } from '@/lib/cn'
import { Logo } from './Logo'
import { navigation } from './navigation'

export function Sidebar() {
  const { user, logout } = useAuth()

  return (
    <div className="hidden w-60 shrink-0 border-r border-slate-200 bg-white md:block">
      <aside className="sticky top-0 flex h-dvh flex-col px-3 py-5">
        <div className="px-2">
          <Logo />
        </div>

        <nav className="mt-8 grid gap-1" aria-label="Main">
          {navigation.map(({ to, label, icon: Icon, end }) => (
            <NavLink
              key={to}
              to={to}
              end={end}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                  isActive ? 'bg-brand-50 text-brand-800' : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900',
                )
              }
            >
              <Icon className="size-4" aria-hidden />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="mt-auto rounded-xl border border-slate-200 p-3">
          <p className="truncate text-sm font-medium text-slate-900">{user?.storeName}</p>
          <p className="truncate text-xs text-slate-500">{user?.email}</p>
          <button
            type="button"
            onClick={() => void logout()}
            className="mt-3 flex w-full cursor-pointer items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-slate-600 hover:bg-slate-100 hover:text-slate-900"
          >
            <LogOut className="size-4" aria-hidden /> Sign out
          </button>
        </div>
      </aside>
    </div>
  )
}
