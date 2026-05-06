import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { get as getAthlete } from '../api/athletes'
import { list as listProgress, create as createProgress, remove as removeProgress } from '../api/progress'
import { list as listRoutines } from '../api/routines'
import { list as listSessions } from '../api/sessions'
import AthleteHealthCard from '../components/AthleteHealthCard'
import AthleteSummaryCard from '../components/AthleteSummaryCard'
import EmptyState from '../components/EmptyState'
import LatestProgressCard from '../components/LatestProgressCard'
import Loader from '../components/Loader'
import ProgressHistoryTable from '../components/ProgressHistoryTable'
import Table from '../components/Table'
import { useAuth } from '../auth/useAuth'
import type { Athlete, ClientProgressPayload, ClientProgressRecord, Routine, Session } from '../lib/types'

type ProgressFormState = {
  recordedAt: string
  weightKg: string
  bodyFatPercentage: string
  waistCm: string
  restingHeartRate: string
  notes: string
}

const initialProgressForm = (): ProgressFormState => ({
  recordedAt: new Date().toISOString().slice(0, 10),
  weightKg: '',
  bodyFatPercentage: '',
  waistCm: '',
  restingHeartRate: '',
  notes: '',
})

function numberOrNull(value: string) {
  const trimmed = value.trim()
  return trimmed ? Number(trimmed) : null
}

