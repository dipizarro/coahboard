import { createContext, useCallback, useContext, useEffect, useState } from 'react'
import * as authApi from '../api/auth'
import { storage } from '../lib/storage'
import { setUnauthorizedHandler } from '../api/client'

type User = {
  email: string
  role: string
  coachId?: number | null
}

type AuthContextType = {
  user: User | null
  token: string | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType>({} as any)

const STORAGE_TOKEN_KEY = 'coachboard_token'
const STORAGE_USER_KEY = 'coachboard_user'

type JwtPayload = {
  exp?: number
}

type InitialAuthState = {
  token: string | null
  user: User | null
}

function decodeJwt(token: string): JwtPayload | null {
  try {
    const [, payload] = token.split('.')
    if (!payload) return null
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/')
    const padded = normalized.padEnd(normalized.length + (4 - (normalized.length % 4)) % 4, '=')
    const decoded =
      atob(padded)
    if (!decoded) return null
    return JSON.parse(decoded)
  } catch {
    return null
  }
}

function isTokenExpired(token: string | null): boolean {
  if (!token) return true
  const payload = decodeJwt(token)
  if (!payload?.exp) return false
  return payload.exp * 1000 < Date.now()
}

function getInitialState(): InitialAuthState {
  const savedToken = storage.get(STORAGE_TOKEN_KEY)
  const savedUserRaw = storage.get(STORAGE_USER_KEY)

  if (!savedToken || isTokenExpired(savedToken)) {
    storage.del(STORAGE_TOKEN_KEY)
    if (savedUserRaw) storage.del(STORAGE_USER_KEY)
    return { token: null, user: null }
  }

  try {
    return { token: savedToken, user: savedUserRaw ? (JSON.parse(savedUserRaw) as User) : null }
  } catch {
    storage.del(STORAGE_USER_KEY)
    return { token: savedToken, user: null }
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => getInitialState().token)
  const [user, setUser] = useState<User | null>(() => getInitialState().user)

  async function login(email: string, password: string) {
    const res = await authApi.login(email, password)
    const u: User = { email: res.email, role: res.role, coachId: res.coachId ?? null }

    setToken(res.token)
    setUser(u)

    storage.set(STORAGE_TOKEN_KEY, res.token)
    storage.set(STORAGE_USER_KEY, JSON.stringify(u))
  }

  const logout = useCallback(() => {
    storage.del(STORAGE_TOKEN_KEY)
    storage.del(STORAGE_USER_KEY)
    setToken(null)
    setUser(null)
  }, [])

  useEffect(() => {
    if (!token) return
    if (isTokenExpired(token)) {
      logout()
    }
  }, [token, logout])

  useEffect(() => {
    setUnauthorizedHandler(logout)
    return () => setUnauthorizedHandler(null)
  }, [logout])

  return (
    <AuthContext.Provider value={{ user, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuthCtx = () => useContext(AuthContext)
