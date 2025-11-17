import { Outlet } from 'react-router-dom'
import Navbar from '../components/Navbar'
import Sidebar from '../components/Sidebar'


export default function Layout() {
return (
<div className="min-h-dvh bg-gray-50">
<Navbar />
<div className="mx-auto grid max-w-7xl grid-cols-1 gap-6 p-4 md:grid-cols-[240px_1fr]">
<aside className="card md:sticky md:top-4"><Sidebar /></aside>
<main className="space-y-4"><Outlet /></main>
</div>
</div>
)
}