import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { act, render } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { refreshSession, setSession } from '../api/session'
import type { AuthResponse } from '../api/types'
import { AuthProvider } from './AuthProvider'

const sessionFor = (userId: string): AuthResponse => ({
  accessToken: `token-${userId}`,
  expiresAt: '2026-11-03T10:00:00Z',
  user: { id: userId, email: `${userId}@test.dev`, displayName: userId, role: 'Owner', tenantId: `tenant-${userId}`, storeName: 'S' },
})

describe('AuthProvider', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 401 })))
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('drops cached data when the session ends or another user signs in', async () => {
    const queryClient = new QueryClient()
    render(
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <span />
        </AuthProvider>
      </QueryClientProvider>,
    )
    await act(() => refreshSession())

    act(() => setSession(sessionFor('alice')))
    queryClient.setQueryData(['products'], ['alice-product'])
    act(() => setSession(sessionFor('alice')))
    expect(queryClient.getQueryData(['products'])).toEqual(['alice-product'])

    act(() => setSession(null))
    expect(queryClient.getQueryData(['products'])).toBeUndefined()

    act(() => setSession(sessionFor('alice')))
    queryClient.setQueryData(['products'], ['alice-product'])
    act(() => setSession(sessionFor('bob')))
    expect(queryClient.getQueryData(['products'])).toBeUndefined()
  })
})
