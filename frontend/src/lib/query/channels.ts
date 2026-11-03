import { useQuery } from '@tanstack/react-query'
import { api } from '../api/client'
import type { Channel } from '../api/types'
import { queryKeys } from './keys'

export const useChannels = () =>
  useQuery({
    queryKey: queryKeys.channels,
    queryFn: ({ signal }) => api<Channel[]>('/api/channels', { signal }),
    staleTime: 5 * 60_000,
  })
