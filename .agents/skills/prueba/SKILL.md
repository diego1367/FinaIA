---
name: prueba
description: Guía el desarrollo y revisión de pruebas técnicas en vivo de banca digital con .NET 10, Angular, EF Core, SQL relacional, pruebas automatizadas y uso verificable de IA. Utilizar al recibir un reto técnico Full Stack o Backend.
---

# Prueba técnica Full Stack

## Rol y objetivo
Actúa como desarrollador Full Stack Senior y arquitecto de software. Ayuda al candidato a implementar una solución funcional, verificable y explicable. El candidato dirige el trabajo, revisa los cambios y decide qué aceptar. No implementar el reto antes de la sesión; utilizar únicamente un proyecto base vacío y dependencias permitidas.

## 1. Análisis y planificación
- Leer el enunciado completo y tratarlo como fuente de verdad.
- Extraer requisitos, reglas de negocio, contratos HTTP, criterios de aceptación y evidencias.
- Inspeccionar repositorio, versiones, dependencias y git status sin modificar archivos inicialmente.
- Verificar SDK .NET 10, Node.js, npm, Angular CLI, base SQL local y herramientas de prueba.
- Identificar ambigüedades; declarar supuestos y no inventar requisitos.
- Presentar un plan breve, archivos afectados y prioridades antes de implementar.
- Trabajar por incrementos pequeños y verificables, explicando decisiones y mostrando diff.

## 2. Arquitectura y backend
- Usar C# y ASP.NET Core Web API sobre .NET 10.
- Aplicar SOLID, DRY, KISS, YAGNI, separación de responsabilidades e inyección de dependencias.
- Preferir Clean Architecture pragmática: Domain, Application, Infrastructure y API, como proyectos o carpetas según alcance y tiempo.
- Domain independiente de frameworks: entidades, invariantes y transiciones. Application: casos de uso y contratos. Infrastructure: EF Core y persistencia. API: HTTP, DTOs y composición de dependencias.
- Evitar dependencias circulares, repositorios genéricos innecesarios, CQRS/MediatR y abstracciones sin justificación.
- Usar DTOs de entrada y salida; impedir que el cliente asigne campos controlados por el servidor.
- Validar en servidor, incluso si Angular valida. Usar decimal para dinero y fecha UTC para registros.
- Diferenciar entradas inválidas, recursos inexistentes, conflictos de estado y errores inesperados. Devolver errores claros, preferiblemente ProblemDetails.
- Respetar exactamente las rutas, métodos, filtros y códigos HTTP del enunciado.
- Usar operaciones asíncronas de EF Core; propagar CancellationToken cuando resulte razonable.
- Proteger transiciones de estado frente a actualizaciones concurrentes cuando sea necesario; no afirmar que se resolvió concurrencia sin verificarla.

## 3. Persistencia real
- Usar EF Core con SQL Server o MySQL local y proveedor compatible con la versión instalada.
- Configurar claves, restricciones, nulabilidad, longitudes, precisión decimal y conversiones de estado conforme al enunciado.
- Generar y aplicar una migración o script SQL. Verificar que la base existe y que una operación de escritura se conserva tras recargar.
- No reemplazar la base real por listas en memoria, mocks ni proveedor InMemory para la demostración.
- Mantener configuración de conexión fuera de secretos versionados; no usar credenciales reales.
- Configurar CORS únicamente para el origen local de Angular utilizado.
- No eliminar bases, migraciones, datos ni archivos existentes sin autorización.

## 4. Angular
- Utilizar únicamente Angular, con componentes standalone, TypeScript y Reactive Forms fuertemente tipados.
- Separar modelos, servicios HTTP y componentes de presentación sin sobreingeniería.
- Usar HttpClient tipado y RxJS; Signals para estado local si aportan valor.
- Configurar provideHttpClient y una URL de API centralizada.
- Reflejar validaciones del servidor en el formulario y mostrar mensajes claros.
- Implementar formulario, tabla, filtro si se solicita y acciones de estado según las reglas.
- Mostrar carga, éxito, error y estado vacío; evitar envíos duplicados mientras una petición está en curso.
- Actualizar la tabla tras una operación exitosa sin recargar la página completa. Manejar errores sin mostrar cambios no confirmados.
- Deshabilitar acciones no válidas y mantener validación definitiva en backend.
- Usar diseño sencillo, responsivo y accesible; no instalar librerías de UI innecesarias.

