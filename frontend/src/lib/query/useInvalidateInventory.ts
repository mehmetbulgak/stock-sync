import { useQueryClient } from '@tanstack/react-query'
import { useCallback } from 'react'
import { queryKeys } from './keys'

export function useInvalidateInventory() {
  const client = useQueryClient()

  return useCallback(
    () =>
      Promise.all([
        client.invalidateQueries({ queryKey: queryKeys.products }),
        client.invalidateQueries({ queryKey: queryKeys.orders }),
        client.invalidateQueries({ queryKey: queryKeys.dashboard }),
      ]),
    [client],
  )
}
