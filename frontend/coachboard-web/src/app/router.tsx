import { Routes, Route, Navigate } from 'react-router-dom'
import Login from '../pages/Login'
import Register from '../pages/Register'
import Dashboard from '../pages/Dashboard'
import AthletesList from '../pages/AthletesList'
import AthleteForm from '../pages/AthleteForm'
import AthleteProfile from '../pages/AthleteProfile'
import ExercisesList from '../pages/ExercisesList'
import ExerciseForm from '../pages/ExerciseForm'
import RoutinesList from '../pages/RoutinesList'
import RoutineForm from '../pages/RoutineForm'
import SessionsList from '../pages/SessionsList'
import SessionForm from '../pages/SessionForm'
import Protected from './protected'
import Layout from './layout'

export default function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />

      <Route
        element={
          <Protected>
            <Layout />
          </Protected>
        }
      >
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="/dashboard" element={<Dashboard />} />
        <Route path="/athletes" element={<AthletesList />} />
        <Route path="/athletes/new" element={<AthleteForm />} />
        <Route path="/athletes/:id/profile" element={<AthleteProfile />} />
        <Route path="/athletes/:id" element={<AthleteForm />} />
        <Route path="/athletes/:athleteId/routines" element={<RoutinesList />} />
        <Route path="/athletes/:athleteId/routines/new" element={<RoutineForm />} />
        <Route path="/athletes/:athleteId/routines/:routineId" element={<RoutineForm />} />
        <Route path="/exercises" element={<ExercisesList />} />
        <Route path="/exercises/new" element={<ExerciseForm />} />
        <Route path="/exercises/:id" element={<ExerciseForm />} />
        <Route path="/sessions" element={<SessionsList />} />
        <Route path="/sessions/new" element={<SessionForm />} />
        <Route path="/sessions/:id" element={<SessionForm />} />
      </Route>

      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}
