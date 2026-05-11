import { Link } from 'react-router-dom'
import type { Exercise } from '../lib/types'
import { resolveAssetUrl } from '../lib/assets'

type ExerciseCardProps = {
  exercise: Exercise
  canEdit: boolean
  canDelete: boolean
  deleting: boolean
  onDelete: (exerciseId: number) => void
}

function iconPath(kind: 'category' | 'muscle' | 'equipment' | 'difficulty' | 'image') {
  if (kind === 'category') return <><path d="M4 7h16" /><path d="M4 12h10" /><path d="M4 17h16" /></>
  if (kind === 'muscle') return <><path d="M6 15c2-5 5-7 9-7" /><path d="M9 18c2-4 5-6 9-6" /><path d="M5 19c4 1 8 1 12-1" /></>
  if (kind === 'equipment') return <><path d="M5 9v6" /><path d="M19 9v6" /><path d="M8 12h8" /><path d="M3 10v4" /><path d="M21 10v4" /></>
  if (kind === 'image') return <><rect x="4" y="5" width="16" height="14" rx="2" /><path d="m8 15 2.5-3 2 2.5 1.5-1.5 2 2" /><path d="M9 9h.01" /></>
  return <><path d="M4 20h16" /><path d="M6 16l4-4 3 3 5-7" /><path d="M18 8h-4" /><path d="M18 8v4" /></>
}

function Metric({ kind, label, value }: { kind: 'category' | 'muscle' | 'equipment' | 'difficulty'; label: string; value?: string | null }) {
  return (
    <div className="flex items-center gap-2 rounded-lg border border-gray-100 bg-gray-50 p-2">
      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-white text-primary-700">
        <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="1.8">
          {iconPath(kind)}
        </svg>
      </span>
      <div className="min-w-0">
        <p className="text-xs text-gray-500">{label}</p>
        <p className="truncate text-sm font-medium text-gray-800">{value || '-'}</p>
      </div>
    </div>
  )
}

function tags(value?: string | null) {
  return (value ?? '')
    .split(',')
    .map(tag => tag.trim())
    .filter(Boolean)
    .slice(0, 3)
}

export default function ExerciseCard({
  exercise,
  canEdit,
  canDelete,
  deleting,
  onDelete,
}: ExerciseCardProps) {
  const mainTags = tags(exercise.tags)
  const prescription = [
    exercise.defaultSets ? `${exercise.defaultSets} series` : null,
    exercise.defaultReps ? `${exercise.defaultReps} reps` : null,
  ].filter(Boolean).join(' · ')

  return (
    <article className="flex h-full flex-col rounded-lg border border-gray-100 bg-white p-4 shadow-sm transition hover:-translate-y-0.5 hover:border-primary-100 hover:shadow-md">
      <div className="mb-4 aspect-[16/9] overflow-hidden rounded-lg bg-gray-100">
        {exercise.imageUrl ? (
          <img src={resolveAssetUrl(exercise.imageUrl)} alt={exercise.name} className="h-full w-full object-cover" loading="lazy" />
        ) : (
          <div className="flex h-full flex-col items-center justify-center gap-2 bg-primary-50 text-primary-700">
            <svg viewBox="0 0 24 24" className="h-10 w-10" fill="none" stroke="currentColor" strokeWidth="1.8">
              {iconPath(exercise.targetMuscleGroup ? 'muscle' : 'image')}
            </svg>
            <p className="text-sm font-medium">{exercise.targetMuscleGroup || exercise.category}</p>
          </div>
        )}
      </div>

      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="mb-2 flex flex-wrap gap-2">
            <span
              className={`rounded-full px-2 py-1 text-xs ${
                exercise.isGlobal ? 'bg-blue-100 text-blue-700' : 'bg-emerald-100 text-emerald-700'
              }`}
            >
              {exercise.isGlobal ? 'Sistema' : 'Propio'}
            </span>
            {!exercise.isActive && <span className="rounded-full bg-gray-100 px-2 py-1 text-xs text-gray-600">Inactivo</span>}
          </div>
          <h2 className="line-clamp-2 text-lg font-semibold text-gray-900">{exercise.name}</h2>
          {exercise.description && <p className="mt-1 line-clamp-2 text-sm text-gray-500">{exercise.description}</p>}
        </div>
      </div>

      <div className="mt-4 grid gap-2 sm:grid-cols-2">
        <Metric kind="category" label="Categoría" value={exercise.category} />
        <Metric kind="muscle" label="Músculo" value={exercise.targetMuscleGroup} />
        <Metric kind="equipment" label="Equipo" value={exercise.equipment} />
        <Metric kind="difficulty" label="Dificultad" value={exercise.difficultyLevel} />
      </div>

      <div className="mt-3 flex flex-wrap gap-2 text-xs">
        {exercise.environment && <span className="rounded-full bg-gray-100 px-2 py-1 text-gray-700">{exercise.environment}</span>}
        {mainTags.map(tag => (
          <span key={tag} className="rounded-full bg-primary-50 px-2 py-1 text-primary-700">
            {tag}
          </span>
        ))}
      </div>

      {prescription && (
        <p className="mt-4 rounded-lg bg-gray-50 px-3 py-2 text-sm font-medium text-gray-700">{prescription}</p>
      )}

      <div className="mt-auto flex flex-wrap gap-2 pt-4">
        {exercise.videoUrl && (
          <a className="btn text-xs" href={exercise.videoUrl} target="_blank" rel="noopener noreferrer">
            Ver video
          </a>
        )}
        {exercise.referenceUrl && (
          <a className="btn text-xs" href={exercise.referenceUrl} target="_blank" rel="noopener noreferrer">
            Ver referencia
          </a>
        )}
        {canEdit ? (
          <Link className="btn-primary text-xs" to={`/exercises/${exercise.id}`}>
            Ver detalle
          </Link>
        ) : (
          <Link className="btn text-xs" to={`/exercises/${exercise.id}`}>
            Ver detalle
          </Link>
        )}
        {canEdit && (
          <Link className="btn text-xs" to={`/exercises/${exercise.id}`}>
            Editar
          </Link>
        )}
        {canDelete && (
          <button className="btn text-xs" disabled={deleting} onClick={() => onDelete(exercise.id)}>
            {deleting ? 'Eliminando...' : 'Eliminar'}
          </button>
        )}
      </div>
    </article>
  )
}
