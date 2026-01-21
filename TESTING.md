# Testing Guide / Guía de Pruebas

This document outlines the testing strategy, structure, and guidelines for the CoachBoard backend.

Este documento describe la estrategia, estructura y pautas de pruebas para el backend de CoachBoard.

## 1. How to Run Tests / Cómo Ejecutar Pruebas

You can run all tests using the .NET CLI from the root directory:
Puede ejecutar todas las pruebas usando la CLI de .NET desde el directorio raíz:

```bash
dotnet test
```

### Filtering by Project / Filtrar por Proyecto
To run specific tests, target the project file:
Para ejecutar pruebas específicas, apunte al archivo del proyecto:

```bash
# Unit Tests / Pruebas Unitarias
dotnet test tests/CoachBoard.Application.Tests/CoachBoard.Application.Tests.csproj

# Integration Tests / Pruebas de Integración
dotnet test tests/CoachBoard.Api.Tests/CoachBoard.Api.Tests.csproj
```

## 2. Project Structure / Estructura del Proyecto

The solution contains three main test projects:
La solución contiene tres proyectos principales de pruebas:

| Project / Proyecto             | Type / Tipo         | Scope / Alcance                                                                 |
|--------------------------------|---------------------|---------------------------------------------------------------------------------|
| `CoachBoard.Tests`             | Unit / Unitarias    | **Domain Layer**: Entities, Value Objects, Domain Logic. No external dep.       |
| `CoachBoard.Application.Tests` | Unit / Unitarias    | **Application Layer**: Handlers, Validators, Mappers. Mocks DbContext/Repos.    |
| `CoachBoard.Api.Tests`         | Integration         | **API Layer**: Controllers, Middleware, Auth. Uses `WebApplicationFactory`.     |

## 3. Coverage & Examples / Cobertura y Ejemplos

### Unit Tests (Application & Domain)
Focus on individual components in isolation. Dependencies are mocked.
Se enfocan en componentes individuales de forma aislada. Las dependencias se simulan (mocks).

**Example/Ejemplo:** Testing a Command Handler
```csharp
[Fact]
public async Task Handle_ValidCommand_ReturnsSuccess()
{
    // Arrange
    var mockRepo = new Mock<ICoachRepository>();
    var handler = new CreateCoachHandler(mockRepo.Object, ...);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

### Integration Tests (API)
Verify the system flows from HTTP request to database (InMemory) and back.
Verifican los flujos del sistema desde la petición HTTP hasta la base de datos (InMemory) y vuelta.

**Example/Ejemplo:** Testing an Endpoint
```csharp
[Fact]
public async Task GetClient_ReturnsOk()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/clients/1");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## 4. Database in Integration Tests / Base de Datos en Pruebas de Integración

**Important:** Integration tests use an **In-Memory Database**, NOT the real SQL Server.
**Importante:** Las pruebas de integración usan una **Base de Datos en Memoria**, NO el SQL Server real.

- **Isolation/Aislamiento:** Each test execution creates a fresh InMemory instance. / Cada ejecución crea una instancia fresca en memoria.
- **Provider/Proveedor:** The `CustomWebApplicationFactory` replaces the SQL Server provider defined in `Program.cs` with `UseInMemoryDatabase`. / La `CustomWebApplicationFactory` reemplaza el proveedor SQL Server de `Program.cs` por `UseInMemoryDatabase`.
- **Data/Datos:** Seeding is done automatically for each test context if configured. / La carga de datos (seed) se hace automáticamente si está configurada.

## 5. Guidelines / Pautas

### Naming Convention / Convención de Nombres
Use the pattern: `MethodName_StateUnderTesting_ExpectedBehavior`
Use el patrón: `NombreMetodo_EstadoBajoPrueba_ComportamientoEsperado`

- `GetById_ExistingId_ReturnsEntity`
- `Create_InvalidData_ReturnsValidationError`

### AAA Pattern / Patrón AAA
Structure every test in 3 clear sections:
Estructure cada prueba en 3 secciones claras:

1.  **Arrange**: Prepare data and mocks. / Preparar datos y mocks.
2.  **Act**: Execute the method under test. / Ejecutar el método bajo prueba.
3.  **Assert**: Verify results and side effects. / Verificar resultados y efectos secundarios.

```csharp
// Arrange
var id = 1;

// Act
var result = service.Get(id);

// Assert
result.Should().NotBeNull();
```
