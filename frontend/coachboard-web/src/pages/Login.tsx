import { FormEvent, useState } from 'react'
import { useAuth } from '../auth/useAuth'
import { useLocation, useNavigate } from 'react-router-dom'

export default function Login() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { login } = useAuth()
  const nav = useNavigate()
  const location = useLocation() as any
  const from = location.state?.from?.pathname || '/dashboard'

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)
    try {
      await login(email, password)
      nav(from, { replace: true })
    } catch (err: any) {
      const msg = err?.response?.data ?? 'Error al iniciar sesión'
      setError(typeof msg === 'string' ? msg : 'Error al iniciar sesión')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="grid min-h-dvh place-items-center bg-gray-50 p-4">
      <form onSubmit={onSubmit} className="card w-full max-w-md space-y-4">
        <h1 className="text-2xl font-bold">Iniciar sesión</h1>
        {error && <p className="text-sm text-red-600">{error}</p>}
  
        <input
          className="input"
          placeholder="Email"
          value={email}
          onChange={e => setEmail(e.target.value)}
        />
        <input
          className="input"
          placeholder="Password"
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
        />
  
        <button className="btn-primary w-full" disabled={loading}>
          {loading ? 'Ingresando…' : 'Entrar'}
        </button>
  
        <p className="text-center text-sm text-gray-500">
          ¿Eres entrenador y aún no tienes cuenta?{' '}
          <a href="/register" className="text-primary-600 hover:underline">
            Regístrate aquí
          </a>
        </p>
      </form>
    </div>
  )
  
}
