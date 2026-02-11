using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VehicleBrands.Domain.Entities;

namespace VehicleBrands.Infrastructure.Data;

/// <summary>
/// Intelligent database seeder that populates the database with car brand data.
/// Only inserts records that don't already exist (checks by name).
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Seeds the database with 50+ car brands if they don't already exist.
    /// </summary>
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        logger.LogInformation("Starting intelligent database seeding...");

        var brandsToSeed = GetCarBrands();
        var addedCount = 0;
        var skippedCount = 0;

        foreach (var brand in brandsToSeed)
        {
            var exists = await context.CarBrands
                .AsNoTracking()
                .AnyAsync(b => b.Name.ToLower() == brand.Name.ToLower());

            if (!exists)
            {
                context.CarBrands.Add(brand);
                await context.SaveChangesAsync();
                addedCount++;
                logger.LogDebug("Adding brand: {BrandName}", brand.Name);
            }
            else
            {
                skippedCount++;
                logger.LogDebug("Skipping existing brand: {BrandName}", brand.Name);
            }
        }

        logger.LogInformation("Database seeding completed. Added: {Added}, Skipped: {Skipped}",
            addedCount, skippedCount);
    }

    /// <summary>
    /// Returns a collection of 50+ car brands for seeding.
    /// </summary>
    private static List<CarBrand> GetCarBrands()
    {
        return new List<CarBrand>
        {
            // Japanese Brands
            new() { Name = "Toyota", CountryOfOrigin = "Japan", FoundedYear = 1937, Website = "https://www.toyota.com", IsLuxury = false, Headquarters = "Toyota City, Aichi", IsActive = true },
            new() { Name = "Honda", CountryOfOrigin = "Japan", FoundedYear = 1948, Website = "https://www.honda.com", IsLuxury = false, Headquarters = "Tokyo", IsActive = true },
            new() { Name = "Nissan", CountryOfOrigin = "Japan", FoundedYear = 1933, Website = "https://www.nissan.com", IsLuxury = false, Headquarters = "Yokohama", IsActive = true },
            new() { Name = "Mazda", CountryOfOrigin = "Japan", FoundedYear = 1920, Website = "https://www.mazda.com", IsLuxury = false, Headquarters = "Hiroshima", IsActive = true },
            new() { Name = "Subaru", CountryOfOrigin = "Japan", FoundedYear = 1953, Website = "https://www.subaru.com", IsLuxury = false, Headquarters = "Tokyo", IsActive = true },
            new() { Name = "Mitsubishi", CountryOfOrigin = "Japan", FoundedYear = 1970, Website = "https://www.mitsubishi-motors.com", IsLuxury = false, Headquarters = "Tokyo", IsActive = true },
            new() { Name = "Lexus", CountryOfOrigin = "Japan", FoundedYear = 1989, Website = "https://www.lexus.com", IsLuxury = true, Headquarters = "Nagoya", IsActive = true },
            new() { Name = "Infiniti", CountryOfOrigin = "Japan", FoundedYear = 1989, Website = "https://www.infiniti.com", IsLuxury = true, Headquarters = "Yokohama", IsActive = true },
            new() { Name = "Acura", CountryOfOrigin = "Japan", FoundedYear = 1986, Website = "https://www.acura.com", IsLuxury = true, Headquarters = "Tokyo", IsActive = true },

            // German Brands
            new() { Name = "BMW", CountryOfOrigin = "Germany", FoundedYear = 1916, Website = "https://www.bmw.com", IsLuxury = true, Headquarters = "Munich, Bavaria", IsActive = true },
            new() { Name = "Mercedes-Benz", CountryOfOrigin = "Germany", FoundedYear = 1926, Website = "https://www.mercedes-benz.com", IsLuxury = true, Headquarters = "Stuttgart", IsActive = true },
            new() { Name = "Audi", CountryOfOrigin = "Germany", FoundedYear = 1909, Website = "https://www.audi.com", IsLuxury = true, Headquarters = "Ingolstadt", IsActive = true },
            new() { Name = "Volkswagen", CountryOfOrigin = "Germany", FoundedYear = 1937, Website = "https://www.volkswagen.com", IsLuxury = false, Headquarters = "Wolfsburg", IsActive = true },
            new() { Name = "Porsche", CountryOfOrigin = "Germany", FoundedYear = 1931, Website = "https://www.porsche.com", IsLuxury = true, Headquarters = "Stuttgart", IsActive = true },
            new() { Name = "Opel", CountryOfOrigin = "Germany", FoundedYear = 1862, Website = "https://www.opel.com", IsLuxury = false, Headquarters = "Rüsselsheim", IsActive = true },

            // American Brands
            new() { Name = "Ford", CountryOfOrigin = "United States", FoundedYear = 1903, Website = "https://www.ford.com", IsLuxury = false, Headquarters = "Dearborn, Michigan", IsActive = true },
            new() { Name = "Chevrolet", CountryOfOrigin = "United States", FoundedYear = 1911, Website = "https://www.chevrolet.com", IsLuxury = false, Headquarters = "Detroit, Michigan", IsActive = true },
            new() { Name = "Tesla", CountryOfOrigin = "United States", FoundedYear = 2003, Website = "https://www.tesla.com", IsLuxury = true, Headquarters = "Austin, Texas", IsActive = true },
            new() { Name = "Cadillac", CountryOfOrigin = "United States", FoundedYear = 1902, Website = "https://www.cadillac.com", IsLuxury = true, Headquarters = "Detroit, Michigan", IsActive = true },
            new() { Name = "Jeep", CountryOfOrigin = "United States", FoundedYear = 1941, Website = "https://www.jeep.com", IsLuxury = false, Headquarters = "Toledo, Ohio", IsActive = true },
            new() { Name = "Dodge", CountryOfOrigin = "United States", FoundedYear = 1900, Website = "https://www.dodge.com", IsLuxury = false, Headquarters = "Auburn Hills, Michigan", IsActive = true },
            new() { Name = "Chrysler", CountryOfOrigin = "United States", FoundedYear = 1925, Website = "https://www.chrysler.com", IsLuxury = false, Headquarters = "Auburn Hills, Michigan", IsActive = true },
            new() { Name = "Lincoln", CountryOfOrigin = "United States", FoundedYear = 1917, Website = "https://www.lincoln.com", IsLuxury = true, Headquarters = "Dearborn, Michigan", IsActive = true },
            new() { Name = "GMC", CountryOfOrigin = "United States", FoundedYear = 1912, Website = "https://www.gmc.com", IsLuxury = false, Headquarters = "Detroit, Michigan", IsActive = true },
            new() { Name = "Buick", CountryOfOrigin = "United States", FoundedYear = 1899, Website = "https://www.buick.com", IsLuxury = false, Headquarters = "Detroit, Michigan", IsActive = true },
            new() { Name = "Ram", CountryOfOrigin = "United States", FoundedYear = 2009, Website = "https://www.ramtrucks.com", IsLuxury = false, Headquarters = "Auburn Hills, Michigan", IsActive = true },

            // Italian Brands
            new() { Name = "Ferrari", CountryOfOrigin = "Italy", FoundedYear = 1947, Website = "https://www.ferrari.com", IsLuxury = true, Headquarters = "Maranello, Modena", IsActive = true },
            new() { Name = "Lamborghini", CountryOfOrigin = "Italy", FoundedYear = 1963, Website = "https://www.lamborghini.com", IsLuxury = true, Headquarters = "Sant'Agata Bolognese", IsActive = true },
            new() { Name = "Maserati", CountryOfOrigin = "Italy", FoundedYear = 1914, Website = "https://www.maserati.com", IsLuxury = true, Headquarters = "Modena", IsActive = true },
            new() { Name = "Fiat", CountryOfOrigin = "Italy", FoundedYear = 1899, Website = "https://www.fiat.com", IsLuxury = false, Headquarters = "Turin", IsActive = true },
            new() { Name = "Alfa Romeo", CountryOfOrigin = "Italy", FoundedYear = 1910, Website = "https://www.alfaromeo.com", IsLuxury = false, Headquarters = "Turin", IsActive = true },
            new() { Name = "Lancia", CountryOfOrigin = "Italy", FoundedYear = 1906, Website = "https://www.lancia.com", IsLuxury = false, Headquarters = "Turin", IsActive = true },

            // British Brands
            new() { Name = "Rolls-Royce", CountryOfOrigin = "United Kingdom", FoundedYear = 1904, Website = "https://www.rolls-roycemotorcars.com", IsLuxury = true, Headquarters = "Goodwood, West Sussex", IsActive = true },
            new() { Name = "Bentley", CountryOfOrigin = "United Kingdom", FoundedYear = 1919, Website = "https://www.bentleymotors.com", IsLuxury = true, Headquarters = "Crewe, England", IsActive = true },
            new() { Name = "Jaguar", CountryOfOrigin = "United Kingdom", FoundedYear = 1922, Website = "https://www.jaguar.com", IsLuxury = true, Headquarters = "Coventry, England", IsActive = true },
            new() { Name = "Land Rover", CountryOfOrigin = "United Kingdom", FoundedYear = 1948, Website = "https://www.landrover.com", IsLuxury = true, Headquarters = "Coventry, England", IsActive = true },
            new() { Name = "Aston Martin", CountryOfOrigin = "United Kingdom", FoundedYear = 1913, Website = "https://www.astonmartin.com", IsLuxury = true, Headquarters = "Gaydon, Warwickshire", IsActive = true },
            new() { Name = "McLaren", CountryOfOrigin = "United Kingdom", FoundedYear = 1963, Website = "https://www.mclaren.com", IsLuxury = true, Headquarters = "Woking, Surrey", IsActive = true },
            new() { Name = "Mini", CountryOfOrigin = "United Kingdom", FoundedYear = 1959, Website = "https://www.mini.com", IsLuxury = false, Headquarters = "Oxford, England", IsActive = true },

            // South Korean Brands
            new() { Name = "Hyundai", CountryOfOrigin = "South Korea", FoundedYear = 1967, Website = "https://www.hyundai.com", IsLuxury = false, Headquarters = "Seoul", IsActive = true },
            new() { Name = "Kia", CountryOfOrigin = "South Korea", FoundedYear = 1944, Website = "https://www.kia.com", IsLuxury = false, Headquarters = "Seoul", IsActive = true },
            new() { Name = "Genesis", CountryOfOrigin = "South Korea", FoundedYear = 2015, Website = "https://www.genesis.com", IsLuxury = true, Headquarters = "Seoul", IsActive = true },

            // French Brands
            new() { Name = "Peugeot", CountryOfOrigin = "France", FoundedYear = 1810, Website = "https://www.peugeot.com", IsLuxury = false, Headquarters = "Paris", IsActive = true },
            new() { Name = "Renault", CountryOfOrigin = "France", FoundedYear = 1899, Website = "https://www.renault.com", IsLuxury = false, Headquarters = "Boulogne-Billancourt", IsActive = true },
            new() { Name = "Citroën", CountryOfOrigin = "France", FoundedYear = 1919, Website = "https://www.citroen.com", IsLuxury = false, Headquarters = "Paris", IsActive = true },
            new() { Name = "Bugatti", CountryOfOrigin = "France", FoundedYear = 1909, Website = "https://www.bugatti.com", IsLuxury = true, Headquarters = "Molsheim", IsActive = true },
            new() { Name = "DS Automobiles", CountryOfOrigin = "France", FoundedYear = 2009, Website = "https://www.dsautomobiles.com", IsLuxury = true, Headquarters = "Paris", IsActive = true },

            // Swedish Brands
            new() { Name = "Volvo", CountryOfOrigin = "Sweden", FoundedYear = 1927, Website = "https://www.volvocars.com", IsLuxury = false, Headquarters = "Gothenburg", IsActive = true },
            new() { Name = "Koenigsegg", CountryOfOrigin = "Sweden", FoundedYear = 1994, Website = "https://www.koenigsegg.com", IsLuxury = true, Headquarters = "Ängelholm", IsActive = true },
            new() { Name = "Polestar", CountryOfOrigin = "Sweden", FoundedYear = 2017, Website = "https://www.polestar.com", IsLuxury = true, Headquarters = "Gothenburg", IsActive = true },

            // Chinese Brands
            new() { Name = "BYD", CountryOfOrigin = "China", FoundedYear = 1995, Website = "https://www.byd.com", IsLuxury = false, Headquarters = "Shenzhen", IsActive = true },
            new() { Name = "Geely", CountryOfOrigin = "China", FoundedYear = 1986, Website = "https://www.geely.com", IsLuxury = false, Headquarters = "Hangzhou", IsActive = true },
            new() { Name = "NIO", CountryOfOrigin = "China", FoundedYear = 2014, Website = "https://www.nio.com", IsLuxury = true, Headquarters = "Shanghai", IsActive = true },

            // Czech Brand
            new() { Name = "Škoda", CountryOfOrigin = "Czech Republic", FoundedYear = 1895, Website = "https://www.skoda-auto.com", IsLuxury = false, Headquarters = "Mladá Boleslav", IsActive = true },

            // Spanish Brand
            new() { Name = "SEAT", CountryOfOrigin = "Spain", FoundedYear = 1950, Website = "https://www.seat.com", IsLuxury = false, Headquarters = "Martorell", IsActive = true },

            // Romanian Brand
            new() { Name = "Dacia", CountryOfOrigin = "Romania", FoundedYear = 1966, Website = "https://www.dacia.com", IsLuxury = false, Headquarters = "Mioveni", IsActive = true }
        };
    }
}
