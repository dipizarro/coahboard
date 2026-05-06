Antes de agregar progreso, conviene corregir los hallazgos.

Objetivo

Cerrar brechas actuales para que un coach no pueda acceder o modificar datos de otro coach.

Tareas
Corregir RoutinesController para validar que el ClientId pertenece al coach autenticado.
Validar en GET /api/routines/{id} que la rutina pertenece a un cliente del coach logueado.
Validar en PUT /api/routines/{id} y DELETE /api/routines/{id} que el coach tiene permiso.
Revisar ExercisesController: decidir si los ejercicios son globales o por coach.
Limpiar CORS antiguo de Azure o moverlo a configuración.
Crear endpoint futuro de dashboard real o dejar issue pendiente.
Entregable

Backend más seguro para entorno multi-coach.
