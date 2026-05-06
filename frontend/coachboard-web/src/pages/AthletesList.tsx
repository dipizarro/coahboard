import { useEffect, useState } from 'react'
import { list, remove } from '../api/athletes'
import { Link } from 'react-router-dom'
import type { Athlete, PagedResult } from '../lib/types'
import { useAuth } from '../auth/useAuth'
import Table from '../components/Table'
import EmptyState from '../components/EmptyState'
import Loader from '../components/Loader'

export default function AthletesList() {
  const [pagedData, setPagedData] = useState<PagedResult<Athlete> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [deletingId, setDeletingId] = useState<string | null>(null)
  const [searchQuery, setSearchQuery] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const { coachId, role } = useAuth()
  const canDelete = role

  const pageSize = 10

  const refresh = (page = 1, q = '') => {
    setLoading(true)
    setError(null)
    setActionError(null)
    if (coachId == null) {
      setPagedData(null)
      setError('Tu sesión no tiene un coach asignado. Vuelve a iniciar sesión.')
      setLoading(false)
      return Promise.resolve()
    }
    return list({ page, pageSize, q: q || undefined, coachId })
      .then(res => {
        setPagedData(res)
        setCurrentPage(res.page)
      })
      .catch(err => {
        const msg = err?.response?.data ?? 'Error al cargar atletas'
        setError(typeof msg === 'string' ? msg : 'Error al cargar atletas')
      })
      .finally(() => setLoading(false))
  }

  const handleDelete = async (athleteId: string) => {
    if (!canDelete) {
      setActionError('Tu rol actual no permite eliminar atletas. Contacta a un administrador.')
      return
    }
    if (!window.confirm('¿Eliminar atleta?')) return
    setActionError(null)
    setDeletingId(athleteId)
    try {
      await remove(athleteId)
      await refresh(currentPage, searchQuery)
    } catch (err: unknown) {
      const axiosError = err as { response?: { status?: number; data?: unknown } }
      const status = axiosError?.response?.status
      if (status === 403) {
        setActionError('La API devolvió 403: solo un administrador puede eliminar atletas.')
      } else {
        const msg = axiosError?.response?.data ?? 'No se pudo eliminar al atleta'
        setActionError(typeof msg === 'string' ? msg : 'No se pudo eliminar al atleta')
      }
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

  useEffect(() => {
    refresh(1, '')
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [coachId])

  const totalPages = pagedData ? Math.ceil(pagedData.total / pageSize) : 0

  return (
    <div className="space-y-4">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold">Atletas</h1>
        <Link to="/athletes/new" className="btn-primary">
          Nuevo atleta
        </Link>
      </div>

      <div className="card space-y-4">
        <form onSubmit={handleSearch} className="flex gap-2">
          <input
            type="text"
            className="input flex-1"
            placeholder="Buscar por nombre o email..."
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
        {!canDelete && (
          <p className="text-sm text-amber-600">
            Tu rol actual no permite eliminar atletas. Si necesitas esta acción, solicita apoyo a un administrador.
          </p>
        )}

        {loading ? (
          <Loader text="Cargando atletas..." />
        ) : !pagedData || pagedData.items.length === 0 ? (
          <EmptyState
            title="No hay atletas"
            message={searchQuery ? 'No se encontraron atletas con ese criterio.' : 'Aún no tienes atletas registrados.'}
            actionLabel={!searchQuery ? 'Crear primer atleta' : undefined}
            onAction={!searchQuery ? () => (window.location.href = '/athletes/new') : undefined}
          />
        ) : (
          <>
            <Table
              columns={[
                {
                  key: 'name',
                  label: 'Nombre',
                  render: a => `${a.firstName} ${a.lastName}`.trim() || '—',
                },
                {
                  key: 'email',
                  label: 'Email',
                  render: a => a.email ?? '—',
                },
                {
                  key: 'phone',
                  label: 'Teléfono',
                  render: a => a.phone ?? '—',
                },
                {
                  key: 'actions',
                  label: 'Acciones',
                  render: a => (
                    <div className="flex flex-wrap gap-2">
                      <Link className="btn-primary text-xs" to={`/athletes/${a.id}/profile`}>
                        Ficha
                      </Link>
                      <Link className="btn text-xs" to={`/athletes/${a.id}`}>
                        Editar
                      </Link>
                      <Link className="btn text-xs" to={`/athletes/${a.id}/routines`}>
                        Rutinas
                      </Link>
                      {canDelete && (
                        <button
                          className="btn text-xs"
                          disabled={deletingId === a.id}
                          onClick={e => {
                            e.stopPropagation()
                            handleDelete(a.id)
                          }}
                        >
                          {deletingId === a.id ? 'Eliminando…' : 'Eliminar'}
                        </button>
                      )}
                    </div>
                  ),
                },
              ]}
              data={pagedData.items}
              keyExtractor={a => a.id}
            />

            {totalPages > 1 && (
              <div className="flex items-center justify-between border-t pt-4">
                <p className="text-sm text-gray-600">
                  Mostrando {pagedData.items.length} de {pagedData.total} atletas
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
