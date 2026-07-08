using System.Text.Json;
using System.Text.Json.Serialization;

namespace LarCooperativa.IntegrationTests;

internal static class TestJson
{
    /// <summary>
    /// Mesmas opções da API (web + enums como string), para serializar/desserializar nos testes.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };
}
