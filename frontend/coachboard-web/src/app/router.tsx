import { Routes, Route, Navigate } from 'react-router-dom'
import Login from '../pages/Login'
import Dashboard from '../pages/Dashboard'
import AthletesList from '../pages/AthletesList'
import AthleteForm from '../pages/AthleteForm'
import Protected from './protected'
import Layout from './layout'


export default function AppRouter() {
return (
<Routes>
<Route path="/login" element={<Login />} />
<Route element={<Protected><Layout /></Protected>}>
<Route path="/" element={<Navigate to="/dashboard" replace />} />
<Route path="/dashboard" element={<Dashboard />} />
<Route path="/athletes" element={<AthletesList />} />
<Route path="/athletes/new" element={<AthleteForm />} />
<Route path="/athletes/:id" element={<AthleteForm />} />
</Route>
<Route path="*" element={<Navigate to="/dashboard" replace />} />
</Routes>
)
}