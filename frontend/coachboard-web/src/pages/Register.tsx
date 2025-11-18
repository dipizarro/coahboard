import { FormEvent, useState } from 'react'
import { useNavigate, useLocation, Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import * as authApi from '../api/auth'

export default function Register() {
  const [name, setName] = useState('')
  const [specialty, setSpecialty] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const { login } = useAuth()
  const nav = useNavigate()
  const location = useLocation() as any
  const from = location.state?.from?.pathname || '/dashboard'

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)

    if (!email || !password) {
      setError('Email y password son obligatorios.')
      return
    }

    if (password !== confirmPassword) {
      setError('Las contraseñas no coinciden.')
      return
    }

    setLoading(true)
    try {
      // 1) Registrar Coach (role se fuerza a "Coach" en el api.auth)
      await authApi.register({
        email,
        password,
        name: name || undefined,
        specialty: specialty || undefined,
      })

      // 2) Reusar el flujo actual de login para guardar token + user en contexto
      await login(email, password)

      nav(from, { replace: true })
    } catch (err: any) {
      const msg = err?.response?.data ?? 'No se pudo crear la cuenta'
      setError(typeof msg === 'string' ? msg : 'No se pudo crear la cuenta')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="grid min-h-dvh place-items-center bg-gray-50 p-4">
      <form onSubmit={onSubmit} className="card w-full max-w-md space-y-4">
        <h1 className="text-2xl font-bold">Crear cuenta de Coach</h1>
        <p className="text-sm text-gray-500">
          Esta app solo permite registro de entrenadores (Coach).
        </p>

        {error && <p className="text-sm text-red-600">{error}</p>}

        <input
          className="input"
          placeholder="Nombre (opcional)"
          value={name}
          onChange={e => setName(e.target.value)}
        />

        <input
          className="input"
          placeholder="Especialidad (opcional, ej: Fuerza, Running...)"
          value={specialty}
          onChange={e => setSpecialty(e.target.value)}
        />

        <input
          className="input"
          placeholder="Email"
          type="email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          autoComplete="email"
        />

        <input
          className="input"
          placeholder="Password"
          type="password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          autoComplete="new-password"
        />

        <input
          className="input"
          placeholder="Repite el password"
          type="password"
          value={confirmPassword}
          onChange={e => setConfirmPassword(e.target.value)}
          autoComplete="new-password"
        />

        <button className="btn-primary w-full" disabled={loading}>
          {loading ? 'Creando cuenta…' : 'Crear cuenta'}
        </button>

        <p className="text-center text-sm text-gray-500">
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="text-primary-600 hover:underline">
            Inicia sesión
          </Link>
        </p>
      </form>
    </div>
  )
}
