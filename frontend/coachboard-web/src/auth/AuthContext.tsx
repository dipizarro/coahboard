import { createContext, useContext, useState } from 'react'
import * as authApi from '../api/auth'
import { storage } from '../lib/storage'

type User = { email: string; role?: string }

type AuthContextType = {
  user: User | null
  token: string | null
  login: (email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType>({} as any)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [token, setToken] = useState<string | null>(storage.get('token'))

  async function login(email: string, password: string) {
    const res = await authApi.login(email, password) // { token, email, role }
    storage.set('token', res.token)
    setToken(res.token)
    setUser({ email: res.email, role: res.role })
  }

  function logout() {
    storage.del('token')
    setToken(null)
    setUser(null)
  }

  return (
    <AuthContext.Provider value={{ user, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuthCtx = () => useContext(AuthContext)
