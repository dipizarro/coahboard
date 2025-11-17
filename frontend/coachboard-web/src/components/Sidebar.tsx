import { NavLink } from 'react-router-dom'
export default function Sidebar(){
const link = 'block rounded-xl px-3 py-2 text-sm hover:bg-gray-100'
const active = ({isActive}:{isActive:boolean})=> isActive? `${link} bg-gray-100 font-medium` : link
return (
<nav className="space-y-1">
<NavLink to="/dashboard" className={active}>Dashboard</NavLink>
<NavLink to="/athletes" className={active}>Atletas</NavLink>
</nav>
)
}