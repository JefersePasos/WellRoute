using WellRoute.Models.Dtos;

namespace WellRoute.Web.Models;

// Portada: collage con buscador y filtro de categoría.
public sealed class PortadaViewModel
{
    public List<DestinoListaDto> Destinos { get; set; } = new();

    public string? Buscar { get; set; }

    public string? Categoria { get; set; }

    public string[] Categorias { get; } = CategoriasDestino.Todas;

    // Destinos que el usuario autenticado ya guardó.
    public HashSet<int> FavoritosIds { get; set; } = new();

    // Solo se muestran los corazones si hay sesión y la API respondió bien.
    public bool PuedeFavoritos { get; set; }

    public string? Error { get; set; }

    public bool HayFiltros =>
        !string.IsNullOrWhiteSpace(Buscar) || !string.IsNullOrWhiteSpace(Categoria);
}

// Una tarjeta del collage (se usa en la portada y en Mis favoritos).
public sealed class TarjetaDestinoViewModel
{
    public DestinoListaDto Destino { get; init; } = new();

    public bool MostrarFavorito { get; init; }

    public bool EsFavorito { get; init; }

    // Página a la que se vuelve después de marcar o quitar el favorito.
    public string ReturnUrl { get; init; } = "/";
}

public sealed class DestinoFichaViewModel
{
    public DestinoDetalleDto Destino { get; init; } = new();

    public bool PuedeFavoritos { get; init; }

    public bool EsFavorito { get; init; }
}
