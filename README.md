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

- **Favoritos**: guardar destinos en la base de datos del usuario para consultarlos después.
- **Búsqueda**: el usuario escribe un destino ("Quiero ir a París") y la app le muestra toda la ficha correspondiente.
- **Intercambio entre grupos**: descarga y visualización de fichas de destino publicadas por otros grupos, mediante un formato JSON compartido (ver sección [Formato de intercambio](#-formato-de-intercambio-entre-grupos)).
- **Elemento sorpresa — Dashboard de estadísticas**: panel visual donde el usuario puede ver qué tipo de contenido guarda más (por sección, por API de origen, por fecha, etc.), implementado con **Chart.js**.

## 🏗️ Arquitectura

El proyecto sigue una arquitectura en capas (n-layer), simulando un entorno de microservicios dentro de una misma solución:

```
WellRoute
├── WellRoute.Web           → Frontend (ASP.NET Core MVC)
├── WellRoute.Api           → API que consume el Web
├── WellRoute.Models        → Class Library con las clases compartidas
└── WellRoute.DataAccess    → Entity Framework + patrón Repositorio
```

- **WellRoute.Web** consume **WellRoute.Api** mediante una capa de servicios (Services).
- **WellRoute.Api** usa **WellRoute.DataAccess** para leer/escribir en la base de datos vía Entity Framework (Database First).
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

- ASP.NET Core (.NET 8) — MVC y Web API
- Entity Framework Core (Database First / Scaffolding)
- SQL Server
- Razor / Model Binding
- Dependency Injection / IoC
- Consumo de APIs REST externas
- Chart.js (dashboard de estadísticas)

## 🚀 Cómo correr el proyecto localmente

1. Cloná el repositorio:
   ```bash
   git clone https://github.com/<usuario-u-organizacion>/WellRoute.git
   ```
2. Abrí `WellRoute.sln` en Visual Studio.
3. Restaurá los paquetes NuGet (Entity Framework Core, SQL Server, Design, Tools).
4. Configurá la cadena de conexión a tu base de datos en `appsettings.json` (proyecto `WellRoute.Api`).
5. Corré el scaffolding de Entity Framework si es la primera vez que configurás el proyecto (ver instrucciones en `/docs`).
6. En Visual Studio, configurá **Multiple Startup Projects**: `WellRoute.Web` y `WellRoute.Api`.
7. Ejecutá el proyecto (F5).

## 🔑 Variables de entorno / API Keys

Cada integrante deberá obtener sus propias API keys para los servicios utilizados (YouTube Data API, Google Places, OpenWeatherMap, Unsplash, TikTok API, Viator/GetYourGuide, etc.) y colocarlas en un archivo de configuración local que **no se debe subir al repositorio** (ver `.gitignore`).

## 👥 Integrantes del grupo

- Deyvin Pasos
- Kenny Alonso
- Nancy Gutierrez
- Abigail Suarez
- Sebastian Flores

## 📄 Licencia

Proyecto académico — SC-701 Programación Avanzada en Web.
