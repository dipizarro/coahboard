import type { ClientProgressSummary } from '../lib/types'

function formatDate(value?: string | null) {
  return value ? new Date(value).toLocaleDateString('es-ES') : '—'
}

function measure(value?: number | null, unit = '') {
  return value == null ? '—' : `${value}${unit ? ` ${unit}` : ''}`
}

function change(value?: number | null, unit = '') {
  if (value == null) return '—'
  const prefix = value > 0 ? '+' : ''
  return `${prefix}${value}${unit ? ` ${unit}` : ''}`
}

function changeColor(value?: number | null) {
  if (value == null || value === 0) return 'text-gray-700'
  return value < 0 ? 'text-green-700' : 'text-amber-700'
}

export default function ProgressSummaryCard({ summary }: { summary?: ClientProgressSummary | null }) {
  if (!summary || summary.totalRecords === 0) {
    return (
      <section className="card space-y-2">
        <p className="text-sm text-gray-500">Evolución</p>
        <h2 className="text-lg font-semibold">Sin indicadores</h2>
        <p className="text-sm text-gray-500">Registra al menos una medición para ver el resumen de progreso.</p>
      </section>
    )
  }

  const metrics = [
    {
      label: 'Peso',
      initial: measure(summary.initialWeightKg, 'kg'),
      current: measure(summary.currentWeightKg, 'kg'),
      delta: change(summary.weightChangeKg, 'kg'),
      deltaClass: changeColor(summary.weightChangeKg),
    },
    {
      label: 'Cintura',
      initial: measure(summary.initialWaistCm, 'cm'),
      current: measure(summary.currentWaistCm, 'cm'),
      delta: change(summary.waistChangeCm, 'cm'),
      deltaClass: changeColor(summary.waistChangeCm),
    },
    {
      label: 'Grasa corporal',
      initial: measure(summary.initialBodyFatPercentage, '%'),
      current: measure(summary.currentBodyFatPercentage, '%'),
      delta: change(summary.bodyFatChangePercentage, '%'),
      deltaClass: changeColor(summary.bodyFatChangePercentage),
    },
  ]

  return (
    <section className="card space-y-4">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <p className="text-sm text-gray-500">Evolución</p>
          <h2 className="text-lg font-semibold">{summary.totalRecords} mediciones</h2>
        </div>
        <div className="text-sm text-gray-500 sm:text-right">
          <p>{formatDate(summary.firstRecordDate)} - {formatDate(summary.lastRecordDate)}</p>
          <p>{summary.daysSinceStart ?? 0} días</p>
        </div>
      </div>

      <div className="grid gap-3 md:grid-cols-3">
        {metrics.map(metric => (
          <div key={metric.label} className="rounded-lg border border-gray-100 p-3">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">{metric.label}</p>
            <div className="mt-2 grid grid-cols-3 gap-2 text-sm">
              <div>
                <p className="text-xs text-gray-500">Inicial</p>
                <p className="font-medium text-gray-800">{metric.initial}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Actual</p>
                <p className="font-medium text-gray-800">{metric.current}</p>
              </div>
              <div>
                <p className="text-xs text-gray-500">Cambio</p>
                <p className={`font-semibold ${metric.deltaClass}`}>{metric.delta}</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      <p className="text-xs text-gray-500">Última actualización: {formatDate(summary.lastUpdatedAt)}</p>
    </section>
  )
}
