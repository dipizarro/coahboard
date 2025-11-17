import { useEffect, useState } from 'react'
import { list } from '../api/athletes'

export default function Dashboard() {
  const [count, setCount] = useState<number | null>(null)

  useEffect(() => {
    list({ page: 1, pageSize: 1 })
      .then((res)=> setCount(res.total))
      .catch(()=> setCount(0))
  }, [])

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Dashboard</h1>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <div className="card">
          <p className="text-sm text-gray-500">Atletas</p>
          <p className="text-3xl font-semibold">{count ?? '—'}</p>
        </div>
      </div>
    </div>
  )
}
