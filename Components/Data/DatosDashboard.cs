using System.Collections.Generic;

namespace HyMFacturan.Components.Data
{
    public class DatosDashboard
    {
        public long DineroTotal { get; set; }

        public string ProductoTop { get; set; } = "Sin datos";
        public int CantidadProducto { get; set; }

        public string ClienteTop { get; set; } = "Sin datos";

        public string MesTop { get; set; } = "Sin datos";

        public List<string> MesesMalos { get; set; } = new List<string>();
    }
}