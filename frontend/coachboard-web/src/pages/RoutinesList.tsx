import { useEffect, useState } from 'react'
import { list, remove } from '../api/routines'
import { Link, useParams, useNavigate } from 'react-router-dom'
import type { Routine, PagedResult } from '../lib/types'
import { useAuth } from '../auth/useAuth'
import Table from '../components/Table'
import EmptyState from '../components/EmptyState'
import Loader from '../components/Loader'
import { get as getAthlete } from '../api/athletes'

export default function RoutinesList() {
  const { athleteId } = useParams<{ athleteId: string }>()
  const nav = useNavigate()
  const [pagedData, setPagedData] = useState<PagedResult<Routine> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [athleteName, setAthleteName] = useState<string>('')
  const { role } = useAuth()
  const canDelete = role === 'Admin' || role === 'Coach'

  const pageSize = 10

  useEffect(() => {
    if (!athleteId) {
      setError('ID de atleta no proporcionado')
      setLoading(false)
      return
    }

    // Cargar nombre del atleta
    getAthlete(athleteId)
      .then(a => setAthleteName(`${a.firstName} ${a.lastName}`.trim()))
      .catch(() => setAthleteName('Atleta'))

    refresh(1, '')
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [athleteId])

  const refresh = (page = 1, q = '') => {
    if (!athleteId) return Promise.resolve()
    setLoading(true)
    setError(null)
    setActionError(null)
    return list({ clientId: Number(athleteId), page, pageSize, q: q || undefined })
      .then(res => {
        setPagedData(res)
        setCurrentPage(res.page)
      })
      .catch(err => {
        const msg = err?.response?.data ?? 'Error al cargar rutinas'
        setError(typeof msg === 'string' ? msg : 'Error al cargar rutinas')
      })
      .finally(() => setLoading(false))
  }

  const handleDelete = async (routineId: number) => {
    if (!canDelete) {
      setActionError('No tienes permisos para eliminar rutinas.')
      return
    }
    if (!window.confirm('¿Eliminar rutina? Esta acción no se puede deshacer.')) return
    setActionError(null)
    setDeletingId(routineId)
    try {
      await remove(routineId)
      await refresh(currentPage, searchQuery)
    } catch (err: unknown) {
      const axiosError = err as { response?: { status?: number; data?: unknown } }
      const msg = axiosError?.response?.data ?? 'No se pudo eliminar la rutina'
      setActionError(typeof msg === 'string' ? msg : 'No se pudo eliminar la rutina')
    } finally {
      setDeletingId(null)
    }
  }

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setCurrentPage(1)
    refresh(1, searchQuery)
  }

  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage)
    refresh(newPage, searchQuery)
  }

  if (!athleteId) {
    return (
      <div className="card">
        <p className="text-red-600">ID de atleta no proporcionado.</p>
        <Link to="/athletes" className="btn-primary mt-4">
          Volver a atletas
        </Link>
      </div>
    )
  }

  const totalPages = pagedData ? Math.ceil(pagedData.total / pageSize) : 0

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <Link to="/athletes" className="text-sm text-gray-500 hover:text-gray-700">
            ← Volver a atletas
          </Link>
          <h1 className="text-2xl font-bold">Rutinas de {athleteName || 'Atleta'}</h1>
        </div>
        <Link to={`/athletes/${athleteId}/routines/new`} className="btn-primary">
          Nueva rutina
        </Link>
      </div>

      <div className="card space-y-4">
        <form onSubmit={handleSearch} className="flex gap-2">
          <input
            type="text"
            className="input flex-1"
            placeholder="Buscar por título..."
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
          <button type="submit" className="btn-primary">
            Buscar
          </button>
          {searchQuery && (
            <button
              type="button"
              className="btn"
              onClick={() => {
                setSearchQuery('')
                setCurrentPage(1)
                refresh(1, '')
              }}
            >
              Limpiar
            </button>
          )}
        </form>

        {error && <p className="text-sm text-red-600">{error}</p>}
        {actionError && <p className="text-sm text-red-600">{actionError}</p>}

        {loading ? (
          <Loader text="Cargando rutinas..." />
        ) : !pagedData || pagedData.items.length === 0 ? (
          <EmptyState
            title="No hay rutinas"
            message={
              searchQuery
                ? 'No se encontraron rutinas con ese criterio.'
                : 'Este atleta aún no tiene rutinas asignadas.'
            }
            actionLabel={!searchQuery ? 'Crear primera rutina' : undefined}
            onAction={!searchQuery ? () => nav(`/athletes/${athleteId}/routines/new`) : undefined}
          />
        ) : (
          <>
            <Table
              columns={[
                {
                  key: 'title',
                  label: 'Título',
                  render: r => r.title,
                },
                {
                  key: 'items',
                  label: 'Ejercicios',
                  render: r => (
                    <span className="text-sm text-gray-600">{r.items.length} ejercicio(s)</span>
                  ),
                },
                {
                  key: 'createdAt',
                  label: 'Creada',
                  render: r =>
                    r.createdAt
                      ? new Date(r.createdAt).toLocaleDateString('es-ES', {
                          year: 'numeric',
                          month: 'short',
                          day: 'numeric',
                        })
                      : '—',
                },
                {
                  key: 'actions',
                  label: 'Acciones',
                  render: r => (
                    <div className="flex gap-2">
                      <Link className="btn text-xs" to={`/athletes/${athleteId}/routines/${r.id}`}>
                        Ver/Editar
                      </Link>
                      {canDelete && (
                        <button
                          className="btn text-xs"
                          disabled={deletingId === r.id}
                          onClick={ev => {
                            ev.stopPropagation()
                            handleDelete(r.id)
                          }}
                        >
                          {deletingId === r.id ? 'Eliminando…' : 'Eliminar'}
                        </button>
                      )}
                    </div>
                  ),
                },
              ]}
              data={pagedData.items}
              keyExtractor={r => r.id}
            />

            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t pt-4">
                <p className="text-sm text-gray-600">
                  Mostrando {pagedData.items.length} de {pagedData.total} rutinas
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

