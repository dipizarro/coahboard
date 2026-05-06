import type { ClientProgressRecord } from '../lib/types'
import EmptyState from './EmptyState'
import Table from './Table'

function formatDate(value?: string | null) {
  return value ? new Date(value).toLocaleDateString('es-ES') : '—'
}

function measure(value?: number | null, unit = '') {
  return value == null ? '—' : `${value}${unit ? ` ${unit}` : ''}`
}

export default function ProgressHistoryTable({
  records,
  deletingId,
  onDelete,
}: {
  records: ClientProgressRecord[]
  deletingId?: number | null
  onDelete?: (record: ClientProgressRecord) => void
}) {
  if (records.length === 0) {
    return <EmptyState title="Sin mediciones" message="Registra la primera medición para comenzar el seguimiento." />
  }

  return (
    <Table
      columns={[
        {
          key: 'recordedAt',
          label: 'Fecha',
          render: record => formatDate(record.recordedAt),
        },
        {
          key: 'weightKg',
          label: 'Peso',
          render: record => measure(record.weightKg, 'kg'),
        },
        {
          key: 'bodyFatPercentage',
          label: 'Grasa',
          render: record => measure(record.bodyFatPercentage, '%'),
        },
        {
          key: 'waistCm',
          label: 'Cintura',
          render: record => measure(record.waistCm, 'cm'),
        },
        {
          key: 'restingHeartRate',
          label: 'FC reposo',
          render: record => measure(record.restingHeartRate, 'ppm'),
        },
        {
          key: 'notes',
          label: 'Notas',
          render: record => record.notes || '—',
        },
        {
          key: 'actions',
          label: 'Acciones',
          render: record =>
            onDelete ? (
              <button
                className="btn text-xs"
                disabled={deletingId === record.id}
                onClick={event => {
                  event.stopPropagation()
                  onDelete(record)
                }}
              >
                {deletingId === record.id ? 'Eliminando...' : 'Eliminar'}
              </button>
            ) : (
              '—'
            ),
        },
      ]}
      data={records}
      keyExtractor={record => record.id}
    />
  )
}
