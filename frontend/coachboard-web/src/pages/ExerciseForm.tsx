import { useEffect, useState, type FormEvent } from 'react'
import { create, get, update } from '../api/exercises'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

const CATEGORIES = ['Fuerza', 'Cardio', 'Movilidad', 'Flexibilidad', 'General']

export default function ExerciseForm() {
  const { id } = useParams()
  const nav = useNavigate()
  const { role } = useAuth()
  const canEdit = role === 'Admin' || role === 'Coach'

  if (!canEdit) {
    return (
      <div className="card">
        <p className="text-red-600">No tienes permisos para crear o editar ejercicios.</p>
      </div>
    )
  }

  const [form, setForm] = useState({
    name: '',
    category: 'General',
    defaultSets: '',
    defaultReps: '',
  })
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (!id) return
    setLoading(true)
    get(Number(id))
      .then(ex => {
        setForm({
          name: ex.name,
          category: ex.category,
          defaultSets: ex.defaultSets?.toString() ?? '',
          defaultReps: ex.defaultReps?.toString() ?? '',
        })
      })
      .catch(() => setError('No se pudo cargar el ejercicio'))
      .finally(() => setLoading(false))
  }, [id])

  function validateForm(): boolean {
    const errors: Record<string, string> = {}
    if (!form.name.trim()) {
      errors.name = 'El nombre es requerido'
    }
    if (!form.category) {
      errors.category = 'La categoría es requerida'
    }
    if (form.defaultSets && (isNaN(Number(form.defaultSets)) || Number(form.defaultSets) < 1)) {
      errors.defaultSets = 'Las series deben ser un número mayor a 0'
    }
    if (form.defaultReps && (isNaN(Number(form.defaultReps)) || Number(form.defaultReps) < 1)) {
      errors.defaultReps = 'Las repeticiones deben ser un número mayor a 0'
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
      const payload = {
        name: form.name.trim(),
        category: form.category,
        defaultSets: form.defaultSets ? Number(form.defaultSets) : null,
        defaultReps: form.defaultReps ? Number(form.defaultReps) : null,
      }
      if (id) {
        await update(Number(id), payload)
      } else {
        await create(payload)
      }
      nav('/exercises')
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: unknown } }
      const msg = axiosError?.response?.data ?? 'No se pudo guardar el ejercicio'
      setError(typeof msg === 'string' ? msg : 'No se pudo guardar el ejercicio')
    } finally {
      setLoading(false)
    }
  }

  return (
    <form onSubmit={onSubmit} className="card space-y-3">
      <h1 className="text-2xl font-bold">{id ? 'Editar' : 'Nuevo'} ejercicio</h1>
      {error && <p className="text-sm text-red-600">{error}</p>}

      <div>
        <input
          className={`input ${fieldErrors.name ? 'border-red-500' : ''}`}
          placeholder="Nombre *"
          value={form.name}
          onChange={e => {
            setForm(f => ({ ...f, name: e.target.value }))
            if (fieldErrors.name) setFieldErrors(prev => ({ ...prev, name: '' }))
          }}
        />
        {fieldErrors.name && <p className="mt-1 text-xs text-red-600">{fieldErrors.name}</p>}
      </div>

      <div>
        <select
          className={`input ${fieldErrors.category ? 'border-red-500' : ''}`}
          value={form.category}
          onChange={e => {
            setForm(f => ({ ...f, category: e.target.value }))
            if (fieldErrors.category) setFieldErrors(prev => ({ ...prev, category: '' }))
          }}
        >
          {CATEGORIES.map(cat => (
            <option key={cat} value={cat}>
              {cat}
            </option>
          ))}
        </select>
        {fieldErrors.category && <p className="mt-1 text-xs text-red-600">{fieldErrors.category}</p>}
      </div>

      <div>
        <input
          className={`input ${fieldErrors.defaultSets ? 'border-red-500' : ''}`}
          placeholder="Series por defecto (opcional)"
          type="number"
          min="1"
          value={form.defaultSets}
          onChange={e => {
            setForm(f => ({ ...f, defaultSets: e.target.value }))
            if (fieldErrors.defaultSets) setFieldErrors(prev => ({ ...prev, defaultSets: '' }))
          }}
        />
        {fieldErrors.defaultSets && (
          <p className="mt-1 text-xs text-red-600">{fieldErrors.defaultSets}</p>
        )}
      </div>

      <div>
        <input
          className={`input ${fieldErrors.defaultReps ? 'border-red-500' : ''}`}
          placeholder="Repeticiones por defecto (opcional)"
          type="number"
          min="1"
          value={form.defaultReps}
          onChange={e => {
            setForm(f => ({ ...f, defaultReps: e.target.value }))
            if (fieldErrors.defaultReps) setFieldErrors(prev => ({ ...prev, defaultReps: '' }))
          }}
        />
        {fieldErrors.defaultReps && <p className="mt-1 text-xs text-red-600">{fieldErrors.defaultReps}</p>}
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

