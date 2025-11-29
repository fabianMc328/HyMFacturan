using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

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
                    Total INTEGER,
                    Archivada INTEGER DEFAULT 0
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

        public async Task CambiarEstadoArchivado(int id, bool archivar)
        {
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
     
            cmd.CommandText = "UPDATE Facturas SET Archivada = $Estado WHERE Id = $Id;";
            cmd.Parameters.AddWithValue("$Estado", archivar ? 1 : 0);
            cmd.Parameters.AddWithValue("$Id", id);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Factura>> ObtenerFacturasArchivadas()
        {
            var lista = new List<Factura>();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT Id, Fecha, Nombre, Total, Archivada FROM Facturas WHERE Archivada = 1;";
            using var lector = await cmd.ExecuteReaderAsync();
            while (await lector.ReadAsync()) lista.Add(MapearFactura(lector));
            return lista;
        }

        

        public async Task<List<Factura>> ObtenerFacturas()
        {
            var lista = new List<Factura>();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand(); 
            cmd.CommandText = "SELECT Id, Fecha, Nombre, Total, Archivada FROM Facturas WHERE Archivada = 0;";
            using var lector = await cmd.ExecuteReaderAsync();
            while (await lector.ReadAsync()) lista.Add(MapearFactura(lector));
            return lista;
        }

        public async Task<List<Factura>> ObtenerFacturaPorAño(int año)
        {
            var lista = new List<Factura>();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT Id, Fecha, Nombre, Total, Archivada FROM Facturas WHERE strftime('%Y', Fecha) = $año AND Archivada = 0 ORDER BY Fecha;";
            cmd.Parameters.AddWithValue("$año", año.ToString());
            using var lector = await cmd.ExecuteReaderAsync();
            while (await lector.ReadAsync()) lista.Add(MapearFactura(lector));
            return lista;
        }

        public async Task<DatosDashboard> ObtenerDatos()
        {
            var datos = new DatosDashboard();
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd1 = conexion.CreateCommand();
            cmd1.CommandText = "SELECT IFNULL(SUM(Total), 0) FROM Facturas WHERE Archivada = 0;";
            var res1 = await cmd1.ExecuteScalarAsync();
            datos.DineroTotal = res1 != null ? Convert.ToInt64(res1) : 0;
            var cmd2 = conexion.CreateCommand();
            cmd2.CommandText = "SELECT Nombre, COUNT(*) as Cantidad FROM Articulos GROUP BY Nombre ORDER BY Cantidad DESC LIMIT 1;";
            using (var lector = await cmd2.ExecuteReaderAsync())
            {
                if (await lector.ReadAsync()) { datos.ProductoTop = lector.GetString(0); datos.CantidadProducto = lector.GetInt32(1); }
            }

            var cmd3 = conexion.CreateCommand();
            cmd3.CommandText = "SELECT Nombre FROM Facturas WHERE Archivada = 0 GROUP BY Nombre ORDER BY SUM(Total) DESC LIMIT 1;";
            var res3 = await cmd3.ExecuteScalarAsync();
            datos.ClienteTop = res3 != null ? res3.ToString() : "N/A";

            var cmd4 = conexion.CreateCommand();
            cmd4.CommandText = "SELECT strftime('%m', Fecha) as Mes, SUM(Total) as TotalVenta FROM Facturas WHERE Archivada = 0 GROUP BY Mes ORDER BY TotalVenta DESC LIMIT 1;";
            var res4 = await cmd4.ExecuteScalarAsync();
            if (res4 != null) { datos.MesTop = NombreDelMes(int.Parse(res4.ToString())); }

            var cmd5 = conexion.CreateCommand();
            cmd5.CommandText = "SELECT strftime('%m', Fecha) as Mes, SUM(Total) as TotalVenta FROM Facturas WHERE Archivada = 0 GROUP BY Mes ORDER BY TotalVenta ASC LIMIT 3;";
            using (var lector = await cmd5.ExecuteReaderAsync())
            {
                while (await lector.ReadAsync()) { datos.MesesMalos.Add(NombreDelMes(int.Parse(lector.GetString(0))) + " (" + lector.GetInt64(1) + ")"); }
            }
            return datos;
        }

 

        private Factura MapearFactura(SqliteDataReader lector)
        {
            return new Factura
            {
                Id = lector.GetInt32(0),
                Fecha = lector.IsDBNull(1) ? default : lector.GetDateTime(1),
                Nombre = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                Total = lector.GetInt32(3),
                Archivada = !lector.IsDBNull(4) && lector.GetInt32(4) == 1
            };
        }

        private string NombreDelMes(int mes)
        {
            if (mes < 1 || mes > 12) return "Desconocido";
            return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mes).ToUpper();
        }

        public async Task GuardarFacturaCompleta(Factura factura, List<Articulo> articulos)
        {
            using var conexion = new SqliteConnection(_connectionString);
            await conexion.OpenAsync();
            var cmd = conexion.CreateCommand();
            cmd.CommandText = "INSERT INTO Facturas (Fecha, Nombre, Total, Archivada) VALUES ($Fecha, $Nombre, $Total, 0); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$Fecha", factura.Fecha);
            cmd.Parameters.AddWithValue("$Nombre", factura.Nombre);
            cmd.Parameters.AddWithValue("$Total", factura.Total);
            var id = (long)await cmd.ExecuteScalarAsync();

            foreach (var a in articulos)
            {
                var cmdA = conexion.CreateCommand();
                cmdA.CommandText = "INSERT INTO Articulos (Nombre, Precio, FacturaId) VALUES ($N, $P, $F);";
                cmdA.Parameters.AddWithValue("$N", a.Nombre);
                cmdA.Parameters.AddWithValue("$P", a.Precio);
                cmdA.Parameters.AddWithValue("$F", id);
                await cmdA.ExecuteNonQueryAsync();
            }
        }

        public async Task BorrarTodas()
        {
            using var conexion = new SqliteConnection(_connectionString); await conexion.OpenAsync();
            var c1 = conexion.CreateCommand(); c1.CommandText = "DELETE FROM Articulos"; await c1.ExecuteNonQueryAsync();
            var c2 = conexion.CreateCommand(); c2.CommandText = "DELETE FROM Facturas"; await c2.ExecuteNonQueryAsync();
            var c3 = conexion.CreateCommand(); c3.CommandText = "DELETE FROM sqlite_sequence"; await c3.ExecuteNonQueryAsync();
        }

        public async Task ActualizarTotalFactura(int fid, int t)
        {
            using var c = new SqliteConnection(_connectionString); await c.OpenAsync();
            var cmd = c.CreateCommand(); cmd.CommandText = "UPDATE Facturas SET Total=$T WHERE Id=$I";
            cmd.Parameters.AddWithValue("$T", t); cmd.Parameters.AddWithValue("$I", fid); await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Articulo>> ObtenerArticulosPorFactura(int fid)
        {
            var l = new List<Articulo>();
            using var c = new SqliteConnection(_connectionString); await c.OpenAsync();
            var cmd = c.CreateCommand(); cmd.CommandText = "SELECT Id, Nombre, Precio, FacturaId FROM Articulos WHERE FacturaId=$F";
            cmd.Parameters.AddWithValue("$F", fid);
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) l.Add(new Articulo { Id = r.GetInt32(0), Nombre = r.GetString(1), Precio = r.GetInt32(2), FacturaId = r.GetInt32(3) });
            return l;
        }

        public async Task EliminarArticulo(int id)
        {
            using var c = new SqliteConnection(_connectionString); await c.OpenAsync();
            var cmd = c.CreateCommand(); cmd.CommandText = "DELETE FROM Articulos WHERE Id=$I"; cmd.Parameters.AddWithValue("$I", id); await cmd.ExecuteNonQueryAsync();
        }

        public async Task ActualizarArticulo(Articulo a)
        {
            using var c = new SqliteConnection(_connectionString); await c.OpenAsync();
            var cmd = c.CreateCommand(); cmd.CommandText = "UPDATE Articulos SET Nombre=$N, Precio=$P WHERE Id=$I";
            cmd.Parameters.AddWithValue("$N", a.Nombre); cmd.Parameters.AddWithValue("$P", a.Precio); cmd.Parameters.AddWithValue("$I", a.Id); await cmd.ExecuteNonQueryAsync();
        }
    }
}