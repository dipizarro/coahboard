
export type PagedResult<T> = {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export type Client = {
  id: number | string
  fullName: string
  email?: string | null
  phone?: string | null
  birthDate?: string | null
  gender?: string | null
  initialHeightCm?: number | null
  mainGoal?: string | null
  experienceLevel?: string | null
  medicalNotes?: string | null
  injuryNotes?: string | null
  generalNotes?: string | null
  startDate?: string | null
  isActive: boolean
  coachId: number
  createdAt?: string
}

export type Athlete = {
  id: string
  firstName: string
  lastName: string
  email?: string | null
  phone?: string | null
  birthDate?: string | null
  gender?: string | null
  initialHeightCm?: number | null
  mainGoal?: string | null
  experienceLevel?: string | null
  medicalNotes?: string | null
  injuryNotes?: string | null
  generalNotes?: string | null
  startDate?: string | null
  isActive: boolean
  createdAt?: string
}

export type Exercise = {
  id: number
  name: string
  category: string
  defaultSets?: number | null
  defaultReps?: number | null
  createdAt?: string
}

export type RoutineItem = {
  exerciseId: number
  exerciseName?: string
  category?: string
  sets: number
  reps: number
  order: number
  notes?: string | null
}

export type Routine = {
  id: number
  title: string
  clientId: number
  createdAt?: string
  items: RoutineItem[]
}

export type SessionStatus = 'Planned' | 'Done' | 'Canceled' | 'Missed'
export type SessionType = 'Training' | 'PersonalBlock' | 'Other'

export type Session = {
  id: number
  coachId: number
  clientId?: number | null
  clientName?: string | null
  routineId?: number | null
  routineTitle?: string | null
  startAt: string
  endAt: string
  status: SessionStatus
  type: SessionType
  location?: string | null
  notes?: string | null
}
