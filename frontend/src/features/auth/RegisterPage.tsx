import { zodResolver } from '@hookform/resolvers/zod'
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
import { registerSchema, type RegisterValues } from './schemas'

export function RegisterPage() {
  const { register: createAccount } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { storeName: '', displayName: '', email: '', password: '' },
  })
  const { errors, isSubmitting } = form.formState

  const submit = async (values: RegisterValues) => {
    setError(null)
    try {
      await createAccount(values)
      navigate('/', { replace: true })
    } catch (e) {
      setError(describeError(e))
    }
  }

  return (
    <AuthLayout
      title="Create your store"
      subtitle={
        <>
          Already have an account?{' '}
          <Link to="/login" className="font-medium text-brand-700 hover:underline">
            Sign in
          </Link>
        </>
      }
    >
      <form className="grid gap-4" onSubmit={form.handleSubmit(submit)} noValidate>
        <FormError message={error} />
        <Field label="Store name" error={errors.storeName?.message}>
          {(props) => <Input autoComplete="organization" {...props} {...form.register('storeName')} />}
        </Field>
        <Field label="Your name" error={errors.displayName?.message}>
          {(props) => <Input autoComplete="name" {...props} {...form.register('displayName')} />}
        </Field>
        <Field label="Email" error={errors.email?.message}>
          {(props) => <Input type="email" autoComplete="email" {...props} {...form.register('email')} />}
        </Field>
        <Field label="Password" error={errors.password?.message} hint="At least 8 characters, with a letter and a number.">
          {(props) => <Input type="password" autoComplete="new-password" {...props} {...form.register('password')} />}
        </Field>
        <Button type="submit" size="lg" loading={isSubmitting} className="mt-2">
          Create store
        </Button>
        <p className="text-xs text-slate-500">Your store starts with Trendyol, Hepsiburada, Web Store and Manual channels.</p>
      </form>
    </AuthLayout>
  )
}
