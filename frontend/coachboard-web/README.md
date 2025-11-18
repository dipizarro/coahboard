# CoachBoard Web (Frontend)

Aplicación web creada con React, TypeScript y Vite para administrar atletas y dashboards de entrenadores en CoachBoard.

## Requisitos

- Node.js ≥ 20
- npm ≥ 10
- Backend de CoachBoard en ejecución (ver carpeta `src/` del monorepo o el API desplegado).

## Configuración rápida

1. Instalar dependencias:
   ```bash
   npm install
   ```
2. Crear un archivo `.env` en la raíz del proyecto basándose en `env.example`.
3. Ejecutar el entorno de desarrollo:
   ```bash
   npm run dev
   ```
4. Abrir `http://localhost:5173` en el navegador.

## Variables de entorno

| Nombre          | Descripción                                              | Valor por defecto |
|-----------------|----------------------------------------------------------|-------------------|
| `VITE_API_URL`  | URL base del backend CoachBoard (incluye protocolo/host) | `http://localhost:5152` |

Ejemplo (`.env`):

```
VITE_API_URL=http://localhost:5152
```

La API debe exponer los endpoints `POST /api/Auth/login`, `POST /api/Auth/register` y el CRUD de clientes/atletas bajo `/api/Clients`.

## Scripts npm

| Script          | Descripción                                             |
|-----------------|---------------------------------------------------------|
| `npm run dev`   | Levanta Vite con HMR.                                   |
| `npm run build` | Compila TypeScript y genera artefactos de producción.   |
| `npm run preview` | Sirve el build localmente para pruebas.               |
| `npm run lint`  | Ejecuta ESLint sobre todo el código fuente.             |

## Estructura relevante

- `src/app`: Router, layout y protección de rutas.
- `src/auth`: Contexto de autenticación y hooks (JWT).
- `src/api`: Cliente Axios y módulos por recurso.
- `src/pages`: Páginas React (login, dashboard, atletas, etc.).
- `src/components`: Componentes reutilizables (navbar, sidebar, tablas).
- `src/lib`: Tipos compartidos y utilidades (storage, helpers).

## Flujo de autenticación

1. `AuthContext` maneja `login/logout`, persistiendo `token` y `user` en `localStorage`.
2. `api/client.ts` añade el header `Authorization: Bearer <token>` en cada request.
3. Las rutas privadas se envuelven con `Protected`, que redirige a `/login` cuando no hay sesión activa.

## Conexión con el backend

- Asegúrate de que el backend tenga habilitado CORS para el origen del frontend.
- El usuario puede registrarse sólo como `Coach`; los roles `Admin` se gestionan desde el backend.
- Para datos reales, inicia sesión con una cuenta existente o crea una nueva desde `/register`.

## Próximos pasos (roadmap módulo 3)

- Finalizar componentes reutilizables (tablas, estados vacíos, loaders).
- Mejorar la experiencia responsive en navbar/sidebar.
- Añadir métricas accionables en el dashboard y completar el CRUD de atletas (búsquedas, filtros, validaciones).
