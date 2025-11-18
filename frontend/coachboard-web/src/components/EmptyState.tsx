type EmptyStateProps = {
  title?: string
  message?: string
  actionLabel?: string
  onAction?: () => void
}

export default function EmptyState({
  title = 'No hay elementos',
  message = 'Aún no hay datos para mostrar.',
  actionLabel,
  onAction,
}: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <div className="mb-4 text-6xl opacity-20">📭</div>
      <h3 className="mb-2 text-lg font-semibold text-gray-700">{title}</h3>
      <p className="mb-4 text-sm text-gray-500">{message}</p>
      {actionLabel && onAction && (
        <button className="btn-primary" onClick={onAction}>
          {actionLabel}
        </button>
      )}
    </div>
  )
}

