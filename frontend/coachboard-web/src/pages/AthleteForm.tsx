import { FormEvent, useEffect, useState } from 'react'
import { create, get, update } from '../api/athletes'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export default function AthleteForm() {
  const { id } = useParams()
  const nav = useNavigate()
  const { coachId } = useAuth()

  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', phone: '' })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (!id) return
    setLoading(true)
    get(id)
      .then(a =>
        setForm({
          firstName: a.firstName,
          lastName: a.lastName,
          email: a.email ?? '',
          phone: a.phone ?? '',
        }),
      )
      .catch(() => setError('No se pudo cargar el atleta'))
      .finally(() => setLoading(false))
  }, [id])

  function validateForm(): boolean {
    const errors: Record<string, string> = {}
    if (!form.firstName.trim()) {
      errors.firstName = 'El nombre es requerido'
    }
    if (!form.lastName.trim()) {
      errors.lastName = 'El apellido es requerido'
    }
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      errors.email = 'El email no es válido'
    }
    if (form.phone && !/^[\d\s\-\+\(\)]+$/.test(form.phone)) {
      errors.phone = 'El teléfono contiene caracteres inválidos'
    }
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setFieldErrors({})

    if (!validateForm()) {
      return
    }

    setLoading(true)
    try {
      if (coachId == null) {
        throw new Error('Tu sesión no tiene un coachId válido. Vuelve a iniciar sesión.')
      }
      if (id) {
        await update(id, form, coachId)
      } else {
        await create(form, coachId)
      }
      nav('/athletes')
    } catch (err: any) {
      const msg = err?.response?.data ?? 'No se pudo guardar el atleta'
      setError(typeof msg === 'string' ? msg : 'No se pudo guardar el atleta')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={onSubmit} className="card space-y-3">
      <h1 className="text-2xl font-bold">{id ? 'Editar' : 'Nuevo'} atleta</h1>
      {error && <p className="text-sm text-red-600">{error}</p>}

      <div>
        <input
          className={`input ${fieldErrors.firstName ? 'border-red-500' : ''}`}
          placeholder="Nombre *"
          value={form.firstName}
          onChange={e => {
            setForm(f => ({ ...f, firstName: e.target.value }))
            if (fieldErrors.firstName) setFieldErrors(prev => ({ ...prev, firstName: '' }))
          }}
        />
        {fieldErrors.firstName && <p className="mt-1 text-xs text-red-600">{fieldErrors.firstName}</p>}
      </div>

      <div>
        <input
          className={`input ${fieldErrors.lastName ? 'border-red-500' : ''}`}
          placeholder="Apellido *"
          value={form.lastName}
          onChange={e => {
            setForm(f => ({ ...f, lastName: e.target.value }))
            if (fieldErrors.lastName) setFieldErrors(prev => ({ ...prev, lastName: '' }))
          }}
        />
        {fieldErrors.lastName && <p className="mt-1 text-xs text-red-600">{fieldErrors.lastName}</p>}
      </div>

      <div>
        <input
          className={`input ${fieldErrors.email ? 'border-red-500' : ''}`}
          placeholder="Email (opcional)"
          type="email"
          value={form.email}
          onChange={e => {
            setForm(f => ({ ...f, email: e.target.value }))
            if (fieldErrors.email) setFieldErrors(prev => ({ ...prev, email: '' }))
          }}
        />
        {fieldErrors.email && <p className="mt-1 text-xs text-red-600">{fieldErrors.email}</p>}
      </div>

      <div>
        <input
          className={`input ${fieldErrors.phone ? 'border-red-500' : ''}`}
          placeholder="Teléfono (opcional)"
          type="tel"
          value={form.phone}
          onChange={e => {
            setForm(f => ({ ...f, phone: e.target.value }))
            if (fieldErrors.phone) setFieldErrors(prev => ({ ...prev, phone: '' }))
          }}
        />
        {fieldErrors.phone && <p className="mt-1 text-xs text-red-600">{fieldErrors.phone}</p>}
      </div>

      <div className="flex gap-2">
        <button className="btn-primary" type="submit" disabled={loading}>
          {loading ? 'Guardando…' : 'Guardar'}
        </button>
        <button className="btn" type="button" onClick={() => nav(-1)}>
          Cancelar
        </button>
      </div>
    </form>
  )
}
