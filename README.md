# Vehicle Brands API

API REST desarrollada en .NET 8 para gestionar marcas de automóviles, utilizando Clean Architecture, Entity Framework Core con PostgreSQL, pruebas unitarias con XUnit y Docker Compose.

## Arquitectura del Proyecto

```
VehicleBrands/
├── src/
│   ├── VehicleBrands.Domain/            # Entidades y contratos (capa de dominio)
│   ├── VehicleBrands.Infrastructure/    # DbContext, Repositorios, Migraciones (capa de datos)
│   └── VehicleBrands.API/              # Controladores, DTOs, Middleware (capa de presentación)
├── tests/
│   └── VehicleBrands.Tests/            # Pruebas unitarias con XUnit
├── docker-compose.yml                   # PostgreSQL + API
└── Dockerfile                           # Build multi-stage
```

## Stack Tecnológico

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 8.0 | Runtime & SDK |
| ASP.NET Core | 8.0 | Framework Web API |
| Entity Framework Core | 8.0.11 | ORM + Migraciones |
| PostgreSQL | 16 | Base de datos |
| XUnit | 2.5.3 | Pruebas unitarias |
| FluentAssertions | 7.0.0 | Aserciones legibles |
| Moq | 4.20.72 | Mocking |
| Coverlet | 6.0.4 | Cobertura de código |
| Docker | Multi-stage | Contenedorización |

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)

---

## Ejecución con Docker Compose

```bash
# Levantar PostgreSQL + API
docker-compose up --build

# API disponible en: http://localhost:8080
# Swagger UI: http://localhost:8080/swagger
# Health Check: http://localhost:8080/health
```

### Verificar que funciona

Una vez que los contenedores estén corriendo, se puede probar con los siguientes comandos:

```bash
# 1. Verificar que los contenedores están activos
docker-compose ps

# 2. Health check - verificar conectividad con la base de datos
curl http://localhost:8080/health

# 3. Obtener todas las marcas de autos (56 marcas precargadas)
curl http://localhost:8080/api/v1/MarcasAutos

# 4. Obtener una marca por ID
curl http://localhost:8080/api/v1/MarcasAutos/1

# 5. Filtrar marcas por país
curl "http://localhost:8080/api/v1/MarcasAutos?country=Japan"

# 6. Filtrar marcas de lujo
curl "http://localhost:8080/api/v1/MarcasAutos?isLuxury=true"

# 7. Ordenar por año de fundación descendente
curl "http://localhost:8080/api/v1/MarcasAutos?sortBy=foundedYear&sortDirection=desc"

# 8. Crear una nueva marca
curl -X POST http://localhost:8080/api/v1/MarcasAutos \
  -H "Content-Type: application/json" \
  -d '{"name":"Pagani","countryOfOrigin":"Italy","foundedYear":1992,"isLuxury":true,"headquarters":"San Cesario sul Panaro","website":"https://www.pagani.com"}'

# 9. Detener los contenedores
docker-compose down

# 10. Detener y eliminar volúmenes (reset completo de la BD)
docker-compose down -v
```

También se puede acceder a **Swagger UI** en http://localhost:8080/swagger para probar los endpoints de forma interactiva desde el navegador.

## Desarrollo Local

```bash
# Asegúrese de tener PostgreSQL corriendo en localhost:5432
dotnet restore
dotnet ef database update --project src/VehicleBrands.Infrastructure --startup-project src/VehicleBrands.API
dotnet run --project src/VehicleBrands.API
```

---

## Conexión a Base de Datos

Se configuró `ApplicationDbContext` utilizando Entity Framework Core con el proveedor de PostgreSQL (`Npgsql`). La cadena de conexión se gestiona mediante `appsettings.json` y variables de entorno para Docker.

## Migraciones y Data Seed

- La migración `InitialCreate` genera la tabla `MarcasAutos` en la base de datos.
- Se implementó un **seeder inteligente** (`DatabaseSeeder`) que verifica si cada registro ya existe antes de insertarlo, evitando duplicados.
- Se cargan **56 marcas de automóviles** de 16 países diferentes como datos iniciales.

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/v1/MarcasAutos` | Obtener todas las marcas de autos |
| GET | `/api/v1/MarcasAutos/{id}` | Obtener una marca por ID |
| POST | `/api/v1/MarcasAutos` | Crear una nueva marca |
| PUT | `/api/v1/MarcasAutos/{id}` | Actualizar una marca existente |
| DELETE | `/api/v1/MarcasAutos/{id}` | Eliminar una marca (soft delete) |
| GET | `/health` | Health check (conectividad BD) |

### Parámetros de Consulta (GET /MarcasAutos)

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | int | 1 | Número de página |
| `pageSize` | int | 10 | Elementos por página (máx. 50) |
| `name` | string | - | Filtro por nombre (parcial, insensible a mayúsculas) |
| `country` | string | - | Filtro por país (parcial, insensible a mayúsculas) |
| `isLuxury` | bool | - | Filtro por marca de lujo |
| `includeInactive` | bool | false | Incluir marcas inactivas |
| `sortBy` | string | name | Columna de ordenamiento: name, country, foundedYear, isLuxury, headquarters |
| `sortDirection` | string | asc | Dirección: asc, desc |

### Formato de Respuesta

Todas las respuestas siguen un formato estándar:

```json
{
  "success": true,
  "data": { ... },
  "message": null,
  "errors": null,
  "timestamp": "2026-02-10T..."
}
```

---

## Pruebas Unitarias

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con cobertura de código
dotnet test --collect:"XPlat Code Coverage" --settings:coverlet.runsettings
```

---

## Docker Compose

El archivo `docker-compose.yml` configura dos servicios:

1. **PostgreSQL** (`postgres:16-alpine`): Base de datos con health check y volumen persistente.
2. **API REST** (build multi-stage): Se conecta a PostgreSQL, ejecuta migraciones automáticamente al iniciar.

```yaml
services:
  postgres:
    image: postgres:16-alpine
    ports: ["5432:5432"]
    healthcheck: pg_isready

  api:
    build: .
    ports: ["8080:8080"]
    depends_on:
      postgres:
        condition: service_healthy
```
