using System.Text.Json.Serialization;

namespace SyncroBE.Application.DTOs.Hacienda
{
    public class HaciendaContributorDto
    {
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("tipoIdentificacion")]
        public string? TipoIdentificacion { get; set; }

        [JsonPropertyName("regimen")]
        public HaciendaRegimenDto? Regimen { get; set; }

        [JsonPropertyName("situacion")]
        public HaciendaSituacionDto? Situacion { get; set; }

        [JsonPropertyName("actividades")]
        public List<HaciendaActividadDto>? Actividades { get; set; }
    }

    public class HaciendaRegimenDto
    {
        [JsonPropertyName("codigo")]
        public int Codigo { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }
    }

    public class HaciendaSituacionDto
    {
        [JsonPropertyName("moroso")]
        public string? Moroso { get; set; }

        [JsonPropertyName("omiso")]
        public string? Omiso { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }

        [JsonPropertyName("administracionTributaria")]
        public string? AdministracionTributaria { get; set; }
    }

    public class HaciendaActividadDto
    {
        [JsonPropertyName("codigo")]
        public string? Codigo { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }
    }
}
