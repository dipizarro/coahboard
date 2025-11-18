type Column<T> = {
  key: keyof T | string
  label: string
  render?: (item: T) => React.ReactNode
}

type TableProps<T> = {
  columns: Column<T>[]
  data: T[]
  keyExtractor: (item: T) => string | number
  onRowClick?: (item: T) => void
}

export default function Table<T extends Record<string, any>>({
  columns,
  data,
  keyExtractor,
  onRowClick,
}: TableProps<T>) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="border-b bg-gray-50">
            {columns.map(col => (
              <th key={String(col.key)} className="p-2 text-left font-medium text-gray-700">
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map(item => {
            const key = keyExtractor(item)
            return (
              <tr
                key={key}
                className={`border-b last:border-none transition-colors ${
                  onRowClick ? 'cursor-pointer hover:bg-gray-50' : ''
                }`}
                onClick={() => onRowClick?.(item)}
              >
                {columns.map(col => (
                  <td key={String(col.key)} className="p-2">
                    {col.render ? col.render(item) : String(item[col.key] ?? '—')}
                  </td>
                ))}
              </tr>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}

