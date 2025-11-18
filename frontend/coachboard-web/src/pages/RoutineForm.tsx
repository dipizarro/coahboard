import { FormEvent, useEffect, useState } from 'react'
import { create, get, update } from '../api/routines'
import { search as searchExercises } from '../api/exercises'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import type { Exercise, RoutineItem } from '../lib/types'
import Loader from '../components/Loader'

export default function RoutineForm() {
  const { athleteId, routineId } = useParams<{ athleteId: string; routineId: string }>()
  const nav = useNavigate()
  const { role } = useAuth()
  const canEdit = role === 'Admin' || role === 'Coach'

  if (!canEdit) {
    return (
      <div className="card">
        <p className="text-red-600">No tienes permisos para crear o editar rutinas.</p>
      </div>
    )
  }

  const [title, setTitle] = useState('')
  const [items, setItems] = useState<RoutineItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const [availableExercises, setAvailableExercises] = useState<Exercise[]>([])
  const [searchExerciseQuery, setSearchExerciseQuery] = useState('')
  const [loadingExercises, setLoadingExercises] = useState(false)

  useEffect(() => {
    if (!athleteId) {
      setError('ID de atleta no proporcionado')
      return
    }

    if (routineId) {
      setLoading(true)
      get(Number(routineId))
        .then(routine => {
          setTitle(routine.title)
          setItems(
            routine.items.map(item => ({
              exerciseId: item.exerciseId,
              exerciseName: item.exerciseName,
              category: item.category,
              sets: item.sets,
              reps: item.reps,
              order: item.order,
              notes: item.notes ?? '',
            })),
          )
        })
        .catch(() => setError('No se pudo cargar la rutina'))
        .finally(() => setLoading(false))
    }

    // Cargar ejercicios disponibles
    loadExercises()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [athleteId, routineId])

  const loadExercises = async (q = '') => {
    setLoadingExercises(true)
    try {
      const result = await searchExercises({ page: 1, pageSize: 50, q })
      setAvailableExercises(result.items)
    } catch {
      // Ignorar errores de carga de ejercicios
    } finally {
      setLoadingExercises(false)
    }
  }

  const handleSearchExercises = (e?: React.FormEvent | React.MouseEvent) => {
    if (e) {
      e.preventDefault()
    }
    loadExercises(searchExerciseQuery)
  }

  const addExercise = (exercise: Exercise) => {
    const newItem: RoutineItem = {
      exerciseId: exercise.id,
      exerciseName: exercise.name,
      category: exercise.category,
      sets: exercise.defaultSets ?? 3,
      reps: exercise.defaultReps ?? 10,
      order: items.length + 1,
      notes: '',
    }
    setItems([...items, newItem])
    setSearchExerciseQuery('')
  }

  const removeItem = (index: number) => {
    const newItems = items.filter((_, i) => i !== index).map((item, i) => ({
      ...item,
      order: i + 1,
    }))
    setItems(newItems)
  }

  const updateItem = (index: number, updates: Partial<RoutineItem>) => {
    const newItems = [...items]
    newItems[index] = { ...newItems[index], ...updates }
    setItems(newItems)
  }

  const moveItem = (index: number, direction: 'up' | 'down') => {
    const newItems = [...items]
    const targetIndex = direction === 'up' ? index - 1 : index + 1
    if (targetIndex < 0 || targetIndex >= newItems.length) return

    ;[newItems[index], newItems[targetIndex]] = [newItems[targetIndex], newItems[index]]
    newItems.forEach((item, i) => {
      item.order = i + 1
    })
    setItems(newItems)
  }

  function validateForm(): boolean {
    const errors: Record<string, string> = {}
    if (!title.trim()) {
      errors.title = 'El título es requerido'
    }
    if (items.length === 0) {
      errors.items = 'Debe agregar al menos un ejercicio'
    }
    items.forEach((item, index) => {
      if (item.sets < 1) {
        errors[`item_${index}_sets`] = 'Las series deben ser mayor a 0'
      }
      if (item.reps < 1) {
        errors[`item_${index}_reps`] = 'Las repeticiones deben ser mayor a 0'
      }
    })
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

    if (!athleteId) {
      setError('ID de atleta no proporcionado')
      return
    }

    setLoading(true)
    try {
      const payload = {
        title: title.trim(),
        items: items.map(item => ({
          exerciseId: item.exerciseId,
          sets: item.sets,
          reps: item.reps,
          order: item.order,
          notes: item.notes || null,
        })),
      }
      if (routineId) {
        await update(Number(routineId), payload)
      } else {
        await create({ ...payload, clientId: Number(athleteId) })
      }
      nav(`/athletes/${athleteId}/routines`)
    } catch (err: unknown) {
      const axiosError = err as { response?: { data?: unknown } }
      const msg = axiosError?.response?.data ?? 'No se pudo guardar la rutina'
      setError(typeof msg === 'string' ? msg : 'No se pudo guardar la rutina')
    } finally {
      setLoading(false)
    }
  }

  if (!athleteId) {
    return (
      <div className="card">
        <p className="text-red-600">ID de atleta no proporcionado.</p>
      </div>
    )
  }

  const filteredExercises = availableExercises.filter(
    ex => !items.some(item => item.exerciseId === ex.id),
  )

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      <div className="card space-y-3">
        <div>
          <input
            className={`input w-full ${fieldErrors.title ? 'border-red-500' : ''}`}
            placeholder="Título de la rutina *"
            value={title}
            onChange={e => {
              setTitle(e.target.value)
              if (fieldErrors.title) setFieldErrors(prev => ({ ...prev, title: '' }))
            }}
          />
          {fieldErrors.title && <p className="mt-1 text-xs text-red-600">{fieldErrors.title}</p>}
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
        {fieldErrors.items && <p className="text-sm text-red-600">{fieldErrors.items}</p>}
      </div>

      <div className="card space-y-4">
        <h2 className="text-lg font-semibold">Agregar ejercicio</h2>
        <div className="flex gap-2">
          <input
            type="text"
            className="input flex-1"
            placeholder="Buscar ejercicio..."
            value={searchExerciseQuery}
            onChange={e => setSearchExerciseQuery(e.target.value)}
            onKeyDown={e => {
              if (e.key === 'Enter') {
                e.preventDefault()
                handleSearchExercises(e as any)
              }
            }}
          />
          <button
            type="button"
            className="btn-primary"
            disabled={loadingExercises}
            onClick={handleSearchExercises}
          >
            {loadingExercises ? 'Buscando...' : 'Buscar'}
          </button>
        </div>

        {filteredExercises.length > 0 && (
          <div className="max-h-48 space-y-2 overflow-y-auto rounded-lg border p-2">
            {filteredExercises.map(ex => (
              <button
                key={ex.id}
                type="button"
                className="flex w-full items-center justify-between rounded-lg border p-2 text-left hover:bg-gray-50"
                onClick={() => addExercise(ex)}
              >
                <div>
                  <p className="font-medium">{ex.name}</p>
                  <p className="text-xs text-gray-500">{ex.category}</p>
                </div>
                <span className="text-sm text-primary-600">+ Agregar</span>
              </button>
            ))}
          </div>
        )}
      </div>

      <div className="card space-y-4">
        <h2 className="text-lg font-semibold">Ejercicios de la rutina ({items.length})</h2>

        {items.length === 0 ? (
          <p className="text-sm text-gray-500">No hay ejercicios agregados. Busca y agrega ejercicios arriba.</p>
        ) : (
          <div className="space-y-3">
            {items.map((item, index) => (
              <div key={index} className="rounded-lg border p-4">
                <div className="mb-3 flex items-center justify-between">
                  <div>
                    <p className="font-medium">{item.exerciseName || `Ejercicio #${index + 1}`}</p>
                    {item.category && (
                      <p className="text-xs text-gray-500">{item.category}</p>
                    )}
                  </div>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      className="btn text-xs"
                      disabled={index === 0}
                      onClick={() => moveItem(index, 'up')}
                    >
                      ↑
                    </button>
                    <button
                      type="button"
                      className="btn text-xs"
                      disabled={index === items.length - 1}
                      onClick={() => moveItem(index, 'down')}
                    >
                      ↓
                    </button>
                    <button
                      type="button"
                      className="btn text-xs text-red-600"
                      onClick={() => removeItem(index)}
                    >
                      Eliminar
                    </button>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                  <div>
                    <label className="mb-1 block text-xs text-gray-600">Series *</label>
                    <input
                      type="number"
                      min="1"
                      className={`input ${fieldErrors[`item_${index}_sets`] ? 'border-red-500' : ''}`}
                      value={item.sets}
                      onChange={e => {
                        updateItem(index, { sets: Number(e.target.value) })
                        if (fieldErrors[`item_${index}_sets`]) {
                          setFieldErrors(prev => {
                            const newErrors = { ...prev }
                            delete newErrors[`item_${index}_sets`]
                            return newErrors
                          })
                        }
                      }}
                    />
                    {fieldErrors[`item_${index}_sets`] && (
                      <p className="mt-1 text-xs text-red-600">{fieldErrors[`item_${index}_sets`]}</p>
                    )}
                  </div>

                  <div>
                    <label className="mb-1 block text-xs text-gray-600">Repeticiones *</label>
                    <input
                      type="number"
                      min="1"
                      className={`input ${fieldErrors[`item_${index}_reps`] ? 'border-red-500' : ''}`}
                      value={item.reps}
                      onChange={e => {
                        updateItem(index, { reps: Number(e.target.value) })
                        if (fieldErrors[`item_${index}_reps`]) {
                          setFieldErrors(prev => {
                            const newErrors = { ...prev }
                            delete newErrors[`item_${index}_reps`]
                            return newErrors
                          })
                        }
                      }}
                    />
                    {fieldErrors[`item_${index}_reps`] && (
                      <p className="mt-1 text-xs text-red-600">{fieldErrors[`item_${index}_reps`]}</p>
                    )}
                  </div>

                  <div className="col-span-2">
                    <label className="mb-1 block text-xs text-gray-600">Notas (opcional)</label>
                    <input
                      type="text"
                      className="input"
                      placeholder="Ej: descanso 60s, peso moderado..."
                      value={item.notes || ''}
                      onChange={e => updateItem(index, { notes: e.target.value })}
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="flex gap-2">
        <button className="btn-primary" type="submit" disabled={loading}>
          {loading ? 'Guardando…' : 'Guardar rutina'}
        </button>
        <button
          className="btn"
          type="button"
          onClick={() => nav(`/athletes/${athleteId}/routines`)}
        >
          Cancelar
        </button>
      </div>
    </form>
  )
}

