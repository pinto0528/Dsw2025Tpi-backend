using System.Text.Json;
using Dsw2025Tpi.Data.Configurations;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

public class Dsw2025TpiContext : DbContext
{
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }

    // Tablas
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
    }

    public void Seedwork<T>(string filePath) where T : class
    {
        if (Set<T>().Any())
            return;

        var fullPath = Path.Combine(AppContext.BaseDirectory, filePath);
        Console.WriteLine($"Seedwork: Cargando archivo JSON desde: {fullPath}");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"No se encontró el archivo de seed: {fullPath}");

        var json = File.ReadAllText(fullPath);

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception("El archivo JSON está vacío.");

        List<T>? data = null;

        try
        {
            data = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al deserializar JSON: {ex.Message}");
        }

        if (data == null || data.Count == 0)
            throw new Exception("El archivo JSON no pudo deserializarse correctamente o está vacío.");

        Console.WriteLine($"Seedwork: {data.Count} objetos deserializados.");

        foreach (var entity in data)
        {
            if (entity == null)
                throw new Exception("Se encontró un objeto nulo en la lista deserializada.");

            if (entity is EntityBase baseEntity)
            {
                if (baseEntity.Id == Guid.Empty)
                    baseEntity.Id = Guid.NewGuid();
            }

            if (entity is Customer customer)
            {
                if (string.IsNullOrWhiteSpace(customer.Email))
                    throw new Exception($"Cliente inválido. Nombre: '{customer.Username ?? "(sin nombre)"}', Email vacío o nulo.");
                if (string.IsNullOrWhiteSpace(customer.Username))
                    throw new Exception($"Cliente inválido. Email: '{customer.Email}', Nombre vacío o nulo.");
            }
        }

        Set<T>().AddRange(data);
        SaveChanges();

        Console.WriteLine("Seedwork: Datos insertados correctamente.");
    }



}
