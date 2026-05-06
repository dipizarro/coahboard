import type { Athlete } from '../lib/types'

function formatDate(value?: string | null) {
  return value ? new Date(value).toLocaleDateString('es-ES') : '—'
}

function fullName(athlete: Athlete) {
  return `${athlete.firstName} ${athlete.lastName}`.trim() || 'Atleta'
}

export default function AthleteSummaryCard({ athlete }: { athlete: Athlete }) {
  const rows = [
    ['Email', athlete.email || '—'],
    ['Teléfono', athlete.phone || '—'],
    ['Nacimiento', formatDate(athlete.birthDate)],
    ['Género', athlete.gender || '—'],
    ['Inicio', formatDate(athlete.startDate)],
    ['Estatura inicial', athlete.initialHeightCm ? `${athlete.initialHeightCm} cm` : '—'],
  ]

  return (
    <section className="card space-y-4">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="text-sm text-gray-500">Ficha del alumno</p>
          <h2 className="text-xl font-semibold">{fullName(athlete)}</h2>
        </div>
        <span
          className={`w-fit rounded-full px-3 py-1 text-xs font-medium ${
            athlete.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'
          }`}
        >
          {athlete.isActive ? 'Activo' : 'Inactivo'}
        </span>
      </div>

      <dl className="grid gap-3 sm:grid-cols-2">
        {rows.map(([label, value]) => (
          <div key={label}>
            <dt className="text-xs font-medium uppercase tracking-wide text-gray-500">{label}</dt>
            <dd className="mt-1 text-sm text-gray-800">{value}</dd>
          </div>
        ))}
      </dl>
    </section>
  )
}
