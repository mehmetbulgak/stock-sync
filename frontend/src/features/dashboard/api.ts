import { useQuery } from '@tanstack/react-query'
import { api } from '@/lib/api/client'
import type { Dashboard } from '@/lib/api/types'
import { queryKeys } from '@/lib/query/keys'

export const useDashboard = () =>
  useQuery({
    queryKey: queryKeys.dashboard,
    queryFn: ({ signal }) => api<Dashboard>('/api/dashboard', { signal }),
  })
