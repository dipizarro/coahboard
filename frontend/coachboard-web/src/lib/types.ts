
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
  coachId?: number | null
  isGlobal: boolean
  name: string
  category: string
  defaultSets?: number | null
  defaultReps?: number | null
  description?: string | null
  instructions?: string | null
  imageUrl?: string | null
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
  isActive: boolean
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

export type ClientProgressRecord = {
  id: number
  clientId: number
  recordedAt: string
  weightKg?: number | null
  heightCm?: number | null
  bodyFatPercentage?: number | null
  chestCm?: number | null
  waistCm?: number | null
  hipCm?: number | null
  leftArmCm?: number | null
  rightArmCm?: number | null
  leftThighCm?: number | null
  rightThighCm?: number | null
  restingHeartRate?: number | null
  notes?: string | null
  createdAt?: string
}

export type ClientProgressSummary = {
  clientId: number
  firstRecordDate?: string | null
  lastRecordDate?: string | null
  totalRecords: number
  initialWeightKg?: number | null
  currentWeightKg?: number | null
  weightChangeKg?: number | null
  initialWaistCm?: number | null
  currentWaistCm?: number | null
  waistChangeCm?: number | null
  initialBodyFatPercentage?: number | null
  currentBodyFatPercentage?: number | null
  bodyFatChangePercentage?: number | null
  daysSinceStart?: number | null
  lastUpdatedAt?: string | null
}

export type ClientProgressPayload = {
  recordedAt: string
  weightKg?: number | null
  heightCm?: number | null
  bodyFatPercentage?: number | null
  chestCm?: number | null
  waistCm?: number | null
  hipCm?: number | null
  leftArmCm?: number | null
  rightArmCm?: number | null
  leftThighCm?: number | null
  rightThighCm?: number | null
  restingHeartRate?: number | null
  notes?: string | null
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
