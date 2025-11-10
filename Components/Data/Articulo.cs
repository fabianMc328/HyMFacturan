namespace HyMFacturan.Components.Data
{
    public class Articulo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Precio { get; set; }

        // Esta es la "llave foránea". Le dice a este artículo
        // a qué factura (por ID) pertenece.
        public int FacturaId { get; set; }
    }
}
