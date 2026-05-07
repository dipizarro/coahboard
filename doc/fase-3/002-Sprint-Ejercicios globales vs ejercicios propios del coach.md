Ejercicios globales vs ejercicios propios del coach

Este punto es importante para el producto.

Hoy los ejercicios parecen globales: cualquier coach puede crearlos, y todos podrían verlos. Eso puede ser riesgoso porque un coach podría llenar el catálogo de otro.

Para un SaaS real hay dos caminos:

Opción A — Biblioteca global controlada por admin
Admin crea ejercicios base.
Coach solo los usa.
Coach no edita ejercicios globales.
Opción B — Biblioteca mixta
Ejercicios globales del sistema.
Ejercicios privados de cada coach.

Yo elegiría la opción B.

Campos:

int? CoachId
bool IsGlobal

Regla:

Admin puede crear globales.
Coach puede crear propios.
Coach ve globales + propios.
Coach edita solo propios.
Admin edita todo.

Esto sí se siente SaaS profesional.
