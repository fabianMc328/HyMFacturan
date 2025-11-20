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

        public async Task ActualizarArticulo(Articulo articulo)
        {
            await _servicioFacturas.ActualizarArticulo(articulo);
        }

        public async Task EliminarArticulo(int articuloId)
        {
            await _servicioFacturas.EliminarArticulo(articuloId);
        }

        public async Task ActualizarTotalFactura(int facturaId, int nuevoTotal)
        {
            await _servicioFacturas.ActualizarTotalFactura(facturaId, nuevoTotal);
        }

        public async Task<List<Articulo>> ObtenerArticulosPorFactura(int facturaId)
        {
            return await _servicioFacturas.ObtenerArticulosPorFactura(facturaId);
        }

        public async Task<List<Factura>> ObtenerFacturasPorAño(int año)
        {
            return await _servicioFacturas.ObtenerFacturaPorAño(año);
        }

        // Método para el Dashboard
        public async Task<DatosDashboard> CargarDatosDashboard()
        {
            return await _servicioFacturas.ObtenerDatos();
        }
    }
}