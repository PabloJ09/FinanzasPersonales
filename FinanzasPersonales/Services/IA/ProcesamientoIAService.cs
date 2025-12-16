using System.Text.Json;
using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Models;
using FinanzasPersonales.Models.DTOs.IA;
using FinanzasPersonales.Database.Repositories;
using MongoDB.Bson;

namespace FinanzasPersonales.Services.IA;

/// <summary>
/// Servicio para procesar texto libre y convertirlo en datos estructurados usando IA
/// </summary>
public interface IProcesamientoIAService
{
    Task<RespuestaIADto> ProcesarTextoLibreAsync(TextoLibreRequestDto request, CancellationToken cancellationToken = default);
    Task<(Categoria categoria, Transaccion transaccion)> ConfirmarYGuardarAsync(ConfirmarTransaccionDto request, CancellationToken cancellationToken = default);
}

public class ProcesamientoIAService : IProcesamientoIAService
{
    private readonly IAnthropicService _anthropicService;
    private readonly IRepository<Categoria> _categoriaRepository;
    private readonly IRepository<Transaccion> _transaccionRepository;
    private readonly ILogger<ProcesamientoIAService> _logger;

    public ProcesamientoIAService(
        IAnthropicService anthropicService,
        IRepository<Categoria> categoriaRepository,
        IRepository<Transaccion> transaccionRepository,
        ILogger<ProcesamientoIAService> logger)
    {
        _anthropicService = anthropicService;
        _categoriaRepository = categoriaRepository;
        _transaccionRepository = transaccionRepository;
        _logger = logger;
    }

    public async Task<RespuestaIADto> ProcesarTextoLibreAsync(TextoLibreRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            throw new ValidationException("El texto no puede estar vacío");
        }

        if (string.IsNullOrWhiteSpace(request.UsuarioId))
        {
            throw new ValidationException("El ID de usuario es requerido");
        }