function textOrNull(value: string) {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString('es-ES', {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

function fullName(athlete?: Athlete | null) {
  if (!athlete) return 'Atleta'
  return `${athlete.firstName} ${athlete.lastName}`.trim() || 'Atleta'
}

export default function AthleteProfile() {
  const { id } = useParams<{ id: string }>()
  const nav = useNavigate()
  const { coachId } = useAuth()
  const [athlete, setAthlete] = useState<Athlete | null>(null)
  const [progress, setProgress] = useState<ClientProgressRecord[]>([])
  const [routines, setRoutines] = useState<Routine[]>([])
  const [sessions, setSessions] = useState<Session[]>([])
  const [loading, setLoading] = useState(true)
  const [savingProgress, setSavingProgress] = useState(false)
  const [deletingProgressId, setDeletingProgressId] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)
  const [progressForm, setProgressForm] = useState<ProgressFormState>(() => initialProgressForm())

  const clientId = id ? Number(id) : NaN

  const latestProgress = useMemo(() => {
    return [...progress].sort((a, b) => new Date(b.recordedAt).getTime() - new Date(a.recordedAt).getTime())[0] ?? null
  }, [progress])

  async function refresh() {
    if (!id || Number.isNaN(clientId)) {
      setError('ID de atleta no válido.')
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    setActionError(null)

    try {
      const today = new Date()
      const from = new Date(today)
      from.setDate(today.getDate() - 90)
      const to = new Date(today)
      to.setDate(today.getDate() + 30)

      const [athleteData, progressData, routinesData, sessionsData] = await Promise.all([
        getAthlete(id),
        listProgress(clientId),
        listRoutines({ clientId, page: 1, pageSize: 5 }),
        coachId
          ? listSessions({
              coachId,
              clientId,
              from: from.toISOString(),
              to: to.toISOString(),
            }).catch(() => [])
          : Promise.resolve([]),
      ])

      setAthlete(athleteData)
      setProgress(progressData)
      setRoutines(routinesData.items)
      setSessions(sessionsData)
    } catch (err) {
      console.error(err)
      setError('No se pudo cargar la ficha del alumno.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    refresh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id, coachId])

  function buildProgressPayload(): ClientProgressPayload {
    return {
      recordedAt: new Date(progressForm.recordedAt).toISOString(),
      weightKg: numberOrNull(progressForm.weightKg),
      bodyFatPercentage: numberOrNull(progressForm.bodyFatPercentage),
      waistCm: numberOrNull(progressForm.waistCm),
      restingHeartRate: progressForm.restingHeartRate ? Number(progressForm.restingHeartRate) : null,
      notes: textOrNull(progressForm.notes),
    }
  }

  async function handleCreateProgress(event: FormEvent) {
    event.preventDefault()
    if (!id || Number.isNaN(clientId)) return

    setSavingProgress(true)
    setActionError(null)
    try {
      const created = await createProgress(clientId, buildProgressPayload())
      setProgress(records => [created, ...records])
      setProgressForm(initialProgressForm())
    } catch (err: any) {
      const msg = err?.response?.data ?? 'No se pudo registrar la medición.'
      setActionError(typeof msg === 'string' ? msg : 'No se pudo registrar la medición.')
    } finally {
      setSavingProgress(false)
    }
  }

  async function handleDeleteProgress(record: ClientProgressRecord) {
    if (!window.confirm('¿Eliminar medición?')) return

    setDeletingProgressId(record.id)
    setActionError(null)
    try {
      await removeProgress(clientId, record.id)
      setProgress(records => records.filter(item => item.id !== record.id))
    } catch (err: any) {
      const msg = err?.response?.data ?? 'No se pudo eliminar la medición.'
      setActionError(typeof msg === 'string' ? msg : 'No se pudo eliminar la medición.')
    } finally {
      setDeletingProgressId(null)
    }
  }

  if (loading) {
    return (
      <div className="space-y-4">
        <Link to="/athletes" className="text-sm text-gray-500 hover:text-gray-700">
          Volver a atletas
        </Link>
        <Loader text="Cargando ficha del alumno..." />
      </div>
    )
  }

  if (error || !athlete) {
    return (
      <div className="card space-y-4">
        <p className="text-sm text-red-600">{error ?? 'No se encontró el atleta.'}</p>
        <button className="btn-primary" onClick={() => nav('/athletes')}>
          Volver a atletas
        </button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <Link to="/athletes" className="text-sm text-gray-500 hover:text-gray-700">
            Volver a atletas
          </Link>
          <h1 className="text-2xl font-bold">{fullName(athlete)}</h1>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link to={`/athletes/${id}`} className="btn">
            Editar datos
          </Link>
          <Link to={`/athletes/${id}/routines`} className="btn-primary">
            Ver rutinas
          </Link>
        </div>
      </div>

      {actionError && <p className="text-sm text-red-600">{actionError}</p>}

      <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_360px]">
        <div className="space-y-4">
          <AthleteSummaryCard athlete={athlete} />
          <AthleteHealthCard athlete={athlete} />
        </div>
        <LatestProgressCard record={latestProgress} />
      </div>

      <section className="card space-y-4">
        <div className="flex flex-col gap-1">
          <p className="text-sm text-gray-500">Seguimiento</p>
          <h2 className="text-lg font-semibold">Nueva medición</h2>
        </div>

        <form onSubmit={handleCreateProgress} className="grid gap-3 md:grid-cols-6">
          <label className="space-y-1 text-sm text-gray-600 md:col-span-2">
            <span>Fecha</span>
            <input
              className="input"
              type="date"
              required
              value={progressForm.recordedAt}
              onChange={event => setProgressForm(form => ({ ...form, recordedAt: event.target.value }))}
            />
          </label>
          <label className="space-y-1 text-sm text-gray-600">
            <span>Peso kg</span>
            <input
              className="input"
              type="number"
              min="0"
              step="0.01"
              value={progressForm.weightKg}
              onChange={event => setProgressForm(form => ({ ...form, weightKg: event.target.value }))}
            />
          </label>
          <label className="space-y-1 text-sm text-gray-600">
            <span>Grasa %</span>
            <input
              className="input"
              type="number"
              min="0"
              max="100"
              step="0.01"
              value={progressForm.bodyFatPercentage}
              onChange={event => setProgressForm(form => ({ ...form, bodyFatPercentage: event.target.value }))}
            />
          </label>
          <label className="space-y-1 text-sm text-gray-600">
            <span>Cintura cm</span>
            <input
              className="input"
              type="number"
              min="0"
              step="0.01"
              value={progressForm.waistCm}
              onChange={event => setProgressForm(form => ({ ...form, waistCm: event.target.value }))}
            />
          </label>
          <label className="space-y-1 text-sm text-gray-600">
            <span>FC reposo</span>
            <input
              className="input"
              type="number"
              min="20"
              max="250"
              value={progressForm.restingHeartRate}
              onChange={event => setProgressForm(form => ({ ...form, restingHeartRate: event.target.value }))}
            />
          </label>
          <label className="space-y-1 text-sm text-gray-600 md:col-span-5">
            <span>Notas</span>
            <input
              className="input"
              value={progressForm.notes}
              onChange={event => setProgressForm(form => ({ ...form, notes: event.target.value }))}
            />
          </label>
          <div className="flex items-end">
            <button className="btn-primary w-full" type="submit" disabled={savingProgress}>
              {savingProgress ? 'Guardando...' : 'Guardar'}
            </button>
          </div>
        </form>
      </section>

      <section className="card space-y-4">
        <div>
          <p className="text-sm text-gray-500">Seguimiento</p>
          <h2 className="text-lg font-semibold">Historial de mediciones</h2>
        </div>
        <ProgressHistoryTable
          records={progress}
          deletingId={deletingProgressId}
          onDelete={handleDeleteProgress}
        />
      </section>

      <div className="grid gap-4 xl:grid-cols-2">
        <section className="card space-y-4">
          <div className="flex items-center justify-between gap-3">
            <div>
              <p className="text-sm text-gray-500">Entrenamiento</p>
              <h2 className="text-lg font-semibold">Rutinas asociadas</h2>
            </div>
            <Link to={`/athletes/${id}/routines/new`} className="btn-primary text-xs">
              Nueva rutina
            </Link>
          </div>

          {routines.length === 0 ? (
            <EmptyState title="Sin rutinas" message="Este alumno aún no tiene rutinas asociadas." />
          ) : (
            <Table
              columns={[
                {
                  key: 'title',
                  label: 'Rutina',
                  render: routine => routine.title,
                },
                {
                  key: 'items',
                  label: 'Ejercicios',
                  render: routine => `${routine.items.length}`,
                },
                {
                  key: 'actions',
                  label: 'Acciones',
                  render: routine => (
                    <Link className="btn text-xs" to={`/athletes/${id}/routines/${routine.id}`}>
                      Ver
                    </Link>
                  ),
                },
              ]}
              data={routines}
              keyExtractor={routine => routine.id}
            />
          )}
        </section>

        <section className="card space-y-4">
          <div>
            <p className="text-sm text-gray-500">Agenda</p>
            <h2 className="text-lg font-semibold">Sesiones próximas o recientes</h2>
          </div>

          {sessions.length === 0 ? (
            <EmptyState title="Sin sesiones" message="No hay sesiones recientes o próximas para este alumno." />
          ) : (
            <div className="space-y-2">
              {[...sessions]
                .sort((a, b) => new Date(a.startAt).getTime() - new Date(b.startAt).getTime())
                .slice(0, 6)
                .map(session => (
                  <Link
                    key={session.id}
                    to={`/sessions/${session.id}`}
                    className="flex items-center justify-between gap-4 rounded-lg border border-gray-100 p-3 hover:bg-gray-50"
                  >
                    <div>
                      <p className="text-sm font-medium">{formatDateTime(session.startAt)}</p>
                      <p className="text-xs text-gray-500">{session.type}</p>
                    </div>
                    <span className="rounded-full bg-gray-100 px-2 py-1 text-xs text-gray-700">{session.status}</span>
                  </Link>
                ))}
            </div>
          )}
        </section>
      </div>
    </div>
  )
}
