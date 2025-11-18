import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import Navbar from '../components/Navbar'
import Sidebar from '../components/Sidebar'

export default function Layout() {
  const [sidebarOpen, setSidebarOpen] = useState(false)

  return (
    <div className="min-h-dvh bg-gray-50">
      <Navbar onMenuClick={() => setSidebarOpen(!sidebarOpen)} />
      <div className="mx-auto max-w-7xl gap-6 p-4 md:grid md:grid-cols-[240px_1fr]">
        <aside
          className={`card fixed inset-y-0 left-0 z-40 w-64 transform bg-white p-4 transition-transform md:relative md:sticky md:top-4 md:z-auto md:transform-none ${
            sidebarOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0'
          }`}
        >
          <Sidebar onClose={() => setSidebarOpen(false)} />
        </aside>
        {sidebarOpen && (
          <div
            className="fixed inset-0 z-30 bg-black/50 md:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}
        <main className="space-y-4 md:col-start-2">
          <Outlet />
        </main>
      </div>
    </div>
  )
}