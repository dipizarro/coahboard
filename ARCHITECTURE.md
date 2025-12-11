# Arquitectura del Proyecto

## Descripción General
Este monorepo contiene el código fuente de la plataforma CoachBoard, dividido en:
- **Backend**: .NET 8 Web API siguiendo principios de Clean Architecture.
- **Frontend**: Aplicación React construida con Vite + TypeScript.

## Estructura del Backend (`src/`)
El backend está organizado en proyectos que representan capas de Clean Architecture:

- **CoachBoard.Api**: El punto de entrada (Web API). Contiene Controladores, Program.cs y lógica de presentación.
- **CoachBoard.Application**: Contiene la lógica de negocio, Casos de Uso (probablemente handlers de MediatR) e interfaces de Servicio.
- **CoachBoard.Domain**: Las entidades centrales del negocio, objetos de valor y lógica de dominio. Sin dependencias externas.
- **CoachBoard.Infrastructure**: Implementación de interfaces definidas en Application (Repositorios, Servicios Externos, Contexto de Base de Datos).
- **CoachBoard.Contracts**: DTOs o contratos compartidos (probablemente referenciados por la API y consumidores).

## Estructura del Frontend (`frontend/coachboard-web/`)
El frontend es una Single Page Application (SPA) construida con React y Vite.

### Directorios Clave (`src/`)
- **api/**: Funciones cliente de la API y configuración de axios/fetch.
- **app/**: Store de Redux o configuración global de la app.
- **auth/**: Contexto/toviders de autenticación y lógica.
- **components/**: Componentes de UI reutilizables.
- **pages/**: Componentes de página correspondientes a rutas (ej. Login, Dashboard).
- **lib/**: Librerías de utilidad o wrappers de terceros.
- **assets/**: Archivos estáticos (imágenes, fuentes).

## Flujo de Trabajo de Desarrollo
- **Backend**: Ejecutar el proyecto `CoachBoard.Api`.
- **Frontend**: Ejecutar `npm run dev` en `frontend/coachboard-web`.
