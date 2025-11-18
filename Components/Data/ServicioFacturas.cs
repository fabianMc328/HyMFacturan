using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HyMFacturan.Components.Data
{
   
    public class ServicioFacturas
    {
        private readonly string _connectionString = "Data Source=mibase.db";

     
        public ServicioFacturas()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

           
            var comandoFacturas = conexion.CreateCommand();
            comandoFacturas.CommandText = @"
                CREATE TABLE IF NOT EXISTS Facturas(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha TEXT, 
                    Nombre TEXT, 
                    Total INTEGER
                )";
            comandoFacturas.ExecuteNonQuery();


            var comandoArticulos = conexion.CreateCommand();
            comandoArticulos.CommandText = @"
                CREATE TABLE IF NOT EXISTS Articulos(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre TEXT, 
                    Precio INTEGER,
                    FacturaId INTEGER,
                    FOREIGN KEY(FacturaId) REFERENCES Facturas(Id)
                )";
            comandoArticulos.ExecuteNonQuery();
        }


        public async Task ActualizarArticulo(Articulo articulo)
        {
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
            cmd.CommandText = "UPDATE Articulos SET Nombre = $Nombre, Precio = $Precio WHERE Id = $Id;";
            cmd.Parameters.AddWithValue("$Nombre", articulo.Nombre ?? string.Empty);
            cmd.Parameters.AddWithValue("$Precio", articulo.Precio);
            cmd.Parameters.AddWithValue("$Id", articulo.Id);
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task EliminarArticulo(int articuloId)
        {
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
            cmd.CommandText = "DELETE FROM Articulos WHERE Id = $Id;";
            cmd.Parameters.AddWithValue("$Id", articuloId);
            await cmd.ExecuteNonQueryAsync();
        }


        public async Task ActualizarTotalFactura( int facturaId, int nuevoTotal)
        {

            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
            cmd.CommandText = "UPDATE Facturas SET Total = $Total WHERE Id = $Id;";
            cmd.Parameters.AddWithValue("$Total", nuevoTotal);
            cmd.Parameters.AddWithValue("$Id", facturaId);

            await cmd.ExecuteNonQueryAsync();



        }

        public async Task GuardarFacturaCompleta(Factura factura, List<Articulo> articulos)
        {
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();

      
            var cmdFactura = conexion.CreateCommand();
            cmdFactura.CommandText = "INSERT INTO Facturas (Fecha, Nombre, Total) VALUES ($Fecha, $Nombre, $Total); SELECT last_insert_rowid();";
            cmdFactura.Parameters.AddWithValue("$Fecha", factura.Fecha);
            cmdFactura.Parameters.AddWithValue("$Nombre", factura.Nombre ?? string.Empty);
            cmdFactura.Parameters.AddWithValue("$Total", factura.Total);

            var nuevoFacturaId = (long)await cmdFactura.ExecuteScalarAsync();

         
            foreach (var articulo in articulos)
            {
                var cmdArticulo = conexion.CreateCommand();
                cmdArticulo.CommandText = "INSERT INTO Articulos (Nombre, Precio, FacturaId) VALUES ($Nombre, $Precio, $FacturaId);";
                cmdArticulo.Parameters.AddWithValue("$Nombre", articulo.Nombre ?? string.Empty);
                cmdArticulo.Parameters.AddWithValue("$Precio", articulo.Precio);
                cmdArticulo.Parameters.AddWithValue("$FacturaId", nuevoFacturaId); 

                await cmdArticulo.ExecuteNonQueryAsync();
            }
        }

    
        public async Task<List<Factura>> ObtenerFacturas()
        {
            var facturasList = new List<Factura>();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();

            var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT Id, Fecha, Nombre, Total FROM Facturas;";
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                facturasList.Add(new Factura
                {
                    Id = lector.GetInt32(0),
                    Fecha = lector.IsDBNull(1) ? default(DateTime) : lector.GetDateTime(1),
                    Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                    Total = lector.GetInt32(3)
                });
            }
            return facturasList;
        }
        public async Task<List<Articulo>> ObtenerArticulosPorFactura(int facturaId)
        {
            var lista = new List<Articulo>();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();

            var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT Id, Nombre, Precio, FacturaId FROM Articulos WHERE FacturaId = $fid;";
            cmd.Parameters.AddWithValue("$fid", facturaId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new Articulo
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Precio = reader.GetInt32(2),
                    FacturaId = reader.GetInt32(3)
                });
            }

            return lista;
        }



        public async Task BorrarTodas()
        {
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();

       
            var cmdArticulos = conexion.CreateCommand();
            cmdArticulos.CommandText = "DELETE FROM Articulos;";
            await cmdArticulos.ExecuteNonQueryAsync();

         
            var cmdFacturas = conexion.CreateCommand();
            cmdFacturas.CommandText = "DELETE FROM Facturas;";
            await cmdFacturas.ExecuteNonQueryAsync();

           
            var cmdSeqArt = conexion.CreateCommand();
            cmdSeqArt.CommandText = "DELETE FROM sqlite_sequence WHERE name = 'Articulos';";
            await cmdSeqArt.ExecuteNonQueryAsync();

            var cmdSeqFact = conexion.CreateCommand();
            cmdSeqFact.CommandText = "DELETE FROM sqlite_sequence WHERE name = 'Facturas';";
            await cmdSeqFact.ExecuteNonQueryAsync();
        }
        public async Task<List<Factura>> ObtenerFacturaPorAño(int año) 
        { 
        var FacturaLista= new List<Factura>();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var comando = conexion.CreateCommand();
            comando.CommandText= "SELECT Id, Fecha, Nombre, Total FROM Facturas WHERE strftime('%Y', Fecha) = $año ORDER BY Fecha;";
            comando.Parameters.AddWithValue("$año", año.ToString());
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                FacturaLista.Add(new Factura
                {
                    Id = lector.GetInt32(0),
                    Fecha = lector.IsDBNull(1) ? default(DateTime) : lector.GetDateTime(1),
                    Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                    Total = lector.GetInt32(3)
                });
            }
            return FacturaLista;

        }

      

        public async Task<DatosDashboard> ObtenerDatos()
        {
            var datos = new DatosDashboard();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();

            // 1. Total Dinero
            var cmd1 = conexion.CreateCommand();
            cmd1.CommandText = "SELECT IFNULL(SUM(Total), 0) FROM Facturas;";
            var res1 = await cmd1.ExecuteScalarAsync();
            datos.DineroTotal = res1 != null ? Convert.ToInt64(res1) : 0;

            // 2. Producto mas vendido
            var cmd2 = conexion.CreateCommand();
            cmd2.CommandText = "SELECT Nombre, COUNT(*) as Cantidad FROM Articulos GROUP BY Nombre ORDER BY Cantidad DESC LIMIT 1;";
            using (var lector = await cmd2.ExecuteReaderAsync())
            {
                if (await lector.ReadAsync())
                {
                    datos.ProductoTop = lector.GetString(0);
                    datos.CantidadProducto = lector.GetInt32(1);
                }
            }

            // 3. Mejor Cliente
            var cmd3 = conexion.CreateCommand();
            cmd3.CommandText = "SELECT Nombre FROM Facturas GROUP BY Nombre ORDER BY SUM(Total) DESC LIMIT 1;";
            var res3 = await cmd3.ExecuteScalarAsync();
            datos.ClienteTop = res3 != null ? res3.ToString() : "N/A";

            // 4. Mejor Mes
            var cmd4 = conexion.CreateCommand();
            cmd4.CommandText = "SELECT strftime('%m', Fecha) as Mes, SUM(Total) as TotalVenta FROM Facturas GROUP BY Mes ORDER BY TotalVenta DESC LIMIT 1;";
            var res4 = await cmd4.ExecuteScalarAsync();
            if (res4 != null)
            {
                int m = int.Parse(res4.ToString());
                datos.MesTop = NombreDelMes(m);
            }

            // 5. Peores 3 meses
            var cmd5 = conexion.CreateCommand();
            cmd5.CommandText = "SELECT strftime('%m', Fecha) as Mes, SUM(Total) as TotalVenta FROM Facturas GROUP BY Mes ORDER BY TotalVenta ASC LIMIT 3;";
            using (var lector = await cmd5.ExecuteReaderAsync())
            {
                while (await lector.ReadAsync())
                {
                    int m = int.Parse(lector.GetString(0));
                    long t = lector.GetInt64(1);
                    datos.MesesMalos.Add(NombreDelMes(m) + " (" + t + ")");
                }
            }

            return datos;
        }

        // Función auxiliar simple
        private string NombreDelMes(int mes)
        {
            if (mes < 1 || mes > 12) return "Desconocido";
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes);
        }






    }
}