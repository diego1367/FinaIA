# Solicitudes de crédito

Aplicación local para registrar solicitudes ficticias, consultarlas y aprobarlas o rechazarlas. El backend está en `backend/` y el frontend en `frontend/`.

## Arranque en Windows

Requiere el SDK de .NET 10, SQL Server LocalDB y `sqlcmd`. El frontend usa Angular 22 y el Node.js 26 instalado en este equipo. Ejecuta estos comandos desde la raíz del proyecto.

```powershell
sqlcmd.exe -S '(localdb)\MSSQLLocalDB' -E -C -b -i backend/sql/001_CreateCreditRequests.sql
dotnet run --project backend/CreditRequests.Api
```

En otra terminal:

```powershell
Set-Location frontend
npm.cmd ci
npm.cmd start
```

Abre http://localhost:4200. La API escucha en http://localhost:5069 y permite CORS desde ese origen de Angular. El script SQL crea la base `CreditRequestsDemo` y se puede volver a ejecutar sin borrar datos. Para otra instancia SQL Server, configura `ConnectionStrings__CreditRequests` en la terminal del backend. Usa el nombre de base indicado en el script.

## Reglas asumidas

- Documento alfanumérico de 6 a 20 caracteres y nombre de 2 a 100 caracteres después de quitar espacios externos.
- Monto positivo en COP, con hasta 13 dígitos enteros y 2 decimales. Plazo entero de 1 a 120 meses.
- Toda solicitud nace pendiente. La primera decisión es definitiva. SQL Server usa `rowversion` para detectar cambios simultáneos.

## API

Con el backend en ejecución en modo Development, abre [Swagger UI](http://localhost:5069/swagger) para explorar y probar los endpoints. El [documento OpenAPI](http://localhost:5069/swagger/v1/swagger.json) también está disponible.

| Método y ruta | Resultado |
| --- | --- |
| `GET /api/credit-requests` | `200`, listado ordenado de más reciente a más antiguo |
| `GET /api/credit-requests/{id}` | `200`, detalle, o `404` |
| `POST /api/credit-requests` | `201` con la solicitud pendiente y su URL, o `400` |
| `PATCH /api/credit-requests/{id}/status` | `200` con el resultado, `400`, `404` o `409` |

El PATCH acepta `{"status":"Approved"}` o `{"status":"Rejected"}`. Angular usa un formulario reactivo tipado, un servicio HttpClient y Signals para actualizar la tabla con la respuesta del servidor.

## Arquitectura del backend

La solución `backend/CreditRequests.Api/CreditRequests.sln` contiene cuatro proyectos:

- **API** valida las solicitudes HTTP, llama a `ICreditRequestService` y traduce los resultados a respuestas HTTP. También configura Swagger y la inyección de dependencias.
- **Application** implementa los casos de uso en `CreditRequestService` y define el contrato de persistencia `ICreditRequestRepository`.
- **Domain** contiene la entidad `CreditRequest` y la regla de aprobación o rechazo. Es independiente de ASP.NET Core y EF Core.
- **Infrastructure** implementa el repositorio con EF Core y SQL Server. Usa `rowversion` para detectar si la solicitud cambió entre la lectura y el guardado.

## Verificación

```powershell
dotnet build backend/CreditRequests.Api
Set-Location frontend
npm.cmd run build
npm.cmd test -- --watch=false
```

En la pantalla, crea una solicitud ficticia, toma la decisión y recarga la página para comprobar que se conserva. También puedes seguir los casos del archivo `backend/CreditRequests.Api/CreditRequests.Api.http` para consultar por ID y verificar los códigos `400` y `409`.

Desde la raíz, `./backend/verify-persistence.ps1` automatiza la verificación contra LocalDB. Arranca la API compilada en el puerto 5071, crea dos solicitudes ficticias, aprueba una, rechaza la otra y consulta ambas después de reiniciar el proceso. Este ejercicio deja los dos registros en la base de demostración.

Si Visual Studio tiene la API abierta, compila en otro directorio y pasa esa salida al script para verificar la compilación nueva:

```powershell
dotnet build backend/CreditRequests.Api/CreditRequests.sln -p:OutputPath=bin/architecture-build
./backend/verify-persistence.ps1 -AppDirectory backend/CreditRequests.Api/bin/architecture-build
```

En la verificación local compilaron ambos proyectos y pasaron tres pruebas de Angular con respuestas HTTP simuladas. La prueba de la API contra LocalDB obtuvo los códigos 200, 201, 400, 404 y 409, y recuperó las solicitudes 1 (aprobada) y 2 (rechazada) después de reiniciar la API.
