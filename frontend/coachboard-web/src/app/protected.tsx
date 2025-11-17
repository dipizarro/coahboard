import { Navigate, useLocation } from 'react-router-dom'
import { useAuthCtx } from '../auth/AuthContext'


export default function Protected({ children }: { children: React.ReactNode }) {
const { token } = useAuthCtx()
const location = useLocation()
if (!token) return <Navigate to="/login" state={{ from: location }} replace />
return <>{children}</>
}