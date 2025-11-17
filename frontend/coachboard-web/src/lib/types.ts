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
  