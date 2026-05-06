import { api } from './client'
import type { Athlete, Client, PagedResult } from '../lib/types'

function toAthlete(c: Client): Athlete {
  const [firstName, ...rest] = (c.fullName ?? '').split(' ')
  return {
    id: String(c.id),
    firstName: firstName || '',
    lastName: rest.join(' ') || '',
    email: c.email,
    phone: c.phone,
    birthDate: c.birthDate,
    gender: c.gender,
    initialHeightCm: c.initialHeightCm,
    mainGoal: c.mainGoal,
    experienceLevel: c.experienceLevel,
    medicalNotes: c.medicalNotes,
    injuryNotes: c.injuryNotes,
    generalNotes: c.generalNotes,
    startDate: c.startDate,
    isActive: c.isActive,
    createdAt: c.createdAt,
  }
}

function ensureCoachId(value?: number | null) {
  if (value == null) {
    throw new Error('El coachId es requerido para esta operación')
  }
  return value
}

export async function list(params: {
  page?: number
  pageSize?: number
  q?: string
  coachId: number | null
}): Promise<PagedResult<Athlete>> {
  const { page = 1, pageSize = 20, q, coachId } = params
  const finalCoachId = ensureCoachId(coachId)

  const { data } = await api.get<PagedResult<Client>>('/api/Clients', {
    params: { coachId: finalCoachId, page, pageSize, q }
  })

  return {
    ...data,
    items: data.items.map(toAthlete),
  }
}

export async function get(id: string): Promise<Athlete> {
  const { data } = await api.get<Client>(`/api/Clients/${id}`)
  return toAthlete(data)
}

export async function create(p: Partial<Athlete>, coachId?: number | null) {
  const fullName = `${p.firstName ?? ''} ${p.lastName ?? ''}`.trim()
  const payload = {
    fullName,
    email: p.email,
    phone: p.phone,
    birthDate: p.birthDate,
    gender: p.gender,
    initialHeightCm: p.initialHeightCm,
    mainGoal: p.mainGoal,
    experienceLevel: p.experienceLevel,
    medicalNotes: p.medicalNotes,
    injuryNotes: p.injuryNotes,
    generalNotes: p.generalNotes,
    startDate: p.startDate,
    isActive: p.isActive ?? true,
    coachId: ensureCoachId(coachId),
  }
  const { data } = await api.post<Client>('/api/Clients', payload)
  return toAthlete(data)
}

export async function update(id: string, p: Partial<Athlete>, coachId?: number | null) {
  const fullName = `${p.firstName ?? ''} ${p.lastName ?? ''}`.trim()
  const payload = {
    fullName,
    email: p.email,
    phone: p.phone,
    birthDate: p.birthDate,
    gender: p.gender,
    initialHeightCm: p.initialHeightCm,
    mainGoal: p.mainGoal,
    experienceLevel: p.experienceLevel,
    medicalNotes: p.medicalNotes,
    injuryNotes: p.injuryNotes,
    generalNotes: p.generalNotes,
    startDate: p.startDate,
    isActive: p.isActive ?? true,
  }
  ensureCoachId(coachId)
  const { data } = await api.put<Client>(`/api/Clients/${id}`, payload)
  return toAthlete(data)
}

export const remove = async (id: string) => (await api.delete(`/api/Clients/${id}`)).data
