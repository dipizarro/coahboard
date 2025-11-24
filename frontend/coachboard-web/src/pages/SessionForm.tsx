import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { create, get, update, remove, updateStatus } from '../api/sessions'
import { list as listAthletes } from '../api/athletes'
import { list as listRoutines } from '../api/routines'
import type { SessionStatus, SessionType, Athlete, Routine } from '../lib/types'
import Loader from '../components/Loader'

export default function SessionForm() {
    const { id } = useParams()
    const navigate = useNavigate()
    const { coachId } = useAuth()
    const isEditing = Boolean(id)

    const [loading, setLoading] = useState(isEditing)
    const [saving, setSaving] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const [athletes, setAthletes] = useState<Athlete[]>([])
    const [routines, setRoutines] = useState<Routine[]>([])

    // Form State
    const [clientId, setClientId] = useState<string>('')
    const [routineId, setRoutineId] = useState<string>('')
    const [date, setDate] = useState(new Date().toISOString().split('T')[0])
    const [startTime, setStartTime] = useState('09:00')
    const [endTime, setEndTime] = useState('10:00')
    const [type, setType] = useState<SessionType>('Training')
    const [location, setLocation] = useState('')
    const [notes, setNotes] = useState('')
    const [status, setStatus] = useState<SessionStatus>('Planned')

    // Helpers para manejo de fechas locales
    const toLocalDateInputValue = (date: Date) => {
        const y = date.getFullYear()
        const m = String(date.getMonth() + 1).padStart(2, '0')
        const d = String(date.getDate()).padStart(2, '0')
        return `${y}-${m}-${d}`
    }

    const toLocalTimeInputValue = (date: Date) => {
        const h = String(date.getHours()).padStart(2, '0')
        const m = String(date.getMinutes()).padStart(2, '0')
        return `${h}:${m}`
    }

    // Cargar datos iniciales
    useEffect(() => {
        if (!coachId) return

        // Cargar atletas
        listAthletes({ coachId, pageSize: 100 }).then(res => {
            setAthletes(res.items)
        })

        if (isEditing && id) {
            get(Number(id))
                .then(session => {
                    setClientId(String(session.clientId || ''))
                    setRoutineId(String(session.routineId || ''))

                    const start = new Date(session.startAt)
                    const end = new Date(session.endAt)

                    // Usar métodos locales para llenar los inputs
                    setDate(toLocalDateInputValue(start))
                    setStartTime(toLocalTimeInputValue(start))
                    setEndTime(toLocalTimeInputValue(end))

                    setType(session.type)
                    setLocation(session.location || '')
                    setNotes(session.notes || '')
                    setStatus(session.status)
                })
                .catch(() => setError('Error al cargar la sesión'))
                .finally(() => setLoading(false))
        }
    }, [coachId, id, isEditing])

    // Cargar rutinas cuando cambia el atleta
    useEffect(() => {
        if (!clientId) {
            setRoutines([])
            return
        }
        listRoutines({ clientId: Number(clientId), page: 1, pageSize: 50 }).then(res => {
            setRoutines(res.items)
        }).catch(() => setRoutines([]))

    }, [clientId])

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        if (!coachId) return
        setSaving(true)
        setError(null)

        try {
            // Construir fechas como strings ISO locales (sin conversión a UTC del navegador)
            // Esto asegura que si el usuario pone 13:00, se envíe "2025-11-25T13:00:00"
            const startAt = `${date}T${startTime}:00`
            const endAt = `${date}T${endTime}:00`

            const payload = {
                coachId,
                clientId: clientId ? Number(clientId) : null,
                routineId: routineId ? Number(routineId) : null,
                startAt,
                endAt,
                type,
                location,
                notes,
                status
            }

            if (isEditing && id) {
                await update(Number(id), payload)
            } else {
                await create({
                    coachId,
                    clientId: clientId ? Number(clientId) : null,
                    routineId: routineId ? Number(routineId) : null,
                    startAt,
                    endAt,
                    type,
                    location,
                    notes
                })
            }
            navigate('/sessions')
        } catch (error) {
            console.error(error)
            setError('Error al guardar la sesión: Verifique los datos de fecha y hora.')
        } finally {
            setSaving(false)
        }
    }

    const handleDelete = async () => {
        if (!window.confirm('¿Eliminar esta sesión?')) return
        if (!id) return
        setSaving(true)
        try {
            await remove(Number(id))
            navigate('/sessions')
        } catch (err) {
            setError('Error al eliminar')
            setSaving(false)
        }
    }

    const handleStatusChange = async (newStatus: SessionStatus) => {
        if (!id) return;
        try {
            await updateStatus(Number(id), newStatus)
            setStatus(newStatus)
        } catch (err) {
            alert('Error al actualizar estado')
        }
    }

    if (loading) return <Loader text="Cargando sesión..." />

    return (
        <div className="max-w-2xl mx-auto space-y-6">
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold">{isEditing ? 'Editar Sesión' : 'Nueva Sesión'}</h1>
                {isEditing && (
                    <div className="flex gap-2">
                        {status !== 'Done' && (
                            <button type="button" onClick={() => handleStatusChange('Done')} className="btn bg-green-100 text-green-700 hover:bg-green-200">
                                Marcar Realizada
                            </button>
                        )}
                        {status !== 'Canceled' && (
                            <button type="button" onClick={() => handleStatusChange('Canceled')} className="btn bg-red-100 text-red-700 hover:bg-red-200">
                                Cancelar
                            </button>
                        )}
                    </div>
                )}
            </div>

            <form onSubmit={handleSubmit} className="card space-y-4">
                {error && <p className="text-red-600">{error}</p>}

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                    <div className="space-y-1">
                        <label className="label">Atleta</label>
                        <select
                            className="input"
                            value={clientId}
                            onChange={e => setClientId(e.target.value)}
                            disabled={isEditing}
                        >
                            <option value="">Seleccionar atleta...</option>
                            {athletes.map(a => (
                                <option key={a.id} value={a.id}>
                                    {a.firstName} {a.lastName}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="space-y-1">
                        <label className="label">Rutina (Opcional)</label>
                        <select
                            className="input"
                            value={routineId}
                            onChange={e => setRoutineId(e.target.value)}
                            disabled={!clientId}
                        >
                            <option value="">Sin rutina</option>
                            {routines.map(r => (
                                <option key={r.id} value={r.id}>
                                    {r.title}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div className="space-y-1">
                        <label className="label">Fecha</label>
                        <input
                            type="date"
                            className="input"
                            value={date}
                            onChange={e => setDate(e.target.value)}
                            required
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-2">
                        <div className="space-y-1">
                            <label className="label">Inicio</label>
                            <input
                                type="time"
                                className="input"
                                value={startTime}
                                onChange={e => setStartTime(e.target.value)}
                                required
                            />
                        </div>
                        <div className="space-y-1">
                            <label className="label">Fin</label>
                            <input
                                type="time"
                                className="input"
                                value={endTime}
                                onChange={e => setEndTime(e.target.value)}
                                required
                            />
                        </div>
                    </div>

                    <div className="space-y-1">
                        <label className="label">Tipo</label>
                        <select
                            className="input"
                            value={type}
                            onChange={e => setType(e.target.value as SessionType)}
                        >
                            <option value="Training">Entrenamiento</option>
                            <option value="PersonalBlock">Bloque Personal</option>
                            <option value="Other">Otro</option>
                        </select>
                    </div>

                    <div className="space-y-1">
                        <label className="label">Ubicación</label>
                        <input
                            type="text"
                            className="input"
                            value={location}
                            onChange={e => setLocation(e.target.value)}
                            placeholder="Ej. Gym Central"
                        />
                    </div>
                </div>

                <div className="space-y-1">
                    <label className="label">Notas</label>
                    <textarea
                        className="input min-h-[100px]"
                        value={notes}
                        onChange={e => setNotes(e.target.value)}
                        placeholder="Detalles de la sesión..."
                    />
                </div>

                <div className="flex items-center justify-between pt-4 border-t">
                    {isEditing ? (
                        <button type="button" onClick={handleDelete} className="btn text-red-600 hover:bg-red-50">
                            Eliminar Sesión
                        </button>
                    ) : (
                        <div />
                    )}
                    <div className="flex gap-2">
                        <button type="button" onClick={() => navigate('/sessions')} className="btn">
                            Cancelar
                        </button>
                        <button type="submit" className="btn-primary" disabled={saving}>
                            {saving ? 'Guardando...' : 'Guardar'}
                        </button>
                    </div>
                </div>
            </form>
        </div>
    )
}
