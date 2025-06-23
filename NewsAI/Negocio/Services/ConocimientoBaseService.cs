using System.Text.Json;
using NewsAI.Dominio.Entidades.Conocimiento;
using NewsAI.Negocio.Interfaces;

public class ConocimientoBaseService : IConocimientoBaseService
{
    private readonly ILogger<ConocimientoBaseService> _logger;
    private BaseConocimiento? _baseConocimiento;
    private readonly string _rutaJsonConocimiento;

    public ConocimientoBaseService(ILogger<ConocimientoBaseService> logger)
    {
        _logger = logger;
        _rutaJsonConocimiento = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "JsonConocimiento.json");
    }

    public async Task<BaseConocimiento> CargarBaseConocimientoAsync()
    {
        try
        {
            if (_baseConocimiento != null)
                return _baseConocimiento;

            if (!File.Exists(_rutaJsonConocimiento))
            {
                _logger.LogError("No se encontró el archivo JsonConocimiento.json en: {Ruta}", _rutaJsonConocimiento);
                return new BaseConocimiento();
            }

            var jsonContent = await File.ReadAllTextAsync(_rutaJsonConocimiento);
            _baseConocimiento = JsonSerializer.Deserialize<BaseConocimiento>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Base de conocimiento cargada con {Count} contextos", _baseConocimiento?.Contextos.Count ?? 0);
            return _baseConocimiento ?? new BaseConocimiento();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la base de conocimiento");
            return new BaseConocimiento();
        }
    }

    public async Task<ResultadoClasificacion> ClasificarTextoAsync(string texto, string titulo = "")
    {
        var baseConocimiento = await CargarBaseConocimientoAsync();
        var textoCompleto = $"{titulo} {texto}".ToLowerInvariant();

        var resultado = new ResultadoClasificacion();
        var scoresPorContexto = new Dictionary<string, double>();

        foreach (var contexto in baseConocimiento.Contextos)
        {
            var score = CalcularScoreContexto(textoCompleto, contexto.Value);
            scoresPorContexto[contexto.Key] = score;

            if (score > resultado.ScoreRelevancia)
            {
                resultado.ContextoPrincipal = contexto.Key;
                resultado.Categoria = contexto.Value.Categoria;
                resultado.ScoreRelevancia = score;
            }
        }

        resultado.ScoresPorContexto = scoresPorContexto;

        // Extraer hashtags y palabras clave del contexto principal
        if (!string.IsNullOrEmpty(resultado.ContextoPrincipal))
        {
            var contextoPrincipal = baseConocimiento.Contextos[resultado.ContextoPrincipal];
            resultado.HashtagsDetectados = contextoPrincipal.Hashtags;
            resultado.PalabrasClaveEncontradas = await ExtraerPalabrasClaveAsync(textoCompleto, resultado.ContextoPrincipal);
            resultado.RazonClasificacion = GenerarRazonClasificacion(resultado, contextoPrincipal);
        }

        _logger.LogDebug("Texto clasificado como: {Contexto} con score: {Score}",
            resultado.ContextoPrincipal, resultado.ScoreRelevancia);

        return resultado;
    }

    public async Task<ResultadoFiltrado> FiltrarPorHashtagsAsync(string texto, List<string> hashtagsUsuario)
    {
        var baseConocimiento = await CargarBaseConocimientoAsync();
        var textoLower = texto.ToLowerInvariant();

        var resultado = new ResultadoFiltrado();
        double mejorScore = 0;
        var hashtagsCoincidentes = new List<string>();
        var palabrasCoincidentes = new List<string>();
        var contextosRelacionados = new List<string>();

        // Normalizar hashtags del usuario (quitar #)
        var hashtagsNormalizados = hashtagsUsuario
            .Select(h => h.Replace("#", "").ToLowerInvariant())
            .ToList();

        foreach (var contexto in baseConocimiento.Contextos)
        {
            // Verificar coincidencias de hashtags
            var coincidenciasHashtags = contexto.Value.Hashtags
                .Where(h => hashtagsNormalizados.Any(hu =>
                    h.Replace("#", "").ToLowerInvariant().Contains(hu) ||
                    hu.Contains(h.Replace("#", "").ToLowerInvariant())))
                .ToList();

            // Verificar coincidencias de palabras clave
            var coincidenciasPalabras = contexto.Value.PalabrasClaves
                .Where(p => textoLower.Contains(p.ToLowerInvariant()))
                .ToList();

            if (coincidenciasHashtags.Any() || coincidenciasPalabras.Any())
            {
                contextosRelacionados.Add(contexto.Key);

                // Calcular score considerando pesos
                double scoreContexto = CalcularScoreFiltrado(coincidenciasHashtags, coincidenciasPalabras, contexto.Value);

                if (scoreContexto > mejorScore)
                {
                    mejorScore = scoreContexto;
                    hashtagsCoincidentes = coincidenciasHashtags;
                    palabrasCoincidentes = coincidenciasPalabras;
                }
            }
        }

        resultado.EsRelevante = mejorScore > 0.3; // Umbral configurable
        resultado.ScoreCoincidencia = mejorScore;
        resultado.HashtagsCoincidentes = hashtagsCoincidentes;
        resultado.PalabrasCoincidentes = palabrasCoincidentes;
        resultado.ContextosRelacionados = contextosRelacionados;

        if (resultado.EsRelevante)
        {
            resultado.MotivoRelevancia = $"Encontradas {hashtagsCoincidentes.Count} coincidencias de hashtags y {palabrasCoincidentes.Count} palabras clave relacionadas";
        }
        else
        {
            resultado.MotivoDescarte = "No se encontraron suficientes coincidencias con los hashtags del usuario";
        }

        _logger.LogDebug("Filtrado completado. Relevante: {Relevante}, Score: {Score}",
            resultado.EsRelevante, resultado.ScoreCoincidencia);

        return resultado;
    }

    public async Task<List<string>> ObtenerContextosRelacionadosAsync(List<string> hashtags)
    {
        var baseConocimiento = await CargarBaseConocimientoAsync();
        var contextosRelacionados = new HashSet<string>();

        var hashtagsNormalizados = hashtags
            .Select(h => h.Replace("#", "").ToLowerInvariant())
            .ToList();

        foreach (var contexto in baseConocimiento.Contextos)
        {
            var tieneRelacion = contexto.Value.Hashtags.Any(h =>
                hashtagsNormalizados.Any(hu =>
                    h.Replace("#", "").ToLowerInvariant().Contains(hu) ||
                    hu.Contains(h.Replace("#", "").ToLowerInvariant())));

            if (tieneRelacion)
            {
                contextosRelacionados.Add(contexto.Key);
            }
        }

        return contextosRelacionados.ToList();
    }

    public async Task<Dictionary<string, double>> CalcularScoresPorContextoAsync(string texto)
    {
        var baseConocimiento = await CargarBaseConocimientoAsync();
        var scores = new Dictionary<string, double>();
        var textoLower = texto.ToLowerInvariant();

        foreach (var contexto in baseConocimiento.Contextos)
        {
            scores[contexto.Key] = CalcularScoreContexto(textoLower, contexto.Value);
        }

        return scores;
    }

    public async Task<List<string>> ExtraerPalabrasClaveAsync(string texto, string contexto)
    {
        var baseConocimiento = await CargarBaseConocimientoAsync();

        if (!baseConocimiento.Contextos.ContainsKey(contexto))
            return new List<string>();

        var contextoInfo = baseConocimiento.Contextos[contexto];
        var textoLower = texto.ToLowerInvariant();

        return contextoInfo.PalabrasClaves
            .Where(p => textoLower.Contains(p.ToLowerInvariant()))
            .ToList();
    }

    private double CalcularScoreContexto(string texto, ContextoConocimiento contexto)
    {
        double score = 0;
        int coincidencias = 0;

        foreach (var palabra in contexto.PalabrasClaves)
        {
            if (texto.Contains(palabra.ToLowerInvariant()))
            {
                coincidencias++;

                // Aplicar peso si existe
                int peso = contexto.Pesos.ContainsKey(palabra) ? contexto.Pesos[palabra] : 1;
                score += peso;
            }
        }

        // Normalizar score por número total de palabras del contexto
        return coincidencias > 0 ? score / contexto.PalabrasClaves.Count : 0;
    }

    private double CalcularScoreFiltrado(List<string> hashtagsCoincidentes, List<string> palabrasCoincidentes, ContextoConocimiento contexto)
    {
        double score = 0;

        // Score por hashtags (peso mayor)
        score += hashtagsCoincidentes.Count * 0.7;

        // Score por palabras clave con pesos
        foreach (var palabra in palabrasCoincidentes)
        {
            int peso = contexto.Pesos.ContainsKey(palabra) ? contexto.Pesos[palabra] : 1;
            score += peso * 0.1;
        }

        return Math.Min(score, 1.0); // Normalizar a máximo 1.0
    }

    private string GenerarRazonClasificacion(ResultadoClasificacion resultado, ContextoConocimiento contexto)
    {
        return $"Clasificado como '{resultado.ContextoPrincipal}' con {resultado.PalabrasClaveEncontradas.Count} palabras clave detectadas y score de {resultado.ScoreRelevancia:F2}";
    }
}
