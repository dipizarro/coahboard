import { useState } from 'react'
import { useAuthCtx } from '../auth/AuthContext'
import { useLocation, useNavigate } from 'react-router-dom'


export default function Login() {
const [email, setEmail] = useState('')
const [password, setPassword] = useState('')
const [loading, setLoading] = useState(false)
const [error, setError] = useState<string | null>(null)
const { login } = useAuthCtx()
const nav = useNavigate()
const location = useLocation() as any
const from = location.state?.from?.pathname || '/dashboard'


async function onSubmit(e: React.FormEvent) {
e.preventDefault()
setLoading(true)
setError(null)
try {
await login(email, password)
nav(from, { replace: true })
} catch (err: any) {
setError(err?.response?.data?.message || 'Credenciales inválidas')
} finally {
setLoading(false)
}
}


return (
<div className="grid min-h-dvh place-items-center bg-gray-50 p-4">
<form onSubmit={onSubmit} className="card w-full max-w-md space-y-4">
<h1 className="text-2xl font-bold">Iniciar sesión</h1>
{error && <p className="text-sm text-red-600">{error}</p>}
<input className="input" placeholder="Email" value={email} onChange={e=>setEmail(e.target.value)} />
<input className="input" placeholder="Password" type="password" value={password} onChange={e=>setPassword(e.target.value)} />
<button className="btn-primary w-full" disabled={loading}>{loading ? 'Ingresando…' : 'Entrar'}</button>
</form>
</div>
)
}