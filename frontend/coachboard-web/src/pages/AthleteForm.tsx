import { useEffect, useState } from 'react'
import { create, get, update } from '../api/athletes'
import { useNavigate, useParams } from 'react-router-dom'


export default function AthleteForm() {
const { id } = useParams()
const nav = useNavigate()
const [form, setForm] = useState({ firstName: '', lastName: '', email: '' })


useEffect(() => {
if (id) get(id).then(a => setForm({ firstName: a.firstName, lastName: a.lastName, email: a.email ?? '' }))
}, [id])


async function onSubmit(e: React.FormEvent) {
e.preventDefault()
if (id) await update(id, form)
else await create(form)
nav('/athletes')
}


return (
<form onSubmit={onSubmit} className="card space-y-3">
<h1 className="text-2xl font-bold">{id ? 'Editar' : 'Nuevo'} atleta</h1>
<input className="input" placeholder="Nombre" value={form.firstName} onChange={e=>setForm(f=>({...f, firstName: e.target.value}))} />
<input className="input" placeholder="Apellido" value={form.lastName} onChange={e=>setForm(f=>({...f, lastName: e.target.value}))} />
<input className="input" placeholder="Email" value={form.email} onChange={e=>setForm(f=>({...f, email: e.target.value}))} />
<div className="flex gap-2">
<button className="btn-primary" type="submit">Guardar</button>
<button className="btn" type="button" onClick={()=> nav(-1)}>Cancelar</button>
</div>
</form>
)
}