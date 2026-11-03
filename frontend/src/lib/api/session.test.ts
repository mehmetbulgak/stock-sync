import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { getAccessToken, refreshSession, setSession } from './session'
import type { AuthResponse } from './types'

const session: AuthResponse = {
  accessToken: 'fresh-token',
  expiresAt: '2026-11-03T10:00:00Z',
  user: { id: 'u1', email: 'a@b.c', displayName: 'A', role: 'Owner', tenantId: 't1', storeName: 'S' },
}

const json = (status: number, body: unknown) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/problem+json' } })

const fetchMock = vi.fn<typeof fetch>()

describe('refreshSession', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', fetchMock)
    setSession(null)
  })

  afterEach(() => {
    fetchMock.mockReset()
    vi.unstubAllGlobals()
  })

  it('retries once when another tab rotated the token a moment earlier', async () => {
    fetchMock.mockResolvedValueOnce(json(401, { code: 'Auth.RefreshTokenRotated' })).mockResolvedValueOnce(json(200, session))

    await expect(refreshSession()).resolves.toEqual(session)

    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(getAccessToken()).toBe('fresh-token')
  })

  it('gives up without retrying when the session is invalid', async () => {
    fetchMock.mockResolvedValueOnce(json(401, { code: 'Auth.InvalidRefreshToken' }))

    await expect(refreshSession()).resolves.toBeNull()

    expect(fetchMock).toHaveBeenCalledTimes(1)
    expect(getAccessToken()).toBeNull()
  })

  it('shares one request between concurrent callers', async () => {
    fetchMock.mockResolvedValueOnce(json(200, session))

    const [first, second] = await Promise.all([refreshSession(), refreshSession()])

    expect(first).toEqual(session)
    expect(second).toEqual(session)
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })
})
