namespace HyMFacturan.Components.Data
{
    public class Articulo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Precio { get; set; }

   
        public int FacturaId { get; set; }
    }
}
