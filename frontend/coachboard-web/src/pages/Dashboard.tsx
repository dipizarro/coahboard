import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { list as listAthletes } from '../api/athletes'
import { list as listRoutines } from '../api/routines'
import { search as searchExercises } from '../api/exercises'
import { list as listSessions } from '../api/sessions'
import { useAuth } from '../auth/useAuth'
import type { Athlete, Session } from '../lib/types'
import Loader from '../components/Loader'

type DashboardStats = {
  athletesCount: number
  routinesCount: number
  exercisesCount: number
  recentAthletes: Athlete[]
  upcomingSessions: Session[]
}

export default function Dashboard() {
  const { coachId } = useAuth()
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (coachId == null) {
      setLoading(false)
      return
    }

    const loadStats = async () => {
      try {
        setLoading(true)
        setError(null)

        // Cargar estadísticas en paralelo
        const today = new Date()
        const nextWeek = new Date(today)
        nextWeek.setDate(today.getDate() + 7)

        const [athletesResult, exercisesResult, sessionsResult] = await Promise.all([
          listAthletes({ page: 1, pageSize: 5, coachId }), // Solo necesitamos los primeros 5 para "recientes"
          searchExercises({ page: 1, pageSize: 1 }), // Solo necesitamos el total
          listSessions({ coachId, from: today.toISOString(), to: nextWeek.toISOString() })
        ])

        // Obtener rutinas de todos los atletas (limitado a los primeros 5 atletas para no sobrecargar)
        let totalRoutines = 0
        const athleteIds = athletesResult.items.slice(0, 5).map(a => Number(a.id))

        if (athleteIds.length > 0) {
          const routinePromises = athleteIds.map(athleteId =>
            listRoutines({ clientId: athleteId, page: 1, pageSize: 1 }).catch(() => null)
          )
          const routineResults = await Promise.all(routinePromises)
          totalRoutines = routineResults.reduce((sum, res) => sum + (res?.total ?? 0), 0)
        }

        // Obtener atletas recientes (ordenados por fecha de creación)
        const recentAthletes = [...athletesResult.items]
          .sort((a, b) => {
            const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0
            const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0
            return dateB - dateA
          })
          .slice(0, 5)

        setStats({
          athletesCount: athletesResult.total,
          routinesCount: totalRoutines,
          exercisesCount: exercisesResult.total,
          recentAthletes,
          upcomingSessions: sessionsResult,
        })
      } catch (err) {
        setError('Error al cargar estadísticas del dashboard')
        console.error(err)
      } finally {
        setLoading(false)
      }
    }

    loadStats()
  }, [coachId])

  if (loading) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <Loader text="Cargando estadísticas..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <div className="card">
          <p className="text-red-600">{error}</p>
        </div>
      </div>
    )
  }

  if (!stats) {
    return (
      <div className="space-y-4">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <div className="card">
          <p className="text-gray-500">No hay datos para mostrar.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <div className="flex gap-2">
          <Link to="/athletes/new" className="btn-primary">
            Nuevo atleta
          </Link>
          <Link to="/exercises" className="btn">
            Ver ejercicios
          </Link>
        </div>
      </div>

      {/* Tarjetas de estadísticas */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Total Atletas</p>
              <p className="text-3xl font-semibold">{stats.athletesCount}</p>
            </div>
            <div className="rounded-full bg-blue-100 p-3">
              <svg className="h-6 w-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"
                />
              </svg>
            </div>
          </div>
          <Link to="/athletes" className="mt-2 block text-sm text-primary-600 hover:underline">
            Ver todos →
          </Link>
        </div>

        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Rutinas Creadas</p>
              <p className="text-3xl font-semibold">{stats.routinesCount}</p>
            </div>
            <div className="rounded-full bg-green-100 p-3">
              <svg className="h-6 w-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                />
              </svg>
            </div>
          </div>
          <p className="mt-2 text-xs text-gray-500">En todos tus atletas</p>
        </div>

        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Ejercicios</p>
              <p className="text-3xl font-semibold">{stats.exercisesCount}</p>
            </div>
            <div className="rounded-full bg-purple-100 p-3">
              <svg className="h-6 w-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M13 10V3L4 14h7v7l9-11h-7z"
                />
              </svg>
            </div>
          </div>
          <Link to="/exercises" className="mt-2 block text-sm text-primary-600 hover:underline">
            Ver todos →
          </Link>
        </div>

        <div className="card">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-500">Promedio</p>
              <p className="text-3xl font-semibold">
                {stats.athletesCount > 0
                  ? Math.round((stats.routinesCount / stats.athletesCount) * 10) / 10
                  : 0}
              </p>
            </div>
            <div className="rounded-full bg-amber-100 p-3">
              <svg className="h-6 w-6 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
                />
              </svg>
            </div>
          </div>
          <p className="mt-2 text-xs text-gray-500">Rutinas por atleta</p>
        </div>
      </div>

      {/* Atletas recientes */}
      <div className="grid gap-6 lg:grid-cols-2">
        <div className="card">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-lg font-semibold">Atletas Recientes</h2>
            <Link to="/athletes" className="text-sm text-primary-600 hover:underline">
              Ver todos
            </Link>
          </div>
          {stats.recentAthletes.length === 0 ? (
            <p className="text-sm text-gray-500">Aún no tienes atletas registrados.</p>
          ) : (
            <div className="space-y-3">
              {stats.recentAthletes.map(athlete => (
                <div
                  key={athlete.id}
                  className="flex items-center justify-between rounded-lg border p-3 hover:bg-gray-50"
                >
                  <div className="flex-1">
                    <p className="font-medium">
                      {athlete.firstName} {athlete.lastName}
                    </p>
                    {athlete.email && <p className="text-xs text-gray-500">{athlete.email}</p>}
                    {athlete.createdAt && (
                      <p className="text-xs text-gray-400">
                        Agregado {new Date(athlete.createdAt).toLocaleDateString('es-ES')}
                      </p>
                    )}
                  </div>
                  <div className="flex gap-2">
                    <Link
                      to={`/athletes/${athlete.id}/routines`}
                      className="btn-primary text-xs"
                    >
                      Rutinas
                    </Link>
                    <Link to={`/athletes/${athlete.id}`} className="btn text-xs">
                      Editar
                    </Link>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Próximas Sesiones */}
        <div className="card">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-lg font-semibold">Próximas Sesiones (7 días)</h2>
            <Link to="/sessions" className="text-sm text-primary-600 hover:underline">
              Ver calendario
            </Link>
          </div>
          {stats.upcomingSessions.length === 0 ? (
            <p className="text-sm text-gray-500">No tienes sesiones programadas para esta semana.</p>
          ) : (
            <div className="space-y-3">
              {stats.upcomingSessions.slice(0, 5).map(session => (
                <Link
                  key={session.id}
                  to={`/sessions/${session.id}`}
                  className="flex items-center justify-between rounded-lg border p-3 hover:bg-gray-50"
                >
                  <div className="flex-1">
                    <p className="font-medium">
                      {session.clientName || 'Sin cliente'}
                    </p>
                    <p className="text-xs text-gray-500">
                      {new Date(session.startAt).toLocaleDateString()} - {new Date(session.startAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </p>
                  </div>
                  <div className={`text-xs px-2 py-1 rounded ${session.status === 'Done' ? 'bg-green-100 text-green-800' :
                    session.status === 'Canceled' ? 'bg-red-100 text-red-800' :
                      'bg-blue-100 text-blue-800'
                    }`}>
                    {session.status}
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        {/* Acciones rápidas */}
        <div className="card">
          <h2 className="mb-4 text-lg font-semibold">Acciones Rápidas</h2>
          <div className="space-y-3">
            <Link
              to="/athletes/new"
              className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-gray-50"
            >
              <div className="rounded-full bg-primary-100 p-2">
                <svg className="h-5 w-5 text-primary-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M12 4v16m8-8H4"
                  />
                </svg>
              </div>
              <div className="flex-1">
                <p className="font-medium">Agregar nuevo atleta</p>
                <p className="text-xs text-gray-500">Registra un nuevo atleta en tu sistema</p>
              </div>
            </Link>

            <Link
              to="/sessions/new"
              className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-gray-50"
            >
              <div className="rounded-full bg-green-100 p-2">
                <svg className="h-5 w-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
              </div>
              <div className="flex-1">
                <p className="font-medium">Programar sesión</p>
                <p className="text-xs text-gray-500">Agenda un nuevo entrenamiento</p>
              </div>
            </Link>

            <Link
              to="/exercises"
              className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-gray-50"
            >
              <div className="rounded-full bg-purple-100 p-2">
                <svg className="h-5 w-5 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M13 10V3L4 14h7v7l9-11h-7z"
                  />
                </svg>
              </div>
              <div className="flex-1">
                <p className="font-medium">Gestionar ejercicios</p>
                <p className="text-xs text-gray-500">Ver, crear o editar ejercicios disponibles</p>
              </div>
            </Link>

            <Link
              to="/athletes"
              className="flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-gray-50"
            >
              <div className="rounded-full bg-blue-100 p-2">
                <svg className="h-5 w-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth={2}
                    d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                  />
                </svg>
              </div>
              <div className="flex-1">
                <p className="font-medium">Ver todos los atletas</p>
                <p className="text-xs text-gray-500">Gestiona tu lista completa de atletas</p>
              </div>
            </Link>
          </div>
        </div>
      </div>
    </div>
  )
}
