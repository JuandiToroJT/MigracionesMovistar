using Microsoft.AspNetCore.Mvc;
using ProyectoMigracionMovistarApi.Models;

namespace ProyectoMigracionMovistarApi.Utils
{
    /// <summary>
    /// Maneja los datos generales de todos los controladores.
    /// </summary>
    public class APIUtil : ControllerBase
    {
        protected MensajeErrorItem AdministrarExcepcion(Exception exception)
        {
            return AdministrarExcepcionSafe(exception);
        }

        public static MensajeErrorItem RecuperarExcepcionSistema(Exception exception)
        {
            return AdministrarExcepcionSafe(exception);
        }

        /// <summary>
        /// Control de cualquier tipo de excepción.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static MensajeErrorItem AdministrarExcepcionSafe(Exception exception)
        {
            MensajeErrorItem objMensajeErrorItem = new MensajeErrorItem();

            try
            {
                Exception ex;
                if (exception is AggregateException aggregateException)
                    ex = aggregateException.GetBaseException();
                else
                    ex = exception;

                if (ex is ReglasExcepcion reglasExcepcion)
                {
                    objMensajeErrorItem.CodigoError = reglasExcepcion.Codigo;
                    objMensajeErrorItem.MensajeError = reglasExcepcion.Descripcion;
                }
                else
                {
                    objMensajeErrorItem.CodigoError = "APIGE01";
                    objMensajeErrorItem.MensajeError = "Error no identificado, contacte con el administrador del servicio";
                }
            }
            catch (Exception)
            {
                objMensajeErrorItem.CodigoError = "APIGE02";
                objMensajeErrorItem.MensajeError = "Error no identificado, contacte con el administrador del servicio";
            }

            return objMensajeErrorItem;
        }
    }
}
