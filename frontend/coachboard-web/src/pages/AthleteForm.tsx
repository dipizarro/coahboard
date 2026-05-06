import { useEffect, useState, type FormEvent } from 'react'
import { create, get, update } from '../api/athletes'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

type AthleteFormState = {
  firstName: string
  lastName: string
  email: string
  phone: string
  birthDate: string
  gender: string
  initialHeightCm: string
  mainGoal: string
  experienceLevel: string
  medicalNotes: string
  injuryNotes: string
  generalNotes: string
  startDate: string
  isActive: boolean
}

const emptyForm: AthleteFormState = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  birthDate: '',
  gender: '',
  initialHeightCm: '',
  mainGoal: '',
  experienceLevel: '',
  medicalNotes: '',
  injuryNotes: '',
  generalNotes: '',
  startDate: '',
  isActive: true,
}

function toDateInput(value?: string | null) {
  return value ? value.slice(0, 10) : ''
}

function nullableText(value: string) {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

function toPayload(form: AthleteFormState) {
  return {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim(),
    email: nullableText(form.email),
    phone: nullableText(form.phone),
    birthDate: form.birthDate || null,
    gender: nullableText(form.gender),
    initialHeightCm: form.initialHeightCm ? Number(form.initialHeightCm) : null,
    mainGoal: nullableText(form.mainGoal),
    experienceLevel: nullableText(form.experienceLevel),
    medicalNotes: nullableText(form.medicalNotes),
    injuryNotes: nullableText(form.injuryNotes),
    generalNotes: nullableText(form.generalNotes),
    startDate: form.startDate || null,
    isActive: form.isActive,
  }
}

export default function AthleteForm() {
  const { id } = useParams()
  const nav = useNavigate()
  const { coachId } = useAuth()

  const [form, setForm] = useState<AthleteFormState>(emptyForm)
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
          birthDate: toDateInput(a.birthDate),
          gender: a.gender ?? '',
          initialHeightCm: a.initialHeightCm?.toString() ?? '',
          mainGoal: a.mainGoal ?? '',
          experienceLevel: a.experienceLevel ?? '',
          medicalNotes: a.medicalNotes ?? '',
          injuryNotes: a.injuryNotes ?? '',
          generalNotes: a.generalNotes ?? '',
          startDate: toDateInput(a.startDate),
          isActive: a.isActive,
        }),
      )
      .catch(() => setError('No se pudo cargar el atleta'))
      .finally(() => setLoading(false))
  }, [id])

  function setField<K extends keyof AthleteFormState>(key: K, value: AthleteFormState[K]) {
    setForm(f => ({ ...f, [key]: value }))
    if (fieldErrors[key]) setFieldErrors(prev => ({ ...prev, [key]: '' }))
  }

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
    if (form.initialHeightCm && Number(form.initialHeightCm) <= 0) {
      errors.initialHeightCm = 'La estatura debe ser mayor a 0'
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
      const payload = toPayload(form)
      if (id) {
        await update(id, payload, coachId)
      } else {
        await create(payload, coachId)
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
    <form onSubmit={onSubmit} className="card space-y-6 text-left">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold">{id ? 'Editar' : 'Nuevo'} atleta</h1>
        <label className="inline-flex items-center gap-2 text-sm text-gray-700">
          <input
            type="checkbox"
            checked={form.isActive}
            onChange={e => setField('isActive', e.target.checked)}
          />
          Activo
        </label>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      <section className="space-y-3">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Datos personales</h2>
        <div className="grid gap-3 md:grid-cols-2">
          <div>
            <input
              className={`input ${fieldErrors.firstName ? 'border-red-500' : ''}`}
              placeholder="Nombre *"
              value={form.firstName}
              onChange={e => setField('firstName', e.target.value)}
            />
            {fieldErrors.firstName && <p className="mt-1 text-xs text-red-600">{fieldErrors.firstName}</p>}
          </div>

          <div>
            <input
              className={`input ${fieldErrors.lastName ? 'border-red-500' : ''}`}
              placeholder="Apellido *"
              value={form.lastName}
              onChange={e => setField('lastName', e.target.value)}
            />
            {fieldErrors.lastName && <p className="mt-1 text-xs text-red-600">{fieldErrors.lastName}</p>}
          </div>

          <div>
            <input
              className={`input ${fieldErrors.email ? 'border-red-500' : ''}`}
              placeholder="Email"
              type="email"
              value={form.email}
              onChange={e => setField('email', e.target.value)}
            />
            {fieldErrors.email && <p className="mt-1 text-xs text-red-600">{fieldErrors.email}</p>}
          </div>

          <div>
            <input
              className={`input ${fieldErrors.phone ? 'border-red-500' : ''}`}
              placeholder="Teléfono"
              type="tel"
              value={form.phone}
              onChange={e => setField('phone', e.target.value)}
            />
            {fieldErrors.phone && <p className="mt-1 text-xs text-red-600">{fieldErrors.phone}</p>}
          </div>

          <label className="space-y-1 text-sm text-gray-600">
            <span>Fecha de nacimiento</span>
            <input
              className="input"
              type="date"
              value={form.birthDate}
              onChange={e => setField('birthDate', e.target.value)}
            />
          </label>

          <label className="space-y-1 text-sm text-gray-600">
            <span>Género</span>
            <select className="input" value={form.gender} onChange={e => setField('gender', e.target.value)}>
              <option value="">Sin especificar</option>
              <option value="Femenino">Femenino</option>
              <option value="Masculino">Masculino</option>
              <option value="No binario">No binario</option>
              <option value="Otro">Otro</option>
            </select>
          </label>

          <label className="space-y-1 text-sm text-gray-600">
            <span>Estatura inicial (cm)</span>
            <input
              className={`input ${fieldErrors.initialHeightCm ? 'border-red-500' : ''}`}
              type="number"
              min="0"
              step="0.01"
              value={form.initialHeightCm}
              onChange={e => setField('initialHeightCm', e.target.value)}
            />
            {fieldErrors.initialHeightCm && <p className="mt-1 text-xs text-red-600">{fieldErrors.initialHeightCm}</p>}
          </label>

          <label className="space-y-1 text-sm text-gray-600">
            <span>Fecha de inicio</span>
            <input
              className="input"
              type="date"
              value={form.startDate}
              onChange={e => setField('startDate', e.target.value)}
            />
          </label>
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Objetivo deportivo</h2>
        <div className="grid gap-3 md:grid-cols-2">
          <input
            className="input"
            placeholder="Objetivo principal"
            value={form.mainGoal}
            onChange={e => setField('mainGoal', e.target.value)}
          />
          <select
            className="input"
            value={form.experienceLevel}
            onChange={e => setField('experienceLevel', e.target.value)}
          >
            <option value="">Nivel de experiencia</option>
            <option value="Principiante">Principiante</option>
            <option value="Intermedio">Intermedio</option>
            <option value="Avanzado">Avanzado</option>
            <option value="Competitivo">Competitivo</option>
          </select>
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Salud y restricciones</h2>
        <div className="grid gap-3 md:grid-cols-2">
          <textarea
            className="input min-h-28"
            placeholder="Notas médicas"
            value={form.medicalNotes}
            onChange={e => setField('medicalNotes', e.target.value)}
          />
          <textarea
            className="input min-h-28"
            placeholder="Lesiones o restricciones"
            value={form.injuryNotes}
            onChange={e => setField('injuryNotes', e.target.value)}
          />
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Notas internas</h2>
        <textarea
          className="input min-h-32"
          placeholder="Notas generales"
          value={form.generalNotes}
          onChange={e => setField('generalNotes', e.target.value)}
        />
      </section>

      <div className="flex flex-wrap gap-2 border-t pt-4">
        <button className="btn-primary" type="submit" disabled={loading}>
          {loading ? 'Guardando...' : 'Guardar'}
        </button>
        <button className="btn" type="button" onClick={() => nav(-1)}>
          Cancelar
        </button>
      </div>
    </form>
  )
}
