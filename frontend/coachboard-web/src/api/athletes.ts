import { api } from './client'
import type { Athlete, Client, PagedResult } from '../lib/types'

const COACH_ID = Number(import.meta.env.VITE_DEFAULT_COACH_ID || 1)

function toAthlete(c: Client): Athlete {
  // tu backend trae fullName -> spliteamos
  const [firstName, ...rest] = (c.fullName ?? '').split(' ')
  return {
    id: String(c.id),
    firstName: firstName || '',
    lastName: rest.join(' ') || '',
    email: c.email,
    phone: c.phone,
    createdAt: c.createdAt,
  }
}

export async function list(params?: { page?: number; pageSize?: number; q?: string }): Promise<PagedResult<Athlete>> {
  const { page = 1, pageSize = 20, q } = params || {}
  const { data } = await api.get<PagedResult<Client>>('/api/Clients', {
    params: { coachId: COACH_ID, page, pageSize, q }
  })
  return {
    ...data,
    items: data.items.map(toAthlete)
  }
}

export async function get(id: string): Promise<Athlete> {
  const { data } = await api.get<Client>(`/api/Clients/${id}`)
  return toAthlete(data)
}

export async function create(p: Partial<Athlete>) {
  // opcional: unir first+last para enviar fullName si el backend lo espera
  const payload = { fullName: `${p.firstName ?? ''} ${p.lastName ?? ''}`.trim(), email: p.email, phone: p.phone, coachId: COACH_ID }
  const { data } = await api.post<Client>('/api/Clients', payload)
  return toAthlete(data)
}

export async function update(id: string, p: Partial<Athlete>) {
  const payload = { fullName: `${p.firstName ?? ''} ${p.lastName ?? ''}`.trim(), email: p.email, phone: p.phone, coachId: COACH_ID }
  const { data } = await api.put<Client>(`/api/Clients/${id}`, payload)
  return toAthlete(data)
}

export const remove = async (id: string) => (await api.delete(`/api/Clients/${id}`)).data
