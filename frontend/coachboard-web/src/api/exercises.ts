import { api } from './client'
import type { Exercise, PagedResult } from '../lib/types'

type ExercisePayload = {
  name: string
  category: string
  defaultSets?: number | null
  defaultReps?: number | null
  coachId?: number | null
  isGlobal?: boolean
  description?: string | null
  instructions?: string | null
  videoUrl?: string | null
  referenceUrl?: string | null
  difficultyLevel?: string | null
  movementPattern?: string | null
  equipment?: string | null
  targetMuscleGroup?: string | null
  secondaryMuscleGroups?: string | null
  exerciseType?: string | null
  environment?: string | null
  tags?: string | null
  isActive?: boolean
}

export async function search(params: {
  page?: number
  pageSize?: number
  q?: string
  category?: string
  targetMuscleGroup?: string
  equipment?: string
  difficultyLevel?: string
  exerciseType?: string
  environment?: string
  tag?: string
}): Promise<PagedResult<Exercise>> {
  const {
    page = 1,
    pageSize = 20,
    q,
    category,
    targetMuscleGroup,
    equipment,
    difficultyLevel,
    exerciseType,
    environment,
    tag,
  } = params
  const { data } = await api.get<PagedResult<Exercise>>('/api/Exercises', {
    params: {
      page,
      pageSize,
      q,
      category,
      targetMuscleGroup,
      equipment,
      difficultyLevel,
      exerciseType,
      environment,
      tag,
    },
  })
  return data
}

export async function get(id: number): Promise<Exercise> {
  const { data } = await api.get<Exercise>(`/api/Exercises/${id}`)
  return data
}

export async function create(payload: ExercisePayload): Promise<Exercise> {
  const { data } = await api.post<Exercise>('/api/Exercises', payload)
  return data
}

export async function update(
  id: number,
  payload: ExercisePayload,
): Promise<Exercise> {
  const { data } = await api.put<Exercise>(`/api/Exercises/${id}`, payload)
  return data
}

export const remove = async (id: number) => (await api.delete(`/api/Exercises/${id}`)).data
