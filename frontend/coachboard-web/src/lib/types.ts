export type PagedResult<T> = {
    items: T[]
    total: number
    page: number
    pageSize: number
  }
  
  export type Client = {
    id: number | string
    fullName: string
    email?: string
    phone?: string
    coachId: number
    createdAt?: string
  }
  
  export type Athlete = {
    id: string
    firstName: string
    lastName: string
    email?: string
    phone?: string
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
  