import { api } from './client'
import type { Session, SessionStatus, SessionType } from '../lib/types'

export async function list(params: {
    coachId: number
    from?: string
    to?: string
    clientId?: number | null
}) {
    const { data } = await api.get<Session[]>('/api/Sessions', { params })
    return data
}

export async function get(id: number) {
    const { data } = await api.get<Session>(`/api/Sessions/${id}`)
    return data
}

export async function create(payload: {
    coachId: number
    clientId?: number | null
    routineId?: number | null
    startAt: string
    endAt: string
    type: SessionType
    location?: string
    notes?: string
}) {
    const { data } = await api.post<Session>('/api/Sessions', payload)
    return data
}

export async function update(
    id: number,
    payload: {
        coachId: number
        clientId?: number | null
        routineId?: number | null
        startAt: string
        endAt: string
        type: SessionType
        location?: string
        notes?: string
        status: SessionStatus
    }
) {
    const { data } = await api.put<Session>(`/api/Sessions/${id}`, payload)
    return data
}

export async function updateStatus(id: number, status: SessionStatus) {
    const { data } = await api.patch(`/api/Sessions/${id}/status`, JSON.stringify(status), {
        headers: { 'Content-Type': 'application/json' },
    })
    return data
}

export async function remove(id: number) {
    const { data } = await api.delete(`/api/Sessions/${id}`)
    return data
}
