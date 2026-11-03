import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowRight } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router'
import { Button } from '@/components/ui/Button'
import { Field } from '@/components/ui/Field'
import { FormError } from '@/components/ui/FormError'
import { Input } from '@/components/ui/FormControls'
import { describeError } from '@/lib/api/errors'
import { useAuth } from '@/lib/auth/AuthContext'
import { AuthLayout } from './AuthLayout'
import { DEMO_CREDENTIALS } from './demo'
import { loginSchema, type LoginValues } from './schemas'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<LoginValues>({ resolver: zodResolver(loginSchema), defaultValues: { email: '', password: '' } })
  const { errors, isSubmitting } = form.formState

  const signIn = async ({ email, password }: LoginValues) => {
    setError(null)
    try {
      await login(email, password)
      navigate('/', { replace: true })
    } catch (e) {
      setError(describeError(e))
    }
  }

  return (
    <AuthLayout
      title="Sign in"
      subtitle={
        <>
          New here?{' '}
          <Link to="/register" className="font-medium text-brand-700 hover:underline">
            Create a store
          </Link>
        </>
      }
    >
      <form className="grid gap-4" onSubmit={form.handleSubmit(signIn)} noValidate>
        <FormError message={error} />
        <Field label="Email" error={errors.email?.message}>
          {(props) => <Input type="email" autoComplete="email" {...props} {...form.register('email')} />}
        </Field>
        <Field label="Password" error={errors.password?.message}>
          {(props) => <Input type="password" autoComplete="current-password" {...props} {...form.register('password')} />}
        </Field>
        <Button type="submit" size="lg" loading={isSubmitting} className="mt-2">
          Sign in
        </Button>
      </form>

      <div className="mt-6 rounded-xl border border-dashed border-brand-200 bg-brand-50/60 p-4">
        <p className="text-sm font-medium text-slate-900">Just looking around?</p>
        <p className="mt-0.5 text-sm text-slate-600">A demo store with 18 products and three weeks of orders is ready.</p>
        <p className="mt-1 text-xs text-slate-500">Shared sample data. Changes may reset. Please don’t enter personal information.</p>
        <Button variant="secondary" size="sm" className="mt-3" disabled={isSubmitting} onClick={() => void signIn(DEMO_CREDENTIALS)}>
          Open the demo store <ArrowRight />
        </Button>
      </div>
    </AuthLayout>
  )
}
