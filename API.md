# CoachBoard API Documentation

## 1. Base URL / URL Base

- **Local:** `http://localhost:5152`
- **Swagger UI:** `http://localhost:5152/swagger`

## 2. Authentication / Autenticación

The API uses **JWT (JSON Web Tokens)**. To access protected endpoints, you must first log in and obtain a token.
La API utiliza **JWT (JSON Web Tokens)**. Para acceder a endpoints protegidos, primero debe iniciar sesión y obtener un token.

### Login Flow / Flujo de Inicio de Sesión

**Endpoint:** `POST /api/auth/login`

**Request Body / Cuerpo de la Petición:**
```json
{
  "email": "coach@test.local",
  "password": "P@ssw0rd!"
}
```

**Success Response / Respuesta Exitosa (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "coach@test.local",
  "role": "Coach",
  "coachId": 1
}
```

## 3. Protected Endpoints / Endpoints Protegidos

Include the token in the `Authorization` header with the `Bearer` scheme.
Incluya el token en el encabezado `Authorization` con el esquema `Bearer`.

**Header Example / Ejemplo de Encabezado:**
```text
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Request Example / Ejemplo de Petición:**
`GET /api/clients?coachId=1`

## 4. Common Errors / Errores Comunes

The API uses standard HTTP status codes and the `ProblemDetails` format (RFC 7807) for errors.
La API usa códigos de estado HTTP estándar y el formato `ProblemDetails` (RFC 7807) para errores.

| Code | Meaning / Significado | Description / Descripción |
|------|-----------------------|---------------------------|
| 400  | Bad Request           | Validation failed or invalid input. / Falló la validación o entrada inválida. |
| 401  | Unauthorized          | Missing or invalid JWT token. / Token JWT faltante o inválido. |
| 403  | Forbidden             | Token valid but user lacks permission (e.g. wrong CoachId). / Token válido pero sin permiso. |
| 404  | Not Found             | Resource not found. / Recurso no encontrado. |
| 500  | Internal Server Error | Unexpected server error. / Error inesperado del servidor. |

**Error Response Example (400) / Ejemplo de Respuesta de Error (400):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Errores de validación",
  "status": 400,
  "detail": "Revisa los campos enviados.",
  "instance": "/api/clients",
  "extensions": {
    "errors": {
      "Email": ["The Email field is not a valid e-mail address."]
    }
  }
}
```
