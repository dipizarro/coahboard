import { api } from './client'

export type LoginResponse = {
  token: string
  email: string
  role: 'Admin' | 'Coach' | 'User' | string
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const { data } = await api.post('/api/Auth/login', { email, password })
  return data
}

// Si más adelante agregas /auth/me, lo reponemos.
// export async function me() { ... }
