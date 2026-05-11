import { useEffect, useState, type FormEvent, type ReactNode } from 'react'
import { create, get, update } from '../api/exercises'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import ExerciseMediaUploader from '../components/ExerciseMediaUploader'

const CATEGORIES = ['Fuerza', 'Cardio', 'Movilidad', 'Flexibilidad', 'General']
const DIFFICULTIES = ['Inicial', 'Intermedio', 'Avanzado']
const EQUIPMENT = ['Peso corporal', 'Mancuernas', 'Barra', 'Kettlebell', 'Máquina', 'Banda elástica', 'TRX']
const MUSCLE_GROUPS = ['Pectoral', 'Espalda', 'Hombros', 'Brazos', 'Core', 'Piernas', 'Glúteos']
const ENVIRONMENTS = ['Gimnasio', 'Casa', 'Exterior']
const MOVEMENT_PATTERNS = ['Squat', 'Hinge', 'Push', 'Pull', 'Lunge', 'Carry', 'Rotation', 'Gait']
const EXERCISE_TYPES = ['Fuerza', 'Cardio', 'Movilidad', 'Flexibilidad', 'Técnica', 'Rehabilitación']

type FormState = {
  name: string
  category: string
  description: string
  instructions: string
  imageUrl: string
  videoUrl: string
  referenceUrl: string
  difficultyLevel: string
  movementPattern: string
  equipment: string
  targetMuscleGroup: string
  secondaryMuscleGroups: string
  exerciseType: string
  environment: string
  tags: string
  defaultSets: string
  defaultReps: string
  isActive: boolean
  isGlobal: boolean
  coachId: number | null
}

const initialForm: FormState = {
  name: '',
  category: 'General',
  description: '',
  instructions: '',
  imageUrl: '',
  videoUrl: '',
  referenceUrl: '',
  difficultyLevel: '',
  movementPattern: '',
  equipment: '',
  targetMuscleGroup: '',
  secondaryMuscleGroups: '',
  exerciseType: '',
  environment: '',
  tags: '',
  defaultSets: '',
  defaultReps: '',
  isActive: true,
  isGlobal: false,
  coachId: null,
}

function Icon({ children }: { children: ReactNode }) {
  return (
    <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-50 text-primary-700">
      <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8">
        {children}
      </svg>
    </span>
  )
}

function Section({
  title,
  eyebrow,
  icon,
  children,
}: {
  title: string
  eyebrow: string
  icon: ReactNode
  children: ReactNode
}) {
  return (
    <section className="card space-y-4">
      <div className="flex items-start gap-3">
        <Icon>{icon}</Icon>
        <div>
          <p className="text-sm text-gray-500">{eyebrow}</p>
          <h2 className="text-lg font-semibold text-gray-900">{title}</h2>
        </div>
      </div>
      {children}
    </section>
  )
}

function Field({
  label,
  error,
  children,
}: {
  label: string
  error?: string
  children: ReactNode
}) {
  return (
    <div className="space-y-1 text-sm text-gray-600">
      <span className="font-medium text-gray-700">{label}</span>
      {children}
      {error && <p className="text-xs text-red-600">{error}</p>}
    </div>
  )
}

function ChipGroup({
  value,
  options,
  onChange,
}: {
  value: string
  options: string[]
  onChange: (value: string) => void
}) {
  return (
    <div className="flex flex-wrap gap-2">
      {options.map(option => {
        const selected = value === option
        return (
          <button
            key={option}
            type="button"
            className={`rounded-lg border px-3 py-2 text-sm transition ${
              selected
                ? 'border-primary-500 bg-primary-50 text-primary-700'
                : 'border-gray-200 bg-white text-gray-700 hover:border-primary-200 hover:bg-gray-50'
            }`}
            onClick={() => onChange(selected ? '' : option)}
          >
            {option}
          </button>
        )
      })}
    </div>
  )
}

