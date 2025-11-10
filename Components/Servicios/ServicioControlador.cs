using HyMFacturan.Components.Data;

namespace HyMFacturan.Components.Servicios
{
    public class ServicioControlador
    {
        // Instancia interna sin usar DI
        private readonly ServicioFacturas _servicioFacturas = new();

        public async Task AgregarFactura(Factura factura)
        {
            await _servicioFacturas.AgregarARTICULO(factura);
        }

        public async Task<List<Factura>> ObtenerFacturas()
        {
            return await _servicioFacturas.ObtenerFactura();
        }

        public async Task BorrarTodas()
        {
            await _servicioFacturas.BorrarTodas();
        }
    }
}