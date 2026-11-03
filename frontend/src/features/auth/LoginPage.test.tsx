import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { createMemoryRouter, RouterProvider } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from '@/lib/api/errors'
import { AuthContext, type AuthContextValue } from '@/lib/auth/AuthContext'
import { DEMO_CREDENTIALS } from './demo'
import { LoginPage } from './LoginPage'

function renderLogin(login: AuthContextValue['login']) {
  const auth: AuthContextValue = { status: 'anonymous', user: null, login, register: vi.fn(), logout: vi.fn() }
  const router = createMemoryRouter(
    [
      { path: '/login', element: <LoginPage /> },
      { path: '/', element: <p>Dashboard</p> },
    ],
    { initialEntries: ['/login'] },
  )

  render(
    <QueryClientProvider client={new QueryClient()}>
      <AuthContext.Provider value={auth}>
        <RouterProvider router={router} />
      </AuthContext.Provider>
    </QueryClientProvider>,
  )
}

describe('LoginPage', () => {
  it('validates before calling the API', async () => {
    const login = vi.fn()
    renderLogin(login)

    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByText('Enter a valid email address')).toBeInTheDocument()
    expect(screen.getByText('Enter your password')).toBeInTheDocument()
    expect(screen.getByLabelText('Email')).toHaveAttribute('aria-invalid', 'true')
    expect(login).not.toHaveBeenCalled()
  })

  it('signs in and goes to the dashboard', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    renderLogin(login)

    await userEvent.type(screen.getByLabelText('Email'), 'seller@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'Passw0rd!')
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(login).toHaveBeenCalledWith('seller@example.com', 'Passw0rd!')
    expect(await screen.findByText('Dashboard')).toBeInTheDocument()
  })

  it('shows the API error message when credentials are wrong', async () => {
    const login = vi.fn().mockRejectedValue(new ApiError(401, 'Email or password is incorrect.', 'Auth.InvalidCredentials'))
    renderLogin(login)

    await userEvent.type(screen.getByLabelText('Email'), 'seller@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'wrong')
    await userEvent.click(screen.getByRole('button', { name: 'Sign in' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('Email or password is incorrect.')
  })

  it('offers a one-click demo login', async () => {
    const login = vi.fn().mockResolvedValue(undefined)
    renderLogin(login)

    await userEvent.click(screen.getByRole('button', { name: /open the demo store/i }))

    expect(login).toHaveBeenCalledWith(DEMO_CREDENTIALS.email, DEMO_CREDENTIALS.password)
  })
})
