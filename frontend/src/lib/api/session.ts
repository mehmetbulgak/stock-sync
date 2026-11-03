import { toApiError } from './errors'
import type { AuthResponse } from './types'

type SessionListener = (session: AuthResponse | null) => void

const REFRESH_URL = '/api/auth/refresh'
const ROTATED_ELSEWHERE = 'Auth.RefreshTokenRotated'
const ROTATION_RETRY_DELAY_MS = 250

let accessToken: string | null = null
let refreshInFlight: Promise<AuthResponse | null> | null = null
const listeners = new Set<SessionListener>()

export function getAccessToken() {
  return accessToken
}

export function setSession(session: AuthResponse | null) {
  accessToken = session?.accessToken ?? null
  listeners.forEach((listener) => listener(session))
}

export function onSessionChange(listener: SessionListener) {
  listeners.add(listener)
  return () => {
    listeners.delete(listener)
  }
}

export function refreshSession(): Promise<AuthResponse | null> {
  refreshInFlight ??= requestRefresh()
    .catch(() => null)
    .then((session) => {
      setSession(session)
      return session
    })
    .finally(() => {
      refreshInFlight = null
    })
  return refreshInFlight
}

async function requestRefresh(retryIfRotatedElsewhere = true): Promise<AuthResponse | null> {
  const response = await fetch(REFRESH_URL, { method: 'POST', credentials: 'same-origin' })
  if (response.ok) {
    return (await response.json()) as AuthResponse
  }

  if (retryIfRotatedElsewhere && (await toApiError(response)).code === ROTATED_ELSEWHERE) {
    await new Promise((resolve) => setTimeout(resolve, ROTATION_RETRY_DELAY_MS))
    return requestRefresh(false)
  }

  return null
}
