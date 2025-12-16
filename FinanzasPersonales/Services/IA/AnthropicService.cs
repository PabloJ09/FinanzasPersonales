using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FinanzasPersonales.Services.IA;

/// <summary>
/// Servicio para interactuar con la API de Anthropic Claude
/// </summary>
public interface IAnthropicService
{
    Task<string> ProcesarTextoConClaudeAsync(string texto, string usuarioId, CancellationToken cancellationToken = default);
}

public class AnthropicService : IAnthropicService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AnthropicService> _logger;
    private readonly string _apiKey;
    private readonly string _model;
    private const string API_URL = "https://api.anthropic.com/v1/messages";
    private const int MAX_TOKENS = 1024;

    public AnthropicService(IHttpClientFactory httpClientFactory, ILogger<AnthropicService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("Anthropic");
        _logger = logger;
        
        // Leer la API Key desde variable de entorno (CRÍTICO para seguridad)
        _apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") 
            ?? throw new InvalidOperationException("ANTHROPIC_API_KEY no está configurada en las variables de entorno");

        // Modelo configurable; permite usar uno disponible en la cuenta/region
        _model = Environment.GetEnvironmentVariable("ANTHROPIC_MODEL")
           ?? "claude-3-5-sonnet-20241022";
    }

    public async Task<string> ProcesarTextoConClaudeAsync(string texto, string usuarioId, CancellationToken cancellationToken = default)
    {
        try
        {
        _logger.LogInformation("Enviando solicitud a Anthropic para usuario {UsuarioId} con modelo {Modelo}", usuarioId, _model);

            var prompt = ConstruirPrompt(texto, usuarioId);
            var requestBody = new
            {
                model = _model,
              max_tokens = MAX_TOKENS,
              messages = new[]
              {
                new
                {
                  role = "user",
                  // Anthropic v1 requires content blocks, send as text block array
                  content = new[]
                  {
                    new { type = "text", text = prompt }
                  }
                }
              }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, API_URL)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            // Headers de seguridad de Anthropic
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
              var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
              _logger.LogError("Error de Anthropic API: {StatusCode} - {Content}", response.StatusCode, errorContent);
              throw new InvalidOperationException($"Anthropic devolvió {response.StatusCode}. Detalle: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Respuesta recibida de Anthropic exitosamente");

            var extracted = ExtraerTextoDeRespuestaAnthropic(responseContent);
            if (string.IsNullOrWhiteSpace(extracted))
            {
              throw new InvalidOperationException("La respuesta de Anthropic no contiene texto útil");
            }

            var jsonSolo = ExtraerJson(extracted);
            if (string.IsNullOrWhiteSpace(jsonSolo))
            {
              throw new InvalidOperationException("No se pudo extraer un JSON válido de la respuesta de IA");
            }

            return jsonSolo;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al comunicarse con Anthropic");
            throw new InvalidOperationException("No se pudo conectar con el servicio de IA. Por favor, intenta más tarde.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al parsear la respuesta de Anthropic");
            throw new InvalidOperationException("La respuesta del servicio de IA no es válida.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al procesar con Anthropic");
            throw;
        }
    }

    private string ConstruirPrompt(string texto, string usuarioId)
    {
        return $@"Eres un asistente financiero experto. Tu tarea es convertir el siguiente texto libre en datos estructurados para una transacción financiera.

TEXTO DEL USUARIO:
{texto}

REGLAS ESTRICTAS:
1. Debes responder ÚNICAMENTE con un JSON válido, sin texto adicional antes o después. No uses bloques de código ni backticks (```), responde solo el JSON crudo.
2. El JSON debe tener exactamente esta estructura:
{{
  ""categoria"": {{
    ""id"": null,
    ""nombre"": ""string o null"",
    ""tipo"": ""Ingreso"" o ""Gasto"" o null,
    ""usuarioId"": ""{usuarioId}""
  }},
  ""transaccion"": {{
    ""id"": null,
    ""tipo"": ""Ingreso"" o ""Gasto"" o null,
    ""monto"": número decimal o null,
    ""descripcion"": ""string o null"",
    ""categoriaId"": null,
    ""fecha"": ""fecha en formato ISO-8601"" o null,
    ""usuarioId"": ""{usuarioId}""
  }}
}}

3. Si el texto menciona algo como ""compré"", ""gasté"", ""pagué"" → tipo: ""Gasto""
4. Si el texto menciona ""cobré"", ""recibí"", ""ingreso"", ""ganancia"" → tipo: ""Ingreso""
5. Si no puedes determinar el monto con certeza, usa null
6. Si no puedes determinar la fecha, usa la fecha y hora actual en formato ISO-8601
7. Si el usuario dice ""hoy"", ""ayer"", ""anteayer"", ""el viernes pasado"" u otras referencias relativas, calcula la fecha real con base en la fecha actual UTC. Usa siempre el año vigente (no inventes años pasados). Formato ISO-8601 completo (ej: {DateTime.UtcNow:O}).
8. Si no se menciona una categoría clara, usa ""Sin clasificar"" como nombre
9. El tipo de la categoría debe coincidir con el tipo de la transacción
10. No inventes datos. Si algo no está claro, usa null
11. Los campos id y categoriaId siempre deben ser null (se generan en el backend)

EJEMPLOS:
Entrada: ""Compré pan por 500 colones hoy""
Salida:
{{
  ""categoria"": {{
    ""id"": null,
    ""nombre"": ""Alimentación"",
    ""tipo"": ""Gasto"",
    ""usuarioId"": ""{usuarioId}""
  }},
  ""transaccion"": {{
    ""id"": null,
    ""tipo"": ""Gasto"",
    ""monto"": 500,
    ""descripcion"": ""Compré pan"",
    ""categoriaId"": null,
    ""fecha"": ""{DateTime.UtcNow:O}"",
    ""usuarioId"": ""{usuarioId}""
  }}
}}

Entrada: ""Me pagaron el sueldo 50000""
Salida:
{{
  ""categoria"": {{
    ""id"": null,
    ""nombre"": ""Salario"",
    ""tipo"": ""Ingreso"",
    ""usuarioId"": ""{usuarioId}""
  }},
  ""transaccion"": {{
    ""id"": null,
    ""tipo"": ""Ingreso"",
    ""monto"": 50000,
    ""descripcion"": ""Pago de sueldo"",
    ""categoriaId"": null,
    ""fecha"": ""{DateTime.UtcNow:O}"",
    ""usuarioId"": ""{usuarioId}""
  }}
}}

AHORA PROCESA EL TEXTO DEL USUARIO Y RESPONDE SOLO CON EL JSON:";
    }

  private static string ExtraerTextoDeRespuestaAnthropic(string responseContent)
  {
    try
    {
      using var doc = JsonDocument.Parse(responseContent);
      if (!doc.RootElement.TryGetProperty("content", out var contentArray) || contentArray.ValueKind != JsonValueKind.Array)
      {
        return string.Empty;
      }

      var partes = new List<string>();
      foreach (var item in contentArray.EnumerateArray())
      {
        if (item.ValueKind != JsonValueKind.Object) continue;
        if (item.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "text")
        {
          if (item.TryGetProperty("text", out var textProp))
          {
            var t = textProp.GetString();
            if (!string.IsNullOrWhiteSpace(t)) partes.Add(t!);
          }
        }
      }

      return string.Join("\n\n", partes).Trim();
    }
    catch
    {
      return string.Empty;
    }
  }

  private static string ExtraerJson(string raw)
  {
    if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

    // 1) Si viene en bloque de código ```json ... ``` o ``` ... ``` extraer el contenido
    var fenceRegex = new Regex("```(?:json)?\\s*([\\s\\S]*?)```", RegexOptions.IgnoreCase);
    var fenceMatch = fenceRegex.Match(raw);
    if (fenceMatch.Success)
    {
      raw = fenceMatch.Groups[1].Value.Trim();
    }

    // 2) Recortar al primer '{' y último '}' para aislar el JSON en caso de ruido
    var first = raw.IndexOf('{');
    var last = raw.LastIndexOf('}');
    if (first >= 0 && last > first)
    {
      raw = raw.Substring(first, last - first + 1);
    }

    // 3) Validar que sea JSON bien formado
    try
    {
      using var _ = JsonDocument.Parse(raw);
      return raw;
    }
    catch
    {
      return string.Empty;
    }
  }
}
