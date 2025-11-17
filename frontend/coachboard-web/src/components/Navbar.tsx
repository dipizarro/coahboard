import { useAuthCtx } from '../auth/AuthContext'
export default function Navbar(){
const { user, logout } = useAuthCtx()
return (
<header className="border-b bg-white">
<div className="mx-auto flex max-w-7xl items-center justify-between p-4">
<div className="font-semibold">🏋️‍♀️ CoachBoard</div>
<div className="flex items-center gap-3 text-sm">
<span className="text-gray-600">{user?.email}</span>
<button className="btn" onClick={logout}>Salir</button>
</div>
</div>
</header>
)
}