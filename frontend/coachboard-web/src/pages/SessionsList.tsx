import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { list } from '../api/sessions'
import { useAuth } from '../auth/useAuth'
import type { Session, SessionStatus } from '../lib/types'
import Loader from '../components/Loader'
import EmptyState from '../components/EmptyState'

type ViewMode = 'calendar' | 'list'

export default function SessionsList() {
    const { coachId } = useAuth()
    const [sessions, setSessions] = useState<Session[]>([])
    const [loading, setLoading] = useState(true)
    const [viewMode, setViewMode] = useState<ViewMode>('calendar')
    const [currentDate, setCurrentDate] = useState(new Date())
    const [error, setError] = useState<string | null>(null)

    // Filtros
    const [filterStatus, setFilterStatus] = useState<SessionStatus | ''>('')

    // Calcular rango de fechas para la vista actual
    const getDateRange = () => {
        const year = currentDate.getFullYear()
        const month = currentDate.getMonth()

        // Primer día del mes
        const start = new Date(year, month, 1)
        // Último día del mes
        const end = new Date(year, month + 1, 0)

        // Ajustar para incluir días de la semana anterior/siguiente si es calendario
        // (Implementación simple por ahora: cargar mes completo + buffer)
        start.setDate(start.getDate() - 7)
        end.setDate(end.getDate() + 7)

        return { start, end }
    }

    const refresh = async () => {
        if (!coachId) return
        setLoading(true)
        setError(null)
        try {
            const { start, end } = getDateRange()
            const data = await list({
                coachId,
                from: start.toISOString(),
                to: end.toISOString()
            })
            setSessions(data)
        } catch (err) {
            console.error(err)
            setError('Error al cargar las sesiones')
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        refresh()
    }, [coachId, currentDate])

    const handlePrevMonth = () => {
        setCurrentDate(new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1))
    }

    const handleNextMonth = () => {
        setCurrentDate(new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1))
    }

    const filteredSessions = sessions.filter(s => {
        if (filterStatus && s.status !== filterStatus) return false
        return true
    })

    return (
        <div className="space-y-4">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <h1 className="text-2xl font-bold">Agenda</h1>
                <div className="flex gap-2">
                    <div className="flex bg-gray-100 rounded-lg p-1">
                        <button
                            className={`px-3 py-1 rounded-md text-sm ${viewMode === 'calendar' ? 'bg-white shadow text-blue-600' : 'text-gray-600'}`}
                            onClick={() => setViewMode('calendar')}
                        >
                            Calendario
                        </button>
                        <button
                            className={`px-3 py-1 rounded-md text-sm ${viewMode === 'list' ? 'bg-white shadow text-blue-600' : 'text-gray-600'}`}
                            onClick={() => setViewMode('list')}
                        >
                            Lista
                        </button>
                    </div>
                    <Link to="/sessions/new" className="btn-primary">
                        Nueva sesión
                    </Link>
                </div>
            </div>

            <div className="card space-y-4">
                {/* Controles de navegación y filtros */}
                <div className="flex flex-wrap items-center justify-between gap-4">
                    <div className="flex items-center gap-2">
                        <button onClick={handlePrevMonth} className="btn-icon">
                            &lt;
                        </button>
                        <h2 className="text-lg font-semibold w-40 text-center">
                            {currentDate.toLocaleString('es-ES', { month: 'long', year: 'numeric' })}
                        </h2>
                        <button onClick={handleNextMonth} className="btn-icon">
                            &gt;
                        </button>
                    </div>

                    <select
                        className="input w-40"
                        value={filterStatus}
                        onChange={e => setFilterStatus(e.target.value as SessionStatus | '')}
                    >
                        <option value="">Todos los estados</option>
                        <option value="Planned">Planificada</option>
                        <option value="Done">Realizada</option>
                        <option value="Canceled">Cancelada</option>
                        <option value="Missed">Perdida</option>
                    </select>
                </div>

                {error && <p className="text-red-600">{error}</p>}

                {loading ? (
                    <Loader text="Cargando agenda..." />
                ) : (
                    <>
                        {viewMode === 'calendar' ? (
                            <CalendarView sessions={filteredSessions} currentDate={currentDate} />
                        ) : (
                            <ListView sessions={filteredSessions} />
                        )}
                    </>
                )}
            </div>
        </div>
    )
}

function CalendarView({ sessions, currentDate }: { sessions: Session[], currentDate: Date }) {
    // Lógica básica de calendario grid
    const year = currentDate.getFullYear()
    const month = currentDate.getMonth()
    const firstDay = new Date(year, month, 1)
    const lastDay = new Date(year, month + 1, 0)

    // Días previos para rellenar la semana
    const startDayOfWeek = firstDay.getDay() === 0 ? 6 : firstDay.getDay() - 1 // Lunes = 0
    const daysInMonth = lastDay.getDate()

    const days = []
    // Relleno inicial
    for (let i = 0; i < startDayOfWeek; i++) {
        days.push(null)
    }
    // Días del mes
    for (let i = 1; i <= daysInMonth; i++) {
        days.push(new Date(year, month, i))
    }

    return (
        <div className="grid grid-cols-7 gap-1">
            {['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'].map(d => (
                <div key={d} className="text-center text-sm font-medium text-gray-500 py-2">
                    {d}
                </div>
            ))}
            {days.map((date, i) => {
                if (!date) return <div key={`empty-${i}`} className="bg-gray-50 h-24 rounded-lg" />

                const daySessions = sessions.filter(s => {
                    const sDate = new Date(s.startAt)
                    return sDate.getDate() === date.getDate() &&
                        sDate.getMonth() === date.getMonth() &&
                        sDate.getFullYear() === date.getFullYear()
                })

                return (
                    <div key={date.toISOString()} className="border rounded-lg h-24 p-1 overflow-y-auto bg-white hover:border-blue-300 transition-colors">
                        <div className="text-right text-xs text-gray-400 mb-1">{date.getDate()}</div>
                        <div className="space-y-1">
                            {daySessions.map(s => (
                                <Link
                                    key={s.id}
                                    to={`/sessions/${s.id}`}
                                    className={`block text-xs p-1 rounded truncate ${s.status === 'Done' ? 'bg-green-100 text-green-800' :
                                            s.status === 'Canceled' ? 'bg-red-100 text-red-800' :
                                                'bg-blue-100 text-blue-800'
                                        }`}
                                    title={`${new Date(s.startAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - ${s.clientName}`}
                                >
                                    {new Date(s.startAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} {s.clientName}
                                </Link>
                            ))}
                        </div>
                    </div>
                )
            })}
        </div>
    )
}

function ListView({ sessions }: { sessions: Session[] }) {
    if (sessions.length === 0) {
        return <EmptyState title="No hay sesiones" message="No hay sesiones para este periodo." />
    }

    // Ordenar por fecha
    const sorted = [...sessions].sort((a, b) => new Date(a.startAt).getTime() - new Date(b.startAt).getTime())

    return (
        <div className="space-y-2">
            {sorted.map(s => (
                <Link
                    key={s.id}
                    to={`/sessions/${s.id}`}
                    className="flex items-center justify-between p-3 border rounded-lg hover:bg-gray-50 transition-colors"
                >
                    <div className="flex items-center gap-4">
                        <div className={`w-2 h-12 rounded-full ${s.status === 'Done' ? 'bg-green-500' :
                                s.status === 'Canceled' ? 'bg-red-500' :
                                    'bg-blue-500'
                            }`} />
                        <div>
                            <p className="font-medium">{s.clientName || 'Sin cliente'}</p>
                            <p className="text-sm text-gray-500">
                                {new Date(s.startAt).toLocaleDateString()} {new Date(s.startAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                - {s.type}
                            </p>
                        </div>
                    </div>
                    <div className="text-sm font-medium px-2 py-1 rounded bg-gray-100">
                        {s.status}
                    </div>
                </Link>
            ))}
        </div>
    )
}
