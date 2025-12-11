# Instrucciones de Despliegue en Azure

## Descripción General
Esta guía explica cómo desplegar el **Frontend** (`coachboard-web`) en **Azure Static Web Apps** y configurarlo para conectarse a tu **Backend**.

## Prerrequisitos del Backend
Asegúrate de que tu backend (`CoachBoard.Api`) esté desplegado en un **Azure App Service** o similar. Necesitarás la **URL** de tu API desplegada (ej. `https://coachboard-api.azurewebsites.net`).

## Despliegue del Frontend (Azure Static Web Apps)

1. **Crear Static Web App**:
   - Ve al Portal de Azure -> Crear un recurso -> **Static Web App**.
   - Selecciona tu suscripción, grupo de recursos y nombre (ej. `coachboard-web`).
   - **Detalles de despliegue**: Selecciona "GitHub".
   - Autoriza GitHub y selecciona tu repositorio.
   - **Presets de compilación**: Selecciona `Custom` o `React`.
     - **Ubicación de la app**: `frontend/coachboard-web`
     - **Ubicación de la api**: (Dejar vacío)
     - **Ubicación de salida**: `dist`
   - Haz clic en **Revisar y crear** -> **Crear**.

2. **Configurar Secretos/Variables**:
   - Una vez creada, Azure añadirá automáticamente un archivo de flujo de trabajo a tu repositorio (o puedes usar el creado en `azure-static-web-apps.yml` si lo conectas manualmente).
   - Ve a tu Static Web App en el Portal de Azure.
   - Ve a **Configuración** -> **Variables de entorno**.
   - Añade la siguiente variable:
     - `VITE_API_URL`: La URL de tu API Backend desplegada.
   - Haz clic en **Aplicar**.

3. **Routing y SPA**:
   - Se ha añadido un archivo `staticwebapp.config.json` en `frontend/coachboard-web/public` para asegurar que el enrutamiento funcione correctamente (manejando 404s sirviendo `index.html`).

## Comandos de Desarrollo Local

### Frontend
```bash
cd frontend/coachboard-web
npm install
npm run dev
```

### Backend
```bash
cd src/CoachBoard.Api
dotnet run
```

## Solución de Problemas
- **Fallo en la Compilación**: Revisa los logs de GitHub Actions. Asegúrate de que las dependencias sean correctas.
- **Problemas de Enrutamiento**: Verifica que `staticwebapp.config.json` esté presente en la salida de compilación (`dist`).
- **Error de Conexión a la API**: Revisa la consola del navegador. Usa la variable de entorno `VITE_API_URL`. Asegúrate de que CORS esté habilitado en el Backend para la URL de la Static Web App.
