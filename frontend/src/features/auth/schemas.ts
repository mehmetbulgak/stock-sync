import { z } from 'zod'

const PASSWORD_MIN = 8
const PASSWORD_MAX = 128

export const loginSchema = z.object({
  email: z.email('Enter a valid email address').max(254),
  password: z.string().min(1, 'Enter your password').max(PASSWORD_MAX),
})

export const registerSchema = z.object({
  storeName: z.string().trim().min(1, 'Enter your store name').max(120),
  displayName: z.string().trim().min(1, 'Enter your name').max(100),
  email: z.email('Enter a valid email address').max(254),
  password: z
    .string()
    .min(PASSWORD_MIN, `Use at least ${PASSWORD_MIN} characters`)
    .max(PASSWORD_MAX)
    .regex(/[A-Za-z]/, 'Include at least one letter')
    .regex(/[0-9]/, 'Include at least one number'),
})

export type LoginValues = z.infer<typeof loginSchema>
export type RegisterValues = z.infer<typeof registerSchema>
