using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace HyMFacturan.Components.Data
{
    public class ServicioFacturas
    {
        private readonly string _connectionString = "Data Source=mibase.db";
        private List<Factura> Facturas = new List<Factura>();

        // Constructor
        public ServicioFacturas()
        {
            InicializarBDD();
        }


        // Método para inicializar la base de datos y crear tablas si no existen
        private void InicializarBDD()
        {
            using var conexion = new SqliteConnection(_connectionString);
            conexion.Open();

            // Comando para crear la tabla Facturas
            var comandoFacturas = conexion.CreateCommand();
            comandoFacturas.CommandText = @"
                CREATE TABLE IF NOT EXISTS Facturas(
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha TEXT, 
                    Nombre TEXT, 
                    Total INTEGER
                )";
            comandoFacturas.ExecuteNonQuery();

            // Comando para crear la tabla Articulos
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

      
    }
}