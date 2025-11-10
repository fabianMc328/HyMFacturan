using HyMFacturan.Components.Data; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyMFacturan.Components.Servicios
{
   
    public class ServicioControlador
    {
      
        private readonly ServicioFacturas _servicioFacturas;

        
        public ServicioControlador(ServicioFacturas servicioFacturas)
        {
            _servicioFacturas = servicioFacturas;
        }


        public async Task GuardarFacturaCompleta(Factura factura, List<Articulo> articulos)
        {
            await _servicioFacturas.GuardarFacturaCompleta(factura, articulos);
        }

        public async Task<List<Factura>> ObtenerFacturas()
        {
            return await _servicioFacturas.ObtenerFacturas();
        }

        public async Task BorrarTodas()
        {
            await _servicioFacturas.BorrarTodas();
        }
    }
}