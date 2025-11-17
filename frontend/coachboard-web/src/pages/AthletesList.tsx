import { useEffect, useState } from 'react'
import { list, remove } from '../api/athletes'
import { Link } from 'react-router-dom'
import type { Athlete } from '../lib/types'

export default function AthletesList() {
  const [rows, setRows] = useState<Athlete[]>([])
  const [loading, setLoading] = useState(true)

  const refresh = (page=1) =>
    list({ page, pageSize: 20 })
      .then(res => setRows(res.items))
      .finally(() => setLoading(false))

  useEffect(() => { refresh() }, [])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Atletas</h1>
        <Link to="/athletes/new" className="btn-primary">Nuevo</Link>
      </div>

      <div className="card overflow-x-auto">
        {loading ? 'Cargando…' : rows.length === 0 ? 'Sin resultados' : (
          <table className="min-w-full text-sm">
            <thead>
              <tr className="text-left text-gray-500">
                <th className="p-2">Nombre</th>
                <th className="p-2">Email</th>
                <th className="p-2">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {rows.map(a => (
                <tr key={a.id} className="border-t">
                  <td className="p-2">{a.firstName} {a.lastName}</td>
                  <td className="p-2">{a.email ?? '—'}</td>
                  <td className="p-2 space-x-2">
                    <Link className="btn" to={`/athletes/${a.id}`}>Editar</Link>
                    <button className="btn" onClick={async()=>{ await remove(a.id); refresh() }}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
