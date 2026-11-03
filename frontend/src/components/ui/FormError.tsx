export function FormError({ message }: { message?: string | null }) {
  if (!message) return null

  return (
    <div role="alert" className="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
      {message}
    </div>
  )
}
