namespace WellRoute.Models.Dtos;

public enum EstadoOperacion
{
    Exitoso,
    NoEncontrado,
    Conflicto,
    Invalido
}

// Resultado de una operación de los servicios de usuarios y roles.
public sealed class ResultadoOperacion<T>
{
    public EstadoOperacion Estado { get; init; }

    public string? Mensaje { get; init; }

    public T? Datos { get; init; }

    public static ResultadoOperacion<T> Ok(T datos) =>
        new() { Estado = EstadoOperacion.Exitoso, Datos = datos };

    public static ResultadoOperacion<T> Fallo(EstadoOperacion estado, string mensaje) =>
        new() { Estado = estado, Mensaje = mensaje };
}
