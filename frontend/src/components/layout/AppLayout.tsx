import { Suspense } from 'react'
import { Outlet } from 'react-router'
import { Spinner } from '../ui/Spinner'
import { MobileHeader } from './MobileHeader'
import { Sidebar } from './Sidebar'

export function AppLayout() {
  return (
    <div className="flex min-h-dvh">
      <Sidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <MobileHeader />
        <main className="mx-auto w-full max-w-7xl flex-1 px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
          <Suspense fallback={<Spinner />}>
            <Outlet />
          </Suspense>
        </main>
      </div>
    </div>
  )
}
