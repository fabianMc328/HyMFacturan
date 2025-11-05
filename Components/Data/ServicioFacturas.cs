using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace HyMFacturan.Components.Data
{
    public class ServicioFacturas
    {
        private List<Facturas> Facturas = new List<Facturas>();
       

        public async Task AgregarARTICULO(Facturas factura)
        {
            string ruta = "mibase.db";
            using var conexion = new SqliteConnection($"Data Source={ruta}");
            await conexion.OpenAsync();

            using var comando = conexion.CreateCommand();
            comando.CommandText = "INSERT INTO Facturas (Fecha, Nombre, Articulo, precio) VALUES ($Fecha, $Nombre, $Articulo, $precio);";
            comando.Parameters.AddWithValue("$Fecha", factura.fecha ?? string.Empty);
            comando.Parameters.AddWithValue("$Nombre", factura.Nombre ?? string.Empty);
            comando.Parameters.AddWithValue("$Articulo", factura.articulo ?? string.Empty);
            comando.Parameters.AddWithValue("$precio", factura.precio);

            await comando.ExecuteNonQueryAsync();

            Facturas.Add(factura);
        }

        public async Task<List<Facturas>> ObtenerFactura()
        {
            Facturas.Clear();
            string ruta = "mibase.db";

            using var conexion = new SqliteConnection($"Data Source={ruta}");
            await conexion.OpenAsync();

            using var comando = conexion.CreateCommand();
            comando.CommandText = "SELECT Fecha, Nombre, Articulo, precio FROM Facturas;";
            using var lector = await comando.ExecuteReaderAsync();

            while (await lector.ReadAsync())
            {
                var fecha = lector.IsDBNull(0) ? string.Empty : lector.GetString(0);
                var nombre = lector.IsDBNull(1) ? string.Empty : lector.GetString(1);
                var articulo = lector.IsDBNull(2) ? string.Empty : lector.GetString(2);
                var precio = lector.IsDBNull(3) ? 0 : lector.GetInt32(3);

                Facturas.Add(new Facturas
                {
                    fecha = fecha,
                    Nombre = nombre,
                    articulo = articulo,
                    precio = precio
                });
            }

            return Facturas;
        }

   
        public async Task BorrarTodas()
        {
            string ruta = "mibase.db";
            using var conexion = new SqliteConnection($"Data Source={ruta}");
            await conexion.OpenAsync();
            using var comando = conexion.CreateCommand();
            comando.CommandText = "DELETE FROM Facturas;";
            await comando.ExecuteNonQueryAsync();
            Facturas.Clear();
        }
    }
}