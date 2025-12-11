import { api } from './client'
import type { Routine, PagedResult } from '../lib/types'

export async function list(params: {
  clientId: number
  page?: number
  pageSize?: number
  q?: string
}): Promise<PagedResult<Routine>> {
  const { clientId, page = 1, pageSize = 20, q } = params
  const { data } = await api.get<PagedResult<Routine>>('/api/Routines', {
    params: { clientId, page, pageSize, q },
  })
  return data
}

export async function get(id: number): Promise<Routine> {
  const { data } = await api.get<Routine>(`/api/Routines/${id}`)
  return data
}

export async function create(payload: {
  title: string
  clientId: number
  items: Array<{
    exerciseId: number
    sets: number
    reps: number
    order: number
    notes?: string | null
  }>
}): Promise<Routine> {
  const { data } = await api.post<Routine>('/api/Routines', payload)
  return data
}

export async function update(
  id: number,
  payload: {
    title: string
    items: Array<{
      exerciseId: number
      sets: number
      reps: number
      order: number
      notes?: string | null
    }>
  },
): Promise<Routine> {
  const { data } = await api.put<Routine>(`/api/Routines/${id}`, payload)
  return data
}

export const remove = async (id: number) => (await api.delete(`/api/Routines/${id}`)).data