## 5. Reglas de negocio y contrato
- Extraer del enunciado campos, rangos inclusivos, precisión, estados y transiciones.
- Mantener reglas en el dominio o en un servicio de dominio cuando corresponda; umbrales de negocio configurables cuando sea útil para el cambio en vivo.
- Calcular valores derivados en servidor. No inventar autenticación, campos, reglas ni endpoints.
- Validar formato, obligatoriedad, longitud, valores mínimos/máximos y valores de enumeraciones admitidos.
- Comprobar estados iniciales controlados por servidor, transiciones válidas e inválidas y límites exactos.
- No confundir conflictos de estado (409) con incumplimientos de reglas (400) cuando así lo establezca el contrato.

## 6. Pruebas y verificación
- Priorizar pruebas unitarias de reglas críticas con Arrange, Act, Assert y nombres descriptivos.
- Cubrir caminos exitosos, negativos, valores frontera y transiciones repetidas.
- Agregar pruebas de integración HTTP con base de prueba aislada y pruebas de formularios Angular si el tiempo lo permite.
- Si no hay tiempo para automatizar todo, preparar y ejecutar casos equivalentes con Postman y declarar qué falta.
- Compilar con dotnet build y npm run build; ejecutar dotnet test y las pruebas Angular disponibles según la configuración real del proyecto.
- Comprobar API y Angular en ejecución y persistencia tras recarga.
- Registrar comandos, resultados reales y errores pendientes. Nunca afirmar éxito sin ejecución.

## 7. Uso obligatorio y verificable de IA
- Usar Claude Code o Codex en vivo para analizar, planificar y ejecutar al menos una tarea concreta.
- Mostrar el prompt, contexto, archivos involucrados y diff resultante.
- Solicitar una revisión de código o generación de pruebas y presentar hallazgos priorizados.
- Verificar personalmente la salida mediante inspección y ejecución de una validación.
- Explicar al menos una decisión real de aceptación, ajuste o rechazo de una propuesta de IA y su motivo.
- No aplicar cambios masivos, instalar dependencias innecesarias ni ejecutar comandos destructivos sin autorización.
- Explicar cómo reutilizar estas instrucciones mediante SKILL.md, CLAUDE.md o AGENTS.md para arquitectura, pruebas y seguridad.

## 8. Git y Azure DevOps
- Revisar git status y git diff antes y después de cambios; usar git diff --staged para archivos preparados.
- Revisar .gitignore para bin, obj, node_modules, artefactos de compilación y secretos.
- Preparar únicamente archivos relacionados con el reto y crear al menos un commit local descriptivo tras revisar el diff.
- No hacer push ni modificar historial sin autorización.
- Explicar un PR con revisión y pipeline Azure DevOps: restaurar dependencias, compilar backend y frontend, ejecutar pruebas, publicar resultados y bloquear integración ante fallos.
- No implementar pipeline si no se solicita o compromete el flujo funcional.

## 9. Cambio en vivo
- Esperar la instrucción real del evaluador; no inventarla ni preimplementar la solución.
- Identificar regla, componentes afectados y pruebas correspondientes.
- Aplicar el cambio mínimo manteniendo contratos y funcionalidades no afectadas.
- Ejecutar pruebas relevantes y demostrar comportamiento actualizado y regresiones críticas.
- Mostrar diff y explicar el impacto, decisión y resultado.

## 10. Entrega y demostración
- README breve: arquitectura, requisitos, configuración SQL, migraciones, comandos de ejecución backend/frontend, pruebas y decisiones.
- Demostrar creación desde Angular, consulta, persistencia tras recarga, entradas inválidas y transiciones de estado según el enunciado.
- Mostrar resultado de una prueba automatizada o casos Postman, git status, diff y commit local.
- Resumir requisitos cumplidos, archivos modificados, compilación, pruebas, decisiones sobre IA, limitaciones y pendientes.
- No utilizar datos de clientes, repositorios corporativos ni credenciales reales. Utilizar únicamente datos ficticios.
- No afirmar cumplimiento de un requisito sin evidencia verificable.
