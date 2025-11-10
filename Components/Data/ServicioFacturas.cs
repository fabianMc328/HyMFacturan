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
        }
    }
}