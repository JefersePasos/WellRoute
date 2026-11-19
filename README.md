# 🧭 WellRoute

**WellRoute** es una guía de viaje interactiva. La página de inicio muestra un collage de destinos (imagen + nombre); al hacer clic en uno, se despliega una ficha completa del lugar con video informativo, lugares icónicos, actividades, clima, compra de entradas y videos cortos de viajeros reales.

> Proyecto desarrollado para el curso **SC-701 Programación Avanzada en Web**.

---

## 🗺️ Concepto

La idea central es responder a la pregunta: **"Quiero ir a París, ¿y ahora qué?"**

1. El usuario entra y ve un **collage de destinos** (solo imagen + nombre), inspirado en páginas tipo TravelMore/TripAdvisor.
2. Al hacer clic en un destino, se abre una **ficha detallada** con:
   - Video informativo del lugar
   - Descripción general (historia, cultura, qué lo hace especial)
   - Lugares icónicos (con foto + descripción corta)
   - Actividades que se pueden hacer
   - Clima, mejor época para viajar, idioma
   - Compra de entradas a atracciones (ej. Torre Eiffel, Coliseo)
   - Videos cortos (estilo TikTok) de gente visitando el lugar
3. El usuario puede **guardar destinos como favoritos** y **buscar** directamente ("¿A dónde quieres ir?").

## 🗂️ Secciones y APIs

| Sección | Contenido | API |
|---|---|---|
| Portada / Collage de destinos | Imágenes de destinos | Unsplash |
| Video informativo | Video del destino | YouTube Data API |
| Videos de viajeros | Clips cortos estilo TikTok de gente visitando el lugar | TikTok API |
| Información general | Datos del país (idioma, moneda, bandera, región) | REST Countries |
| Información de ciudad | Datos de la ciudad (población, coordenadas, zona horaria) | GeoDB Cities |
| El Clima del Viajero | Clima actual y pronóstico del destino | OpenWeatherMap |
| Lugares icónicos | Puntos de interés cercanos (monumentos, plazas, museos) | Google Places API |
| Actividades | Tours, experiencias y cosas que hacer | GetYourGuide / Viator API |
| Compra de entradas | Boletos a atracciones (ej. Torre Eiffel, Coliseo) | Viator API / Tiqets API |
| Presupuesto de viaje | Conversión de moneda para estimar gastos | ExchangeRate API |

## ✨ Funcionalidades principales

- **Autenticación y roles**: inicio de sesión con bloqueo por intentos fallidos, contraseña temporal obligatoria de cambiar en el primer ingreso, y gestión de usuarios, roles y permisos desde un panel de administración.
- **Historial de accesos y bitácora**: registro de inicios y cierres de sesión, y de las acciones administrativas.
- **Favoritos**: guardar destinos en la base de datos del usuario para consultarlos después.
- **Búsqueda**: el usuario escribe un destino ("Quiero ir a París") y la app le muestra toda la ficha correspondiente.
- **Intercambio entre grupos**: descarga y visualización de fichas de destino publicadas por otros grupos, mediante un formato JSON compartido (ver sección [Formato de intercambio](#-formato-de-intercambio-entre-grupos)).
- **Elemento sorpresa — Dashboard de estadísticas**: panel visual donde el usuario puede ver qué tipo de contenido guarda más (por sección, por API de origen, por fecha, etc.), implementado con **Chart.js**.

## 🏗️ Arquitectura

El proyecto sigue una arquitectura en capas (n-layer), simulando un entorno de microservicios dentro de una misma solución:

```
WellRoute
├── WellRoute.Web           → Frontend (ASP.NET Core MVC)
├── WellRoute.API           → API REST que consume el Web
├── WellRoute.Arquitecture  → Lógica de negocio (autenticación, usuarios, roles, auditoría)
├── WellRoute.Models        → Entidades y DTOs compartidos
└── WellRoute.DataAccess    → Entity Framework Core (DbContext y migraciones)
```

- **WellRoute.Web** consume **WellRoute.API** mediante una capa de servicios (Services) y guarda el token de la API dentro de su cookie de sesión.
- **WellRoute.API** expone los endpoints protegidos con JWT y por permisos, y usa **WellRoute.Arquitecture** para la lógica.
- **WellRoute.DataAccess** accede a SQL Server con Entity Framework Core (Code First, con migraciones).
- **WellRoute.Models** es referenciado por todos los demás proyectos para mantener bajo acoplamiento.

## 🔄 Formato de intercambio entre grupos

Estructura JSON acordada para publicar y descargar fichas de destino entre grupos:

```json
{
  "nombreDestino": "string",
  "pais": "string",
  "categoria": "string (ej: Cultura, Playa, Aventura, Gastronomía)",
  "descripcion": "string",
  "imagenUrl": "string",
  "videoUrl": "string",
  "fuenteApi": "string",
  "fechaPublicacion": "datetime",
  "grupoOrigen": "string"
}
```

## 🛠️ Tecnologías

- ASP.NET Core (.NET 10) — MVC y Web API
- Entity Framework Core 10 (Code First con migraciones)
- SQL Server
- Autenticación con JWT (API) y cookies (Web)
- Razor / Model Binding
- Dependency Injection / IoC
- Consumo de APIs REST externas
- Chart.js (dashboard de estadísticas)

## 🚀 Cómo correr el proyecto localmente

**Requisitos:** SDK de .NET 10, SQL Server (LocalDB o Express) y la herramienta de migraciones de EF:

```bash
dotnet tool install --global dotnet-ef
```

1. Clona el repositorio:
   ```bash
   git clone https://github.com/JefersePasos/WellRoute.git
   ```
2. Abre `WellRoute.slnx` en Visual Studio.
3. Configura los secretos de la API (no se suben al repositorio). La cadena de conexión por defecto de `appsettings.json` usa LocalDB; si usas otra instancia de SQL Server, sobrescríbela con tu propio valor:
   ```bash
   dotnet user-secrets set "Jwt:Key" "una-clave-larga-de-minimo-32-caracteres" --project WellRoute.API
   dotnet user-secrets set "ConnectionStrings:WellRouteConnection" "Server=TU_SERVIDOR;Database=WellRoute;Trusted_Connection=true;Encrypt=true;TrustServerCertificate=true;" --project WellRoute.API
   ```
4. Crea la base de datos aplicando las migraciones:
   ```bash
   dotnet ef database update --project WellRoute.DataAccess --startup-project WellRoute.API
   ```
5. En Visual Studio, configura **Multiple Startup Projects**: `WellRoute.API` y `WellRoute.Web`, con el perfil **https** en ambos.
6. Ejecuta el proyecto (F5). En Development, la API crea un administrador de prueba si no hay usuarios (`admin@wellroute.local`). Su contraseña temporal está en `WellRoute.API/DevSeeder.cs` y el sistema pide cambiarla en el primer inicio de sesión.

## 🔑 Variables de entorno / API Keys

Cada integrante deberá obtener sus propias API keys para los servicios utilizados (YouTube Data API, Google Places, OpenWeatherMap, Unsplash, TikTok API, Viator/GetYourGuide, etc.) y colocarlas en un archivo de configuración local o en `user-secrets`, que **no se debe subir al repositorio** (ver `.gitignore`).

## 👥 Integrantes del grupo

- Deyvin Pasos
- Kenny Alonso
- Nancy Gutierrez
- Abigail Suarez
- Sebastian Flores

## 📄 Licencia

Proyecto académico — SC-701 Programación Avanzada en Web.
