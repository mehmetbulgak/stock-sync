import type { ReactNode } from 'react'
import { Navigate } from 'react-router'
import { useAuth } from '@/lib/auth/AuthContext'
import { Spinner } from './ui/Spinner'

export function RequireAuth({ children }: { children: ReactNode }) {
  const { status } = useAuth()
  if (status === 'loading') return <Spinner fullPage />
  if (status === 'anonymous') return <Navigate to="/login" replace />
  return children
}

export function GuestOnly({ children }: { children: ReactNode }) {
  const { status } = useAuth()
  if (status === 'loading') return <Spinner fullPage />
  if (status === 'authenticated') return <Navigate to="/" replace />
  return children
}
