# Santa Cecilia

## Descripción del proyecto
Santa Cecilia es una solución para gestión operativa agrícola y nómina semanal, compuesta por:
- Un backend ASP.NET Core Web API que centraliza reglas de negocio y persistencia.
- Un frontend .NET MAUI multiplataforma para operación diaria (administración, registro de tiempos y pago).

El sistema cubre autenticación, validación de usuarios, administración de trabajadores, lotes y tipos de trabajo, registro de tiempos, cálculo de nómina y trazabilidad mediante bitácora.

## Problema que resuelve
En operaciones con múltiples trabajadores y actividades diarias, es común tener:
- Registro manual disperso de jornadas y actividades.
- Errores en cálculo de pago semanal.
- Dificultad para auditar quién hizo qué cambio y cuándo.

Santa Cecilia centraliza esos procesos para reducir errores, acelerar cierres semanales y mejorar control operativo.

## Arquitectura y enfoque técnico
### Enfoque general
- Arquitectura cliente-servidor.
- Frontend MAUI consume API REST del backend.
- Backend integrado con Firebase Auth y Firestore.

### Backend
- API REST en ASP.NET Core (`net10.0`).
- Servicios por dominio (`UserService`, `WorkerService`, `WorkedTimeService`, `PayrollService`, etc.).
- Persistencia en Firestore.
- Registro de auditoría en acciones críticas.

### Frontend
- Aplicación MAUI (`net10.0-android`, `net10.0-ios`, `net10.0-maccatalyst`, `net10.0-windows10.0.19041.0`).
- Páginas XAML y servicios (`ApiService`, `AuthService`, `SessionService`).
- Consumo HTTP al backend y autenticación con Firebase.

## Tecnologías utilizadas
- .NET SDK 10.0
- ASP.NET Core Web API
- .NET MAUI
- Firebase Authentication (Admin SDK y cliente)
- Google Cloud Firestore
- ClosedXML (exportación de datos)
- iText7 (generación de PDF)

## Requisitos previos
- .NET SDK 10.0 instalado.
- Workloads de MAUI instalados:
  - `dotnet workload install maui`
- Visual Studio 2022/2025 con soporte MAUI (recomendado en Windows).
- Proyecto Firebase/GCP activo con Firestore habilitado.
- Archivo de credenciales de servicio Firebase (`firebase-key.json`).

## Instalación paso a paso
1. Clonar repositorio:
   ```bash
   git clone <url-del-repositorio>
   cd SantaCecilia
   ```
2. Restaurar paquetes:
   ```bash
   dotnet restore backend/backend.csproj
   dotnet restore frontend/frontend.csproj
   ```
3. Preparar credenciales Firebase:
   - Colocar `firebase-key.json` en `backend/`.
4. Verificar URL del backend en frontend:
   - Revisar `frontend/Configuration/AppSettings.cs`.
   - En Android emulador usa `http://10.0.2.2:5191`.
   - En DEBUG local usa `http://localhost:5191`.

## Configuración
### Backend
Configuración detectada:
- `backend/Program.cs` inicializa Firebase con:
  - archivo: `firebase-key.json`
  - `ProjectId`: `santa-cecilia-s`

`appsettings.json` incluye sección `Firebase`, pero actualmente `Program.cs` usa valores directos. Para despliegues productivos se recomienda externalizar esta configuración.

### Frontend
`frontend/Configuration/AppSettings.cs` contiene:
- `BackendUrl` por plataforma/entorno.
- `FirebaseApiKey` para autenticación cliente.

Para producción:
- Reemplazar `BackendUrl` por dominio HTTPS real.
- Rotar/gestionar `FirebaseApiKey` según políticas de seguridad del proyecto.

## Ejecución del proyecto
### 1) Ejecutar backend
```bash
dotnet run --project backend/backend.csproj
```
API por defecto en desarrollo: `http://localhost:5191`.

### 2) Ejecutar frontend (Windows)
```bash
dotnet build frontend/frontend.csproj -f net10.0-windows10.0.19041.0
dotnet run --project frontend/frontend.csproj -f net10.0-windows10.0.19041.0
```

### 3) Hot reload backend (opcional)
```bash
dotnet watch run --project backend/backend.csproj
```

## Ejemplos de uso
### Registro de usuario
```bash
curl -X POST http://localhost:5191/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "usuario@dominio.com",
    "password": "Password123!"
  }'
```

### Login con token Firebase
```bash
curl -X POST http://localhost:5191/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "<firebase-id-token>"
  }'
```

### Crear trabajador
```bash
curl -X POST http://localhost:5191/api/workers/worker_001 \
  -H "Content-Type: application/json" \
  -H "X-User-Id: admin_uid" \
  -d '{
    "name": "Luis",
    "lastName": "Fernandez",
    "identification": "8-123-456",
    "active": true
  }'
```

### Procesar nómina semanal
```bash
curl -X POST http://localhost:5191/api/payrolls/process \
  -H "Content-Type: application/json" \
  -H "X-User-Id: admin_uid" \
  -d '{
    "weekStart": "2026-02-08"
  }'
```

## Estructura de carpetas
```text
SantaCecilia/
├── backend/
│   ├── Controllers/      # Endpoints REST por dominio
│   ├── DTOs/             # Contratos de entrada/salida
│   ├── Models/           # Entidades de negocio
│   ├── Services/         # Lógica de acceso y reglas
│   ├── Program.cs        # Configuración de app, Firebase, CORS
│   └── appsettings*.json # Configuración de entorno
├── frontend/
│   ├── Pages/            # Pantallas XAML
│   ├── Services/         # Integraciones HTTP y sesión
│   ├── Models/           # Modelos de UI y dominio local
│   ├── ViewModels/       # Estado y binding
│   ├── Resources/        # Imágenes, fuentes y assets MAUI
│   └── MauiProgram.cs    # DI y bootstrap MAUI
└── SantaCecilia.slnx     # Solución principal
```

## Buenas prácticas para contribuir
- Crear ramas por objetivo (`feature/*`, `fix/*`, `chore/*`).
- Usar Conventional Commits.
  - Ejemplo: `fix(payroll): improve mobile action controls`
- Mantener cambios atómicos por pantalla o módulo.
- Validar compilación antes de abrir PR:
  ```bash
  dotnet build backend/backend.csproj
  dotnet build frontend/frontend.csproj -f net10.0-windows10.0.19041.0
  ```
- No incluir secretos (como `firebase-key.json`) en commits.

## Guía básica de despliegue
### Backend
1. Publicar binarios:
   ```bash
   dotnet publish backend/backend.csproj -c Release -o ./artifacts/backend
   ```
2. Inyectar credenciales de Firebase en servidor (archivo seguro o secret manager).
3. Configurar `ProjectId`, CORS y URL pública detrás de HTTPS (reverse proxy o App Service).
4. Ejecutar con servicio administrado (systemd, Windows Service, contenedor, etc.).

### Frontend
Publicar según plataforma objetivo:
- Windows:
  ```bash
  dotnet publish frontend/frontend.csproj -f net10.0-windows10.0.19041.0 -c Release
  ```
- Android/iOS/MacCatalyst: publicar con certificados/perfiles de firma correspondientes del entorno.

## Licencia
Este proyecto está bajo una **licencia propietaria** y **no es software libre**.

- Consulte el archivo `LICENSE` para el texto legal completo.
- Queda prohibido usar, copiar, modificar o distribuir este software sin autorización previa y por escrito del titular.
