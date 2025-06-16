namespace ProyectoMigracionMovistarApi.Utils
{
    /// <summary>
    /// Constantes para el proyecto de migraciones de Movistar a Tigo o Claro
    /// </summary>
    public static class Constantes
    {
        public const int CodigoMovistar = 1;
        public const int CodigoTigo = 2;
        public const int CodigoClaro = 3;

        public const int CantidadHilos = 10;
        public const int TamañoLote = 500;

        public static readonly List<string> PrefijosParaTigo = new()
        {
            "315",
            "321"
        };
    }
}
