namespace ProyectoMigracionMovistarApi.Utils
{
    public class ReglasExcepcion : Exception
    {
        /// <summary>
        /// Constructor de la clase ReglasExcepcion
        /// </summary>
        /// <returns></returns>
        public ReglasExcepcion(string _codigo, string _mensaje) 
        {
            Codigo = _codigo;
            Descripcion = _mensaje;
        }

        /// <summary>
        /// Código de error que identifica la regla de negocio que se ha violado
        /// </summary>
        public string Codigo { get; set; }

        /// <summary>
        /// Descripción del error que se ha producido
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Representación en cadena de la excepción de reglas
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[" + Codigo + "] : " + Descripcion;
        }
    }
}
