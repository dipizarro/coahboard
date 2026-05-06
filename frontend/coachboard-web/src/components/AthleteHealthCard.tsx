import type { Athlete } from '../lib/types'

function field(value?: string | null) {
  return value?.trim() || '—'
}

export default function AthleteHealthCard({ athlete }: { athlete: Athlete }) {
  return (
    <section className="card space-y-4">
      <div>
        <p className="text-sm text-gray-500">Objetivo y salud</p>
        <h2 className="text-lg font-semibold">Contexto de entrenamiento</h2>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Objetivo principal</p>
          <p className="mt-1 text-sm text-gray-800">{field(athlete.mainGoal)}</p>
        </div>
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Experiencia</p>
          <p className="mt-1 text-sm text-gray-800">{field(athlete.experienceLevel)}</p>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Notas médicas</p>
          <p className="mt-1 whitespace-pre-wrap text-sm text-gray-800">{field(athlete.medicalNotes)}</p>
        </div>
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Lesiones</p>
          <p className="mt-1 whitespace-pre-wrap text-sm text-gray-800">{field(athlete.injuryNotes)}</p>
        </div>
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-gray-500">Notas internas</p>
          <p className="mt-1 whitespace-pre-wrap text-sm text-gray-800">{field(athlete.generalNotes)}</p>
        </div>
      </div>
    </section>
  )
}
