import { useEffect, useState } from 'react'
import { search, remove } from '../api/exercises'
import { Link } from 'react-router-dom'
import type { Exercise, PagedResult } from '../lib/types'
import { useAuth } from '../auth/useAuth'
import Table from '../components/Table'
import EmptyState from '../components/EmptyState'
import Loader from '../components/Loader'

const CATEGORIES = ['Fuerza', 'Cardio', 'Movilidad', 'Flexibilidad', 'General']
const MUSCLE_GROUPS = ['Pectoral', 'Espalda', 'Hombros', 'Brazos', 'Core', 'Piernas', 'Glúteos']
const EQUIPMENT = ['Peso corporal', 'Mancuernas', 'Barra', 'Kettlebell', 'Máquina', 'Banda elástica', 'TRX']
const DIFFICULTIES = ['Inicial', 'Intermedio', 'Avanzado']
const ENVIRONMENTS = ['Gimnasio', 'Casa', 'Exterior']

export default function ExercisesList() {
  const [pagedData, setPagedData] = useState<PagedResult<Exercise> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedCategory, setSelectedCategory] = useState<string>('')
  const [selectedMuscle, setSelectedMuscle] = useState<string>('')
  const [selectedEquipment, setSelectedEquipment] = useState<string>('')
  const [selectedDifficulty, setSelectedDifficulty] = useState<string>('')
  const [selectedEnvironment, setSelectedEnvironment] = useState<string>('')
  const [tagFilter, setTagFilter] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const { role } = useAuth()
  const canEdit = role === 'Admin' || role === 'Coach'

  const pageSize = 10

  const hasFilters = Boolean(
    searchQuery
      || selectedCategory
      || selectedMuscle
      || selectedEquipment
      || selectedDifficulty
      || selectedEnvironment
      || tagFilter,
  )

  const refresh = (
    page = 1,
    q = searchQuery,
    category = selectedCategory,
    targetMuscleGroup = selectedMuscle,
    equipment = selectedEquipment,
    difficultyLevel = selectedDifficulty,
    environment = selectedEnvironment,
    tag = tagFilter,
  ) => {
    setLoading(true)
    setError(null)
    setActionError(null)
    return search({
      page,
      pageSize,
      q: q || undefined,
      category: category || undefined,
      targetMuscleGroup: targetMuscleGroup || undefined,
      equipment: equipment || undefined,
      difficultyLevel: difficultyLevel || undefined,
      environment: environment || undefined,
      tag: tag || undefined,
    })
      .then(res => {
        setPagedData(res)
        setCurrentPage(res.page)
      })
      .catch(err => {
        const msg = err?.response?.data ?? 'Error al cargar ejercicios'
        setError(typeof msg === 'string' ? msg : 'Error al cargar ejercicios')
      })
      .finally(() => setLoading(false))
  }

  const handleDelete = async (exerciseId: number) => {
    const exercise = pagedData?.items.find(item => item.id === exerciseId)
    const canDeleteExercise = role === 'Admin' || (role === 'Coach' && exercise && !exercise.isGlobal)
    if (!canDeleteExercise) {
      setActionError('No tienes permisos para eliminar este ejercicio.')
      return
    }
    if (!window.confirm('¿Eliminar ejercicio?')) return
    setActionError(null)
    setDeletingId(exerciseId)
    try {
      await remove(exerciseId)
      await refresh(currentPage)
    } catch (err: unknown) {
      const axiosError = err as { response?: { status?: number; data?: unknown } }
      const status = axiosError?.response?.status
      if (status === 403) {
        setActionError('No tienes permisos para eliminar ejercicios.')
      } else {
        const msg = axiosError?.response?.data ?? 'No se pudo eliminar el ejercicio'
        setActionError(typeof msg === 'string' ? msg : 'No se pudo eliminar el ejercicio')
      }
    } finally {
      setDeletingId(null)
    }
  }

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setCurrentPage(1)
    refresh(1)
  }

  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage)
    refresh(newPage)
  }

  useEffect(() => {
    refresh(1, '', '', '', '', '', '', '')
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const totalPages = pagedData ? Math.ceil(pagedData.total / pageSize) : 0

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold">Ejercicios</h1>
        {canEdit && (
          <Link to="/exercises/new" className="btn-primary">
            Nuevo ejercicio
          </Link>
        )}
      </div>

      <div className="card space-y-4">
        <form onSubmit={handleSearch} className="grid gap-2 lg:grid-cols-6">
          <input
            type="text"
            className="input lg:col-span-2"
            placeholder="Buscar por nombre, descripción o tag..."
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
          <select
            className="input"
            value={selectedCategory}
            onChange={e => setSelectedCategory(e.target.value)}
          >
            <option value="">Todas las categorías</option>
            {CATEGORIES.map(cat => (
              <option key={cat} value={cat}>
                {cat}
              </option>
            ))}
          </select>
          <select
            className="input"
            value={selectedMuscle}
            onChange={e => setSelectedMuscle(e.target.value)}
          >
            <option value="">Todos los músculos</option>
            {MUSCLE_GROUPS.map(item => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
          <select
            className="input"
            value={selectedEquipment}
            onChange={e => setSelectedEquipment(e.target.value)}
          >
            <option value="">Todo equipamiento</option>
            {EQUIPMENT.map(item => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
          <select
            className="input"
            value={selectedDifficulty}
            onChange={e => setSelectedDifficulty(e.target.value)}
          >
            <option value="">Toda dificultad</option>
            {DIFFICULTIES.map(item => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
          <select
            className="input"
            value={selectedEnvironment}
            onChange={e => setSelectedEnvironment(e.target.value)}
          >
            <option value="">Todo entorno</option>
            {ENVIRONMENTS.map(item => (
              <option key={item} value={item}>
                {item}
              </option>
            ))}
          </select>
          <input
            type="text"
            className="input lg:col-span-2"
            placeholder="Tag..."
            value={tagFilter}
            onChange={e => setTagFilter(e.target.value)}
          />
          <div className="flex gap-2 lg:col-span-4">
            <button type="submit" className="btn-primary">
              Buscar
            </button>
            {hasFilters && (
              <button
                type="button"
                className="btn"
                onClick={() => {
                  setSearchQuery('')
                  setSelectedCategory('')
                  setSelectedMuscle('')
                  setSelectedEquipment('')
                  setSelectedDifficulty('')
                  setSelectedEnvironment('')
                  setTagFilter('')
                  setCurrentPage(1)
                  refresh(1, '', '', '', '', '', '', '')
                }}
              >
                Limpiar
              </button>
            )}
          </div>
        </form>

        {error && <p className="text-sm text-red-600">{error}</p>}
        {actionError && <p className="text-sm text-red-600">{actionError}</p>}

        {loading ? (
          <Loader text="Cargando ejercicios..." />
        ) : !pagedData || pagedData.items.length === 0 ? (
          <EmptyState
            title="No hay ejercicios"
            message={
              hasFilters
                ? 'No se encontraron ejercicios con ese criterio.'
                : 'Aún no hay ejercicios registrados.'
            }
            actionLabel={!hasFilters && canEdit ? 'Crear primer ejercicio' : undefined}
            onAction={
              !hasFilters && canEdit
                ? () => (window.location.href = '/exercises/new')
                : undefined
            }
          />
        ) : (
          <>
            <Table
              columns={[
                {
                  key: 'name',
                  label: 'Nombre',
                  render: e => e.name,
                },
                {
                  key: 'category',
                  label: 'Categoría',
                  render: e => (
                    <span className="rounded-full bg-primary-100 px-2 py-1 text-xs text-primary-700">
                      {e.category}
                    </span>
                  ),
                },
                {
                  key: 'origin',
                  label: 'Origen',
                  render: e => (
                    <span
                      className={`rounded-full px-2 py-1 text-xs ${
                        e.isGlobal ? 'bg-blue-100 text-blue-700' : 'bg-emerald-100 text-emerald-700'
                      }`}
                    >
                      {e.isGlobal ? 'Sistema' : 'Propio'}
                    </span>
                  ),
                },
                {
                  key: 'defaultSets',
                  label: 'Series',
                  render: e => e.defaultSets ?? '—',
                },
                {
                  key: 'defaultReps',
                  label: 'Repeticiones',
                  render: e => e.defaultReps ?? '—',
                },
                {
                  key: 'actions',
                  label: 'Acciones',
                  render: e => (
                    <div className="flex gap-2">
                      {(role === 'Admin' || (canEdit && !e.isGlobal)) && (
                        <Link className="btn text-xs" to={`/exercises/${e.id}`}>
                          Editar
                        </Link>
                      )}
                      {(role === 'Admin' || (role === 'Coach' && !e.isGlobal)) && (
                        <button
                          className="btn text-xs"
                          disabled={deletingId === e.id}
                          onClick={ev => {
                            ev.stopPropagation()
                            handleDelete(e.id)
                          }}
                        >
                          {deletingId === e.id ? 'Eliminando…' : 'Eliminar'}
                        </button>
                      )}
                    </div>
                  ),
                },
              ]}
              data={pagedData.items}
              keyExtractor={e => e.id}
            />

            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t pt-4">
                <p className="text-sm text-gray-600">
                  Mostrando {pagedData.items.length} de {pagedData.total} ejercicios
                </p>
                <div className="flex gap-2">
                  <button
                    className="btn"
                    disabled={currentPage === 1}
                    onClick={() => handlePageChange(currentPage - 1)}
                  >
                    Anterior
                  </button>
                  <span className="flex items-center px-3 text-sm text-gray-600">
                    Página {currentPage} de {totalPages}
                  </span>
                  <button
                    className="btn"
                    disabled={currentPage >= totalPages}
                    onClick={() => handlePageChange(currentPage + 1)}
                  >
                    Siguiente
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  )
}
