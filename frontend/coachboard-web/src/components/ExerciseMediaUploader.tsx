import { useEffect, useState, type FormEvent } from 'react'
import { list, remove, upload } from '../api/exerciseMedia'
import type { ExerciseMedia } from '../lib/types'
import EmptyState from './EmptyState'
import Loader from './Loader'

type ExerciseMediaUploaderProps = {
  exerciseId?: number | null
  canManage: boolean
}

function textOrNull(value: string) {
  const trimmed = value.trim()
  return trimmed ? trimmed : null
}

export default function ExerciseMediaUploader({ exerciseId, canManage }: ExerciseMediaUploaderProps) {
  const [media, setMedia] = useState<ExerciseMedia[]>([])
  const [file, setFile] = useState<File | null>(null)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [error, setError] = useState<string | null>(null)

  async function refresh() {
    if (!exerciseId) return

    setLoading(true)
    setError(null)
    try {
      setMedia(await list(exerciseId))
    } catch {
      setError('No se pudo cargar la galería del ejercicio.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    refresh()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [exerciseId])

  async function handleUpload(event: FormEvent) {
    event.preventDefault()
    if (!exerciseId || !file) return

    setSaving(true)
    setError(null)
    try {
      const created = await upload(exerciseId, {
        file,
        title: textOrNull(title),
        description: textOrNull(description),
      })
      setMedia(items => [created, ...items])
      setFile(null)
      setTitle('')
      setDescription('')
    } catch (err: any) {
      const msg = err?.response?.data ?? 'No se pudo subir la imagen.'
      setError(typeof msg === 'string' ? msg : 'No se pudo subir la imagen.')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(item: ExerciseMedia) {
    if (!exerciseId || !window.confirm('¿Eliminar imagen?')) return

    setDeletingId(item.id)
    setError(null)
    try {
      await remove(exerciseId, item.id)
      setMedia(items => items.filter(current => current.id !== item.id))
    } catch (err: any) {
      const msg = err?.response?.data ?? 'No se pudo eliminar la imagen.'
      setError(typeof msg === 'string' ? msg : 'No se pudo eliminar la imagen.')
    } finally {
      setDeletingId(null)
    }
  }

  if (!exerciseId) {
    return (
      <section className="card space-y-2">
        <p className="text-sm text-gray-500">Galería</p>
        <h2 className="text-lg font-semibold">Imágenes del ejercicio</h2>
        <p className="text-sm text-gray-500">Guarda el ejercicio antes de subir imágenes.</p>
      </section>
    )
  }

  return (
    <section className="card space-y-4">
      <div>
        <p className="text-sm text-gray-500">Galería</p>
        <h2 className="text-lg font-semibold">Imágenes del ejercicio</h2>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      {canManage && (
        <form onSubmit={handleUpload} className="grid gap-3 md:grid-cols-6">
          <label className="space-y-1 text-sm text-gray-600 md:col-span-2">
            <span>Imagen</span>
            <input
              className="input"
              type="file"
              accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
              onChange={event => setFile(event.target.files?.[0] ?? null)}
            />
          </label>
          <label className="space-y-1 text-sm text-gray-600 md:col-span-2">
            <span>Título</span>
            <input className="input" value={title} onChange={event => setTitle(event.target.value)} />
          </label>
          <label className="space-y-1 text-sm text-gray-600 md:col-span-2">
            <span>Descripción</span>
            <input className="input" value={description} onChange={event => setDescription(event.target.value)} />
          </label>
          <div className="md:col-span-6">
            <button className="btn-primary" type="submit" disabled={!file || saving}>
              {saving ? 'Subiendo...' : 'Subir imagen'}
            </button>
          </div>
        </form>
      )}

      {loading ? (
        <Loader text="Cargando imágenes..." />
      ) : media.length === 0 ? (
        <EmptyState title="Sin imágenes" message="Este ejercicio aún no tiene imágenes asociadas." />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {media.map(item => (
            <article key={item.id} className="overflow-hidden rounded-lg border border-gray-100 bg-white">
              <img src={item.url} alt={item.title || 'Imagen del ejercicio'} className="aspect-[4/3] w-full object-cover" />
              <div className="space-y-2 p-3">
                <div>
                  <h3 className="text-sm font-semibold text-gray-900">{item.title || 'Imagen'}</h3>
                  {item.description && <p className="text-sm text-gray-500">{item.description}</p>}
                </div>
                {canManage && (
                  <button className="btn text-xs" disabled={deletingId === item.id} onClick={() => handleDelete(item)}>
                    {deletingId === item.id ? 'Eliminando...' : 'Eliminar'}
                  </button>
                )}
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  )
}
