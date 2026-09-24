# 🧭 WellRoute

**WellRoute** es un periódico digital que combina bienestar y viajes en un solo lugar: un feed de noticias, recetas, rutinas, destinos y multimedia, organizado como las secciones de un diario tradicional, pero alimentado en tiempo real por 10 APIs externas distintas.

> Proyecto desarrollado para el curso **SC-701 Programación Avanzada en Web**.

---

## 📰 Concepto

Así como un periódico tiene secciones de Deportes, Opinión o Cultura alimentadas por distintos corresponsales, WellRoute organiza su contenido en secciones temáticas, cada una conectada a una API diferente.

## 🗂️ Secciones y APIs

| Sección | Contenido | API |
|---|---|---|
| Portada / Titulares | Noticias de bienestar y turismo | NewsAPI |
| Editorial | Frases motivacionales del día | Quotable |
| Vida Activa | Rutinas de ejercicio | ExerciseDB |
| Buena Mesa | Recetas saludables | TheMealDB |
| Multimedia | Podcasts de desarrollo personal y viajes | Spotify API |
| El Clima del Viajero | Clima de destinos | OpenWeatherMap |
| Destinos | Datos de países y ciudades | REST Countries |
| Fotorreportaje | Fotografías de bienestar y destinos | Unsplash |
| Videoteca | Videos de viajeros y rutinas | YouTube Data API |
| La Librería | Libros de viaje y desarrollo personal | Open Library |

## ✨ Funcionalidades principales

- **Favoritos**: guardar contenido de cualquier sección en la base de datos del usuario.
- **Intercambio entre grupos**: descarga y visualización de contenido publicado por otros grupos, mediante un formato JSON compartido (ver sección [Formato de intercambio](#-formato-de-intercambio-entre-grupos)).
- **Elemento sorpresa**: _(completar con lo que el grupo decida implementar, ej. lector de texto a voz, ticker de última hora, gamificación, etc.)_

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

Estructura JSON acordada para publicar y descargar contenido entre grupos:

```json
{
  "titulo": "string",
  "seccion": "string",
  "resumen": "string",
  "imagenUrl": "string",
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

Cada integrante deberá obtener sus propias API keys para los servicios utilizados (NewsAPI, Spotify, OpenWeatherMap, Unsplash, YouTube, etc.) y colocarlas en un archivo de configuración local que **no se debe subir al repositorio** (ver `.gitignore`).

## 👥 Integrantes del grupo

- Deyvin Pasos
- Kenny Alonso
- Nancy Gutierrez
- Abigail Suarez
- Sebastian Flores

## 📄 Licencia

Proyecto académico — SC-701 Programación Avanzada en Web.
