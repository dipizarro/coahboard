import { NavLink } from 'react-router-dom'

type SidebarProps = {
  onClose?: () => void
}

export default function Sidebar({ onClose }: SidebarProps) {
  const link = 'block rounded-xl px-3 py-2 text-sm hover:bg-gray-100 transition-colors'
  const active = ({ isActive }: { isActive: boolean }) =>
    isActive ? `${link} bg-gray-100 font-medium` : link

  return (
    <nav className="space-y-1">
      <div className="mb-4 flex items-center justify-between md:hidden">
        <h2 className="font-semibold">Menú</h2>
        <button
          onClick={onClose}
          className="rounded-lg p-1 hover:bg-gray-100"
          aria-label="Cerrar menú"
        >
          <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>
      </div>
      <NavLink to="/dashboard" className={active} onClick={onClose}>
        Dashboard
      </NavLink>
      <NavLink to="/athletes" className={active} onClick={onClose}>
        Atletas
      </NavLink>
      <NavLink to="/exercises" className={active} onClick={onClose}>
        Ejercicios
      </NavLink>
    </nav>
  )
}