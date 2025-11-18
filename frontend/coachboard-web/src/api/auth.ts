import { api } from './client'

export type AuthResponse = {
  token: string
  email: string
  role: 'Admin' | 'Coach' | 'User' | string
  coachId?: number | null
}

// Login normal
export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/api/Auth/login', { email, password })
  return data
}

// Payload para registro de Coach
export type RegisterPayload = {
  email: string
  password: string
  name?: string
  specialty?: string
}

// Registro SIEMPRE como Coach (no permitimos Admin desde la app)
export async function register(payload: RegisterPayload): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/api/Auth/register', {
    ...payload,
    role: 'Coach', // <- hardcodeado
  })
  return data
}
