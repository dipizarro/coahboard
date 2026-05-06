import { api } from './client'
import type { ClientProgressPayload, ClientProgressRecord } from '../lib/types'

export async function list(clientId: number): Promise<ClientProgressRecord[]> {
  const { data } = await api.get<ClientProgressRecord[]>(`/api/clients/${clientId}/progress`)
  return data
}

export async function create(clientId: number, payload: ClientProgressPayload): Promise<ClientProgressRecord> {
  const { data } = await api.post<ClientProgressRecord>(`/api/clients/${clientId}/progress`, payload)
  return data
}

export async function update(
  clientId: number,
  progressId: number,
  payload: ClientProgressPayload,
): Promise<ClientProgressRecord> {
  const { data } = await api.put<ClientProgressRecord>(`/api/clients/${clientId}/progress/${progressId}`, payload)
  return data
}

export async function remove(clientId: number, progressId: number) {
  const { data } = await api.delete(`/api/clients/${clientId}/progress/${progressId}`)
  return data
}
