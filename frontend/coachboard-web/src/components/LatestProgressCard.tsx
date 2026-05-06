import type { ClientProgressRecord } from '../lib/types'

function formatDate(value?: string | null) {
  return value ? new Date(value).toLocaleDateString('es-ES') : '—'
}

function measure(value?: number | null, unit = '') {
  return value == null ? '—' : `${value}${unit ? ` ${unit}` : ''}`
}

export default function LatestProgressCard({ record }: { record?: ClientProgressRecord | null }) {
  if (!record) {
    return (
      <section className="card space-y-2">
        <p className="text-sm text-gray-500">Última medición</p>
        <h2 className="text-lg font-semibold">Sin registros</h2>
        <p className="text-sm text-gray-500">Aún no hay mediciones de progreso para este alumno.</p>
      </section>
    )
  }

  const metrics = [
    ['Peso', measure(record.weightKg, 'kg')],
    ['Grasa corporal', measure(record.bodyFatPercentage, '%')],
    ['Cintura', measure(record.waistCm, 'cm')],
    ['FC reposo', measure(record.restingHeartRate, 'ppm')],
  ]

  return (
    <section className="card space-y-4">
      <div>
        <p className="text-sm text-gray-500">Última medición</p>
        <h2 className="text-lg font-semibold">{formatDate(record.recordedAt)}</h2>
      </div>

      <div className="grid grid-cols-2 gap-3">
        {metrics.map(([label, value]) => (
          <div key={label} className="rounded-lg border border-gray-100 p-3">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">{label}</p>
            <p className="mt-1 text-lg font-semibold text-gray-900">{value}</p>
          </div>
        ))}
      </div>

      {record.notes && <p className="whitespace-pre-wrap text-sm text-gray-600">{record.notes}</p>}
    </section>
  )
}
