import { LogOut } from 'lucide-react'
import { NavLink } from 'react-router'
import { useAuth } from '@/lib/auth/AuthContext'
import { cn } from '@/lib/cn'
import { Logo } from './Logo'
import { navigation } from './navigation'

export function MobileHeader() {
  const { logout } = useAuth()

  return (
    <header className="sticky top-0 z-30 flex items-center justify-between border-b border-slate-200 bg-white/90 px-4 py-3 backdrop-blur md:hidden">
      <Logo />
      <nav className="flex gap-1" aria-label="Main">
        {navigation.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            aria-label={label}
            className={({ isActive }) => cn('rounded-lg p-2', isActive ? 'bg-brand-50 text-brand-800' : 'text-slate-500')}
          >
            <Icon className="size-5" aria-hidden />
          </NavLink>
        ))}
        <button type="button" aria-label="Sign out" onClick={() => void logout()} className="cursor-pointer rounded-lg p-2 text-slate-500">
          <LogOut className="size-5" aria-hidden />
        </button>
      </nav>
    </header>
  )
}