function textOrNull(value: string) {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

function isValidUrl(value: string) {
  if (!value.trim()) return true
  try {
    const url = new URL(value)
    return url.protocol === 'http:' || url.protocol === 'https:'
  } catch {
    return false
  }
}

export default function ExerciseForm() {
  const { id } = useParams()
  const nav = useNavigate()
  const { role } = useAuth()
  const canEdit = role === 'Admin' || role === 'Coach'

  const [form, setForm] = useState<FormState>(initialForm)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const canManageCurrent = role === 'Admin' || !id || (role === 'Coach' && !form.isGlobal)

  useEffect(() => {
    if (!id) return
    setLoading(true)
    get(Number(id))
      .then(ex => {
        setForm({
          name: ex.name,
          category: ex.category,
          description: ex.description ?? '',
          instructions: ex.instructions ?? '',
          imageUrl: ex.imageUrl ?? '',
          videoUrl: ex.videoUrl ?? '',
          referenceUrl: ex.referenceUrl ?? '',
          difficultyLevel: ex.difficultyLevel ?? '',
          movementPattern: ex.movementPattern ?? '',
          equipment: ex.equipment ?? '',
          targetMuscleGroup: ex.targetMuscleGroup ?? '',
          secondaryMuscleGroups: ex.secondaryMuscleGroups ?? '',
          exerciseType: ex.exerciseType ?? '',
          environment: ex.environment ?? '',
          tags: ex.tags ?? '',
          defaultSets: ex.defaultSets?.toString() ?? '',
          defaultReps: ex.defaultReps?.toString() ?? '',
          isActive: ex.isActive,
          isGlobal: ex.isGlobal,
          coachId: ex.coachId ?? null,
        })
      })
      .catch(() => setError('No se pudo cargar el ejercicio'))
      .finally(() => setLoading(false))
  }, [id])

  function updateField<K extends keyof FormState>(key: K, value: FormState[K]) {
    setForm(current => ({ ...current, [key]: value }))
    if (fieldErrors[key]) setFieldErrors(prev => ({ ...prev, [key]: '' }))
  }

  function validateForm(): boolean {
    const errors: Record<string, string> = {}
    if (!form.name.trim()) {
      errors.name = 'El nombre es requerido'
    }
    if (!form.category) {
      errors.category = 'La categoría es requerida'
    }
    if (form.defaultSets && (Number.isNaN(Number(form.defaultSets)) || Number(form.defaultSets) < 1)) {
      errors.defaultSets = 'Las series deben ser un número mayor a 0'
    }
    if (form.defaultReps && (Number.isNaN(Number(form.defaultReps)) || Number(form.defaultReps) < 1)) {
      errors.defaultReps = 'Las repeticiones deben ser un número mayor a 0'
    }
    if (!isValidUrl(form.videoUrl)) {
      errors.videoUrl = 'Ingresa una URL válida con http o https'
    }
    if (!isValidUrl(form.imageUrl)) {
      errors.imageUrl = 'Ingresa una URL válida con http o https'
    }
    if (!isValidUrl(form.referenceUrl)) {
      errors.referenceUrl = 'Ingresa una URL válida con http o https'
    }
    setFieldErrors(errors)
    return Object.keys(errors).length === 0
  }

  function buildPayload() {
    return {
      name: form.name.trim(),
      category: form.category,
      defaultSets: form.defaultSets ? Number(form.defaultSets) : null,
      defaultReps: form.defaultReps ? Number(form.defaultReps) : null,
      description: textOrNull(form.description),
      instructions: textOrNull(form.instructions),
      imageUrl: textOrNull(form.imageUrl),
      videoUrl: textOrNull(form.videoUrl),
      referenceUrl: textOrNull(form.referenceUrl),
      difficultyLevel: textOrNull(form.difficultyLevel),
      movementPattern: textOrNull(form.movementPattern),
      equipment: textOrNull(form.equipment),
      targetMuscleGroup: textOrNull(form.targetMuscleGroup),
      secondaryMuscleGroups: textOrNull(form.secondaryMuscleGroups),
      exerciseType: textOrNull(form.exerciseType),
      environment: textOrNull(form.environment),
      tags: textOrNull(form.tags),
      isActive: form.isActive,
      isGlobal: form.isGlobal,
      coachId: form.coachId,
    }
  }

  async function handleCreateOwnCopy() {
    setError(null)
    setFieldErrors({})

    if (!validateForm()) {
      return
    }

    setLoading(true)
    try {
      const created = await create({
        ...buildPayload(),
        isGlobal: false,
        coachId: null,
      })
      nav(`/exercises/${created.id}`)
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: unknown } }
      const msg = axiosError?.response?.data ?? 'No se pudo crear una copia propia del ejercicio'
      setError(typeof msg === 'string' ? msg : 'No se pudo crear una copia propia del ejercicio')
    } finally {
      setLoading(false)
    }
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setFieldErrors({})

    if (!canManageCurrent) {
      setError('Este ejercicio pertenece al sistema. Crea una copia propia para modificarlo.')
      return
    }

    if (!validateForm()) {
      return
    }

    setLoading(true)
    try {
      const payload = buildPayload()
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

  if (!canEdit) {
    return (
      <div className="card">
        <p className="text-red-600">No tienes permisos para crear o editar ejercicios.</p>
      </div>
    )
  }

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-sm text-gray-500">Biblioteca de ejercicios</p>
          <h1 className="text-2xl font-bold">{id ? 'Editar ejercicio' : 'Nuevo ejercicio'}</h1>
        </div>
        <span
          className={`w-fit rounded-lg px-3 py-2 text-sm font-medium ${
            form.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
          }`}
        >
          {form.isActive ? 'Activo' : 'Inactivo'}
        </span>
      </div>

      {error && <p className="rounded-lg border border-red-100 bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      {!canManageCurrent && (
        <div className="rounded-lg border border-blue-100 bg-blue-50 p-4 text-sm text-blue-800">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p>Este ejercicio es del sistema. Puedes revisarlo, pero para editarlo debes crear una copia propia.</p>
            <button className="btn-primary w-fit" type="button" disabled={loading} onClick={handleCreateOwnCopy}>
              Crear copia propia
            </button>
          </div>
        </div>
      )}

      <form onSubmit={onSubmit} className="space-y-5">
        <fieldset disabled={!canManageCurrent || loading} className="space-y-5 disabled:opacity-70">
          <Section
            eyebrow="Base"
            title="Información principal"
            icon={<><path d="M4 7h16" /><path d="M4 12h10" /><path d="M4 17h16" /></>}
          >
            <div className="grid gap-4 md:grid-cols-2">
              <Field label="Nombre *" error={fieldErrors.name}>
                <input
                  className={`input ${fieldErrors.name ? 'border-red-500' : ''}`}
                  value={form.name}
                  onChange={e => updateField('name', e.target.value)}
                />
              </Field>
              <Field label="Categoría *" error={fieldErrors.category}>
                <select
                  className={`input ${fieldErrors.category ? 'border-red-500' : ''}`}
                  value={form.category}
                  onChange={e => updateField('category', e.target.value)}
                >
                  {CATEGORIES.map(cat => (
                    <option key={cat} value={cat}>
                      {cat}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Descripción">
                <textarea
                  className="input min-h-28 md:col-span-2"
                  value={form.description}
                  onChange={e => updateField('description', e.target.value)}
                />
              </Field>
              <label className="flex items-center gap-3 rounded-lg border border-gray-100 bg-gray-50 p-3 text-sm text-gray-700">
                <input
                  type="checkbox"
                  checked={form.isActive}
                  onChange={e => updateField('isActive', e.target.checked)}
                />
                Ejercicio activo en el catálogo
              </label>
            </div>
          </Section>

          <Section
            eyebrow="Taxonomía"
            title="Clasificación del ejercicio"
            icon={<><path d="M12 3v18" /><path d="M3 12h18" /><path d="m6 6 12 12" /><path d="m18 6-12 12" /></>}
          >
            <div className="space-y-5">
              <div className="grid gap-4 lg:grid-cols-2">
                <Field label="Dificultad">
                  <ChipGroup value={form.difficultyLevel} options={DIFFICULTIES} onChange={value => updateField('difficultyLevel', value)} />
                </Field>
                <Field label="Patrón de movimiento">
                  <ChipGroup value={form.movementPattern} options={MOVEMENT_PATTERNS} onChange={value => updateField('movementPattern', value)} />
                </Field>
                <Field label="Equipamiento">
                  <ChipGroup value={form.equipment} options={EQUIPMENT} onChange={value => updateField('equipment', value)} />
                </Field>
                <Field label="Músculo principal">
                  <ChipGroup value={form.targetMuscleGroup} options={MUSCLE_GROUPS} onChange={value => updateField('targetMuscleGroup', value)} />
                </Field>
                <Field label="Entorno">
                  <ChipGroup value={form.environment} options={ENVIRONMENTS} onChange={value => updateField('environment', value)} />
                </Field>
                <Field label="Tipo de ejercicio">
                  <select className="input" value={form.exerciseType} onChange={e => updateField('exerciseType', e.target.value)}>
                    <option value="">Seleccionar tipo</option>
                    {EXERCISE_TYPES.map(type => (
                      <option key={type} value={type}>
                        {type}
                      </option>
                    ))}
                  </select>
                </Field>
              </div>
              <Field label="Grupos musculares secundarios">
                <input
                  className="input"
                  placeholder="Glúteos, core, aductores..."
                  value={form.secondaryMuscleGroups}
                  onChange={e => updateField('secondaryMuscleGroups', e.target.value)}
                />
              </Field>
            </div>
          </Section>

          <Section
            eyebrow="Programación"
            title="Prescripción sugerida"
            icon={<><rect x="4" y="5" width="16" height="14" rx="2" /><path d="M8 3v4" /><path d="M16 3v4" /><path d="M8 12h8" /></>}
          >
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Series por defecto" error={fieldErrors.defaultSets}>
                <input
                  className={`input ${fieldErrors.defaultSets ? 'border-red-500' : ''}`}
                  type="number"
                  min="1"
                  value={form.defaultSets}
                  onChange={e => updateField('defaultSets', e.target.value)}
                />
              </Field>
              <Field label="Repeticiones por defecto" error={fieldErrors.defaultReps}>
                <input
                  className={`input ${fieldErrors.defaultReps ? 'border-red-500' : ''}`}
                  type="number"
                  min="1"
                  value={form.defaultReps}
                  onChange={e => updateField('defaultReps', e.target.value)}
                />
              </Field>
            </div>
          </Section>

          <Section
            eyebrow="Contenido técnico"
            title="Referencias y explicación"
            icon={<><path d="M10 13a5 5 0 0 0 7 0l2-2a5 5 0 0 0-7-7l-1 1" /><path d="M14 11a5 5 0 0 0-7 0l-2 2a5 5 0 0 0 7 7l1-1" /></>}
          >
            <div className="grid gap-4 md:grid-cols-2">
              <Field label="Imagen URL" error={fieldErrors.imageUrl}>
                <input
                  className={`input ${fieldErrors.imageUrl ? 'border-red-500' : ''}`}
                  value={form.imageUrl}
                  onChange={e => updateField('imageUrl', e.target.value)}
                  placeholder="https://..."
                />
              </Field>
              <Field label="Video URL" error={fieldErrors.videoUrl}>
                <input
                  className={`input ${fieldErrors.videoUrl ? 'border-red-500' : ''}`}
                  value={form.videoUrl}
                  onChange={e => updateField('videoUrl', e.target.value)}
                  placeholder="https://..."
                />
              </Field>
              <Field label="Referencia URL" error={fieldErrors.referenceUrl}>
                <input
                  className={`input ${fieldErrors.referenceUrl ? 'border-red-500' : ''}`}
                  value={form.referenceUrl}
                  onChange={e => updateField('referenceUrl', e.target.value)}
                  placeholder="https://..."
                />
              </Field>
              <div className="md:col-span-2">
                <Field label="Instrucciones">
                  <textarea
                    className="input min-h-36"
                    value={form.instructions}
                    onChange={e => updateField('instructions', e.target.value)}
                    placeholder="Describe ejecución, ritmo, respiración y puntos de control."
                  />
                </Field>
              </div>
            </div>
          </Section>

          <Section
            eyebrow="Organización"
            title="Tags"
            icon={<><path d="M20 12v7a2 2 0 0 1-2 2h-7L4 14V5a2 2 0 0 1 2-2h8z" /><path d="M8 8h.01" /></>}
          >
            <Field label="Etiquetas">
              <input
                className="input"
                value={form.tags}
                onChange={e => updateField('tags', e.target.value)}
                placeholder="fuerza, hipertrofia, empuje..."
              />
            </Field>
          </Section>
        </fieldset>

        <div className="flex flex-wrap gap-2">
          <button className="btn-primary" type="submit" disabled={loading || !canManageCurrent}>
            {loading ? 'Guardando...' : 'Guardar'}
          </button>
          <button className="btn" type="button" onClick={() => nav(-1)}>
            Cancelar
          </button>
        </div>
      </form>

      <ExerciseMediaUploader
        exerciseId={id ? Number(id) : null}
        canManage={canManageCurrent}
      />
    </div>
  )
}