        try
        {
            _logger.LogInformation("Procesando texto libre para usuario {UsuarioId}", request.UsuarioId);

            // Enviar a Anthropic
            var respuestaIA = await _anthropicService.ProcesarTextoConClaudeAsync(
                request.Texto, 
                request.UsuarioId, 
                cancellationToken);

            // Parsear la respuesta JSON con conocimiento del texto original
            var datosEstructurados = ParsearRespuestaIA(respuestaIA, request.UsuarioId, request.Texto);

            // Validar y generar advertencias
            var advertencias = ValidarDatosEstructurados(datosEstructurados);

            var respuesta = new RespuestaIADto
            {
                Categoria = datosEstructurados.Categoria,
                Transaccion = datosEstructurados.Transaccion,
                TextoOriginal = request.Texto,
                Advertencias = advertencias
            };

            _logger.LogInformation("Texto procesado exitosamente para usuario {UsuarioId}", request.UsuarioId);

            return respuesta;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear la respuesta de IA");
            throw new ValidationException("La IA no devolvió un formato válido. Por favor, intenta reformular tu texto.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar texto libre");
            throw;
        }
    }

    public async Task<(Categoria categoria, Transaccion transaccion)> ConfirmarYGuardarAsync(
        ConfirmarTransaccionDto request, 
        CancellationToken cancellationToken = default)
    {
        if (request.Categoria == null || request.Transaccion == null)
        {
            throw new ValidationException("Categoría y Transacción son requeridas");
        }

        if (string.IsNullOrWhiteSpace(request.UsuarioId))
        {
            throw new ValidationException("El ID de usuario es requerido");
        }

        try
        {
            _logger.LogInformation("Confirmando transacción para usuario {UsuarioId}", request.UsuarioId);

            // Validar datos antes de guardar
            ValidarDatosParaGuardar(request);

            // Buscar o crear la categoría
            var categoria = await BuscarOCrearCategoriaAsync(request.Categoria, request.UsuarioId, cancellationToken);

            // Crear la transacción
            var transaccion = new Transaccion
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Tipo = request.Transaccion.Tipo!,
                Monto = request.Transaccion.Monto!.Value,
                Descripcion = request.Transaccion.Descripcion ?? "Sin descripción",
                CategoriaId = categoria.Id ?? "Categoria desconocida",
                Fecha = request.Transaccion.Fecha ?? DateTime.UtcNow,
                UsuarioId = request.UsuarioId
            };

            await _transaccionRepository.AddAsync(transaccion);

            _logger.LogInformation("Transacción {TransaccionId} guardada exitosamente", transaccion.Id);

            return (categoria, transaccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar y guardar transacción");
            throw;
        }
    }

    private (CategoriaIADto Categoria, TransaccionIADto Transaccion) ParsearRespuestaIA(string respuestaJson, string usuarioId, string textoOriginal)
    {
        try
        {
            // Limpiar la respuesta por si hay texto extra
            var jsonLimpio = LimpiarJson(respuestaJson);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var documento = JsonSerializer.Deserialize<JsonElement>(jsonLimpio, options);

            var categoria = ParsearCategoria(documento.GetProperty("categoria"), usuarioId);
            var transaccion = ParsearTransaccion(documento.GetProperty("transaccion"), usuarioId, textoOriginal);

            return (categoria, transaccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al parsear JSON de IA: {Json}", respuestaJson);
            throw new JsonException("No se pudo interpretar la respuesta de la IA", ex);
        }
    }

    private string LimpiarJson(string respuesta)
    {
        // Eliminar posibles bloques de código markdown
        respuesta = respuesta.Trim();
        
        if (respuesta.StartsWith("```json"))
        {
            respuesta = respuesta.Substring(7);
        }
        else if (respuesta.StartsWith("```"))
        {
            respuesta = respuesta.Substring(3);
        }

        if (respuesta.EndsWith("```"))
        {
            respuesta = respuesta.Substring(0, respuesta.Length - 3);
        }

        return respuesta.Trim();
    }

    private CategoriaIADto ParsearCategoria(JsonElement elemento, string usuarioId)
    {
        return new CategoriaIADto
        {
            Id = elemento.TryGetProperty("id", out var id) && id.ValueKind != JsonValueKind.Null 
                ? id.GetString() 
                : null,
            Nombre = elemento.TryGetProperty("nombre", out var nombre) && nombre.ValueKind != JsonValueKind.Null 
                ? nombre.GetString() 
                : "Sin clasificar",
            Tipo = elemento.TryGetProperty("tipo", out var tipo) && tipo.ValueKind != JsonValueKind.Null 
                ? tipo.GetString() 
                : null,
            UsuarioId = usuarioId
        };
    }

    private TransaccionIADto ParsearTransaccion(JsonElement elemento, string usuarioId, string textoOriginal)
    {
        var tieneReferenciaTemporal = TextoTieneReferenciaTemporal(textoOriginal);
        var tieneAnoExplicito = TextoTieneAnoExplicito(textoOriginal);

        return new TransaccionIADto
        {
            Id = null,
            Tipo = elemento.TryGetProperty("tipo", out var tipo) && tipo.ValueKind != JsonValueKind.Null 
                ? tipo.GetString() 
                : null,
            Monto = elemento.TryGetProperty("monto", out var monto) && monto.ValueKind != JsonValueKind.Null 
                ? monto.GetDecimal() 
                : null,
            Descripcion = elemento.TryGetProperty("descripcion", out var desc) && desc.ValueKind != JsonValueKind.Null 
                ? desc.GetString() 
                : null,
            CategoriaId = null,
            // Si el texto original NO tiene referencia temporal explícita, forzar hoy (UTC)
            Fecha = CalcularFecha(elemento, textoOriginal, tieneReferenciaTemporal, tieneAnoExplicito),
            UsuarioId = usuarioId
        };
    }

    private bool TextoTieneReferenciaTemporal(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;

        texto = texto.ToLowerInvariant();

        // Palabras clave relativas comunes en ES
        var claves = new[]
        {
            "hoy","ayer","anteayer","anoche","mañana","pasado mañana",
            "la semana pasada","la semana que viene","el mes pasado","este mes","el mes que viene",
            "el viernes pasado","el lunes","el martes","el miércoles","el jueves","el viernes","el sábado","el domingo",
            "esta semana","este fin de semana","ayer por la noche","esta mañana","esta tarde"
        };

        if (claves.Any(k => texto.Contains(k))) return true;

        // Meses y formatos de fecha con números (dd/mm/yyyy, yyyy-mm-dd, dd-mm, etc.)
        var meses = new[]
        {
            "enero","febrero","marzo","abril","mayo","junio","julio","agosto","septiembre","setiembre","octubre","noviembre","diciembre"
        };

        if (meses.Any(m => texto.Contains(m))) return true;

        // Patrón numérico de fechas comunes
        var patrones = new[]
        {
            "\\b[0-3]?\\d/[0-1]?\\d/(?:\\d{2}|\\d{4})\\b",   // 16/12/2025 o 16/12/25
            "\\b(?:\\d{4})-[0-1]?\\d-[0-3]?\\d\\b",           // 2025-12-16
            "\\b[0-3]?\\d-[0-1]?\\d-(?:\\d{2}|\\d{4})\\b", // 16-12-2025
            "\\b[0-3]?\\d/[0-1]?\\d\\b",                        // 16/12
            "\\b[0-3]?\\d-[0-1]?\\d\\b"                         // 16-12
        };

        foreach (var p in patrones)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(texto, p)) return true;
        }

        return false;
    }

    private bool TextoTieneAnoExplicito(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return false;
        // Busca años de 4 dígitos razonables (2000-2099)
        return System.Text.RegularExpressions.Regex.IsMatch(texto, "\\b20\\d{2}\\b");
    }

    private DateTime CalcularFecha(JsonElement elemento, string textoOriginal, bool tieneReferenciaTemporal, bool tieneAnoExplicito)
    {
        // Sin referencia temporal explícita, siempre hoy UTC
        if (!tieneReferenciaTemporal)
        {
            return DateTime.UtcNow;
        }

        // Intentar inferir la fecha exacta desde el texto (ayer, viernes pasado, etc.)
        var ahora = DateTime.UtcNow;
        var inferida = InferirFechaDesdeTexto(textoOriginal, ahora);
        if (inferida.HasValue)
        {
            return inferida.Value;
        }

        // Con referencia temporal pero sin poder inferir, usar la fecha de IA o hoy si falta
        DateTime fecha;
        if (elemento.TryGetProperty("fecha", out var fechaProp) && fechaProp.ValueKind != JsonValueKind.Null && !string.IsNullOrWhiteSpace(fechaProp.GetString()))
        {
            if (!DateTime.TryParse(fechaProp.GetString()!, out fecha))
            {
                fecha = DateTime.UtcNow;
            }
        }
        else
        {
            fecha = DateTime.UtcNow;
        }

        // Si el texto NO incluye año explícito pero la IA devolvió un año diferente al actual, ajustar al año vigente
        if (!tieneAnoExplicito && fecha.Year != ahora.Year)
        {
            fecha = AjustarAno(fecha, ahora.Year);
        }

        return fecha;
    }

    private DateTime? InferirFechaDesdeTexto(string texto, DateTime ahoraUtc)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var t = texto.ToLowerInvariant();

        // Relativas simples
        if (t.Contains("hoy")) return ahoraUtc;
        if (t.Contains("ayer")) return ahoraUtc.AddDays(-1);
        if (t.Contains("anteayer")) return ahoraUtc.AddDays(-2);
        if (t.Contains("mañana"))
        {
            if (t.Contains("pasado mañana")) return ahoraUtc.AddDays(2);
            return ahoraUtc.AddDays(1);
        }
        if (t.Contains("anoche")) return ahoraUtc.AddDays(-1);

        // Día de la semana + calificador
        var dias = new Dictionary<string, DayOfWeek>
        {
            {"lunes", DayOfWeek.Monday},
            {"martes", DayOfWeek.Tuesday},
            {"miércoles", DayOfWeek.Wednesday},
            {"miercoles", DayOfWeek.Wednesday},
            {"jueves", DayOfWeek.Thursday},
            {"viernes", DayOfWeek.Friday},
            {"sábado", DayOfWeek.Saturday},
            {"sabado", DayOfWeek.Saturday},
            {"domingo", DayOfWeek.Sunday},
        };

        foreach (var kv in dias)
        {
            if (t.Contains(kv.Key))
            {
                var objetivo = kv.Value;
                if (t.Contains("pasado"))
                {
                    return DiaDeLaSemanaPasado(ahoraUtc, objetivo);
                }
                if (t.Contains("próximo") || t.Contains("proximo") || t.Contains("que viene"))
                {
                    return DiaDeLaSemanaProximo(ahoraUtc, objetivo);
                }
                if (t.Contains("este"))
                {
                    return DiaDeLaSemanaEste(ahoraUtc, objetivo);
                }
                // Si solo hay el día ("el viernes"), asumir el más reciente pasado
                return DiaDeLaSemanaEste(ahoraUtc, objetivo) <= ahoraUtc
                    ? DiaDeLaSemanaEste(ahoraUtc, objetivo)
                    : DiaDeLaSemanaPasado(ahoraUtc, objetivo);
            }
        }

        // Formatos numéricos explícitos sin año (dd/mm o dd-mm) ⇒ usar año actual
        var m = System.Text.RegularExpressions.Regex.Match(t, "\\b([0-3]?\\d)[/-]([0-1]?\\d)\\b");
        if (m.Success)
        {
            var d = int.Parse(m.Groups[1].Value);
            var mo = int.Parse(m.Groups[2].Value);
            var y = ahoraUtc.Year;
            if (d >= 1 && d <= 31 && mo >= 1 && mo <= 12)
            {
                var maxDias = DateTime.DaysInMonth(y, mo);
                if (d > maxDias) d = maxDias;
                return new DateTime(y, mo, d, ahoraUtc.Hour, ahoraUtc.Minute, ahoraUtc.Second, DateTimeKind.Utc);
            }
        }

        // Fechas ISO o dd/mm/yyyy ya deberían ser detectadas por TryParse en CalcularFecha
        return null;
    }

    private DateTime DiaDeLaSemanaPasado(DateTime ahora, DayOfWeek objetivo)
    {
        var delta = (7 + (ahora.DayOfWeek - objetivo)) % 7;
        if (delta == 0) delta = 7;
        return ahora.AddDays(-delta);
    }

    private DateTime DiaDeLaSemanaProximo(DateTime ahora, DayOfWeek objetivo)
    {
        var delta = (7 - (ahora.DayOfWeek - objetivo)) % 7;
        if (delta == 0) delta = 7;
        return ahora.AddDays(delta);
    }

    private DateTime DiaDeLaSemanaEste(DateTime ahora, DayOfWeek objetivo)
    {
        var delta = objetivo - ahora.DayOfWeek;
        return ahora.AddDays((int)delta);
    }

    private DateTime AjustarAno(DateTime fecha, int nuevoAno)
    {
        var mes = fecha.Month;
        var dia = fecha.Day;
        // Manejar días inválidos (p.ej. 29/02 en año no bisiesto)
        var diasEnMes = DateTime.DaysInMonth(nuevoAno, mes);
        if (dia > diasEnMes) dia = diasEnMes;
        return new DateTime(nuevoAno, mes, dia, fecha.Hour, fecha.Minute, fecha.Second, fecha.Kind);
    }

    private List<string> ValidarDatosEstructurados((CategoriaIADto Categoria, TransaccionIADto Transaccion) datos)
    {
        var advertencias = new List<string>();

        // Validar tipo
        if (string.IsNullOrWhiteSpace(datos.Transaccion.Tipo) || 
            (datos.Transaccion.Tipo != "Ingreso" && datos.Transaccion.Tipo != "Gasto"))
        {
            advertencias.Add("No se pudo determinar si es un ingreso o gasto. Por favor, verifica.");
        }

        // Validar monto
        if (!datos.Transaccion.Monto.HasValue || datos.Transaccion.Monto <= 0)
        {
            advertencias.Add("No se pudo determinar el monto. Por favor, verifica.");
        }

        // Validar categoría
        if (string.IsNullOrWhiteSpace(datos.Categoria.Nombre))
        {
            advertencias.Add("No se pudo determinar la categoría. Se usará 'Sin clasificar'.");
        }

        // Validar coherencia entre tipo de categoría y transacción
        if (!string.IsNullOrWhiteSpace(datos.Categoria.Tipo) && 
            !string.IsNullOrWhiteSpace(datos.Transaccion.Tipo) &&
            datos.Categoria.Tipo != datos.Transaccion.Tipo)
        {
            advertencias.Add("El tipo de categoría no coincide con el tipo de transacción.");
        }

        // Validar descripción
        if (string.IsNullOrWhiteSpace(datos.Transaccion.Descripcion))
        {
            advertencias.Add("No hay descripción. Por favor, agrega una.");
        }

        return advertencias;
    }

    private void ValidarDatosParaGuardar(ConfirmarTransaccionDto request)
    {
        var errores = new List<string>();

        // Validar transacción
        if (string.IsNullOrWhiteSpace(request.Transaccion.Tipo))
        {
            errores.Add("El tipo de transacción es requerido");
        }
        else if (request.Transaccion.Tipo != "Ingreso" && request.Transaccion.Tipo != "Gasto")
        {
            errores.Add("El tipo debe ser 'Ingreso' o 'Gasto'");
        }

        if (!request.Transaccion.Monto.HasValue || request.Transaccion.Monto <= 0)
        {
            errores.Add("El monto debe ser mayor a 0");
        }

        if (string.IsNullOrWhiteSpace(request.Transaccion.Descripcion))
        {
            errores.Add("La descripción es requerida");
        }

        // Validar categoría
        if (string.IsNullOrWhiteSpace(request.Categoria.Nombre))
        {
            errores.Add("El nombre de la categoría es requerido");
        }

        if (string.IsNullOrWhiteSpace(request.Categoria.Tipo))
        {
            errores.Add("El tipo de categoría es requerido");
        }
        else if (request.Categoria.Tipo != "Ingreso" && request.Categoria.Tipo != "Gasto")
        {
            errores.Add("El tipo de categoría debe ser 'Ingreso' o 'Gasto'");
        }

        if (errores.Any())
        {
            throw new ValidationException($"Datos inválidos: {string.Join(", ", errores)}");
        }
    }

    private async Task<Categoria> BuscarOCrearCategoriaAsync(
        CategoriaIADto categoriaDto, 
        string usuarioId, 
        CancellationToken cancellationToken)
    {
            // Buscar si ya existe una categoría con el mismo nombre y tipo para este usuario
        var categoriasExistentes = await _categoriaRepository.GetAllAsync();
        var categoriaExistente = categoriasExistentes.FirstOrDefault(c => 
            c.Nombre.Equals(categoriaDto.Nombre, StringComparison.OrdinalIgnoreCase) && 
            c.Tipo == categoriaDto.Tipo &&
            c.UsuarioId == usuarioId);        if (categoriaExistente != null)
        {
            _logger.LogInformation("Usando categoría existente {CategoriaId}", categoriaExistente.Id);
            return categoriaExistente!;
        }

        // Crear nueva categoría
        var nuevaCategoria = new Categoria
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Nombre = categoriaDto.Nombre!,
            Tipo = categoriaDto.Tipo!,
            UsuarioId = usuarioId
        };

        await _categoriaRepository.AddAsync(nuevaCategoria);
        _logger.LogInformation("Nueva categoría {CategoriaId} creada", nuevaCategoria.Id);

        return nuevaCategoria;
    }
}
