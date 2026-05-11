import { api } from './client'
import type { ExerciseMedia } from '../lib/types'

export async function list(exerciseId: number): Promise<ExerciseMedia[]> {
  const { data } = await api.get<ExerciseMedia[]>(`/api/exercises/${exerciseId}/media`)
  return data
}

export async function upload(
  exerciseId: number,
  payload: {
    file: File
    title?: string | null
    description?: string | null
  },
): Promise<ExerciseMedia> {
  const formData = new FormData()
  formData.append('file', payload.file)
  if (payload.title) formData.append('title', payload.title)
  if (payload.description) formData.append('description', payload.description)

  const { data } = await api.post<ExerciseMedia>(`/api/exercises/${exerciseId}/media`, formData)
  return data
}

export async function remove(exerciseId: number, mediaId: number) {
  const { data } = await api.delete(`/api/exercises/${exerciseId}/media/${mediaId}`)
  return data
}
