namespace SyncroBE.API.Helpers
{
    /// <summary>
    /// Conversión de fechas de filtro (zona local del negocio) a UTC.
    /// Las marcas de tiempo (PurchaseDate, EmissionDate) se guardan en UTC
    /// (DateTime.UtcNow), pero los filtros llegan como una fecha local de
    /// Costa Rica (YYYY-MM-DD). Costa Rica no observa horario de verano, por
    /// lo que el offset es fijo en UTC-6.
    /// </summary>
    public static class ReportDates
    {
        private static readonly TimeSpan CostaRicaOffset = TimeSpan.FromHours(-6);

        /// <summary>Inicio del día local (00:00 CR) expresado en UTC. Límite inferior inclusivo.</summary>
        public static DateTime StartUtc(DateTime localDate) =>
            DateTime.SpecifyKind(localDate.Date, DateTimeKind.Utc) - CostaRicaOffset;

        /// <summary>Inicio del día local siguiente (00:00 CR del día +1) en UTC. Límite superior exclusivo.</summary>
        public static DateTime EndUtcExclusive(DateTime localDate) =>
            DateTime.SpecifyKind(localDate.Date.AddDays(1), DateTimeKind.Utc) - CostaRicaOffset;
    }
}
