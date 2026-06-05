using EntFram_04.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;


namespace DapperMailList;

class Program
{
    private static string? _connectionString;

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
        
        var config = builder.Build();
        _connectionString = config.GetConnectionString("DefaultConnection");
        
        if (!TestConnection())
        {
            Console.WriteLine("Критическая ошибка: невозможно продолжить работу без подключения к БД.");
            return;
        }
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== Спилок Рассылки (Dapper) ===");
            Console.WriteLine("1. Отобразить всех покупателей");
            Console.WriteLine("2. Отобразить email всех покупателей");
            Console.WriteLine("3. Отобразить список разделов (категорий)");
            Console.WriteLine("4. Отобразить список акционных товаров");
            Console.WriteLine("5. Отобразить все города");
            Console.WriteLine("6. Отобразить все страны");
            Console.WriteLine("7. Показать покупателей из конкретного города");
            Console.WriteLine("8. Показать покупателей из конкретной страны");
            Console.WriteLine("9. Показать все акции для конкретной страны");
            Console.WriteLine("0. Выход");
            Console.Write("\nВыберите опцию: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1": ShowAllBuyers(); break;
                case "2": ShowAllEmails(); break;
                case "3": ShowAllCategories(); break;
                case "4": ShowAllPromotionalProducts(); break;
                case "5": ShowAllCities(); break;
                case "6": ShowAllCountries(); break;
                case "7": ShowBuyersByCity(); break;
                case "8": ShowBuyersByCountry(); break;
                case "9": ShowPromotionsByCountry(); break;
                case "0": return;
                default: 
                    Console.WriteLine("Неверный выбор. Нажмите любую клавишу...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    private static bool TestConnection()
    {
        Console.WriteLine("Попытка подключения к базе данных...");
        try
        {
            using IDbConnection db = new SqlConnection(_connectionString);
            db.Open();
            Console.WriteLine("Успешное подключение к базе данных «Список рассылки»!");
            Console.WriteLine("Нажмите любую клавишу для перехода в меню...");
            Console.ReadKey();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения! Сообщение: {ex.Message}");
            return false;
        }
    }
    private static void ShowAllBuyers()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var buyers = db.Query<Buyer>("SELECT * FROM Buyers");
        
        foreach (var b in buyers)
        {
            Console.WriteLine($"ФИО - {b.FullName} | Дата рождения: {b.BirthDate:dd.MM.yyyy} | Пол: {b.Gender} | Email - {b.Email}");
        }
        Console.ReadKey();
    }
    private static void ShowAllEmails()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var emails = db.Query<string>("SELECT Email FROM Buyers");
        
        Console.WriteLine("Email адреса всех покупателей:");
        foreach (var email in emails) 
            Console.WriteLine($"- {email}");
        Console.ReadKey();
    }
    private static void ShowAllCategories()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var categories = db.Query<Category>("SELECT * FROM Categories");
        
        Console.WriteLine("Доступные разделы товаров:");
        foreach (var c in categories) 
            Console.WriteLine($"ID: {c.Id} | Название: {c.Name}");
        Console.ReadKey();
    }
    private static void ShowAllPromotionalProducts()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var products = db.Query<PromotionalProduct>("SELECT * FROM PromotionalProducts");
        
        foreach (var p in products)
        {
            Console.WriteLine($"Название акции - {p.Name} | Начало: {p.StartDate:dd.MM.yyyy} | Конец: {p.EndDate:dd.MM.yyyy}");
        }
        Console.ReadKey();
    }
    private static void ShowAllCities()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var cities = db.Query<City>("SELECT * FROM Cities");
        foreach (var c in cities) 
            Console.WriteLine($"- {c.Name}");
        Console.ReadKey();
    }
    private static void ShowAllCountries()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var countries = db.Query<Country>("SELECT * FROM Countries");
        foreach (var c in countries) 
            Console.WriteLine($"- {c.Name}");
        Console.ReadKey();
    }

    private static void ShowBuyersByCity()
    {
        Console.Clear();
        Console.Write("Введите название города для фильтрации: ");
        string? cityName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(cityName))
            return;

        using IDbConnection db = new SqlConnection(_connectionString);
        string sql = @"select b.* from Buyers b
                       join Cities c on b.CityId = c.Id
                       where c.Name = @City";

        var buyers = db.Query<Buyer>(sql, new { City = cityName });

        Console.WriteLine($"\nПокупатели из города {cityName}:");
        if (!buyers.Any())
            Console.WriteLine("Никого не найдено.");

        foreach (var b in buyers)
            Console.WriteLine($"- {b.FullName} ({b.Email})");
        Console.ReadKey();
    }
    private static void ShowBuyersByCountry()
    {
        Console.Clear();
        Console.Write("Введите название страны: ");
        string? countryName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(countryName)) 
            return;

        using IDbConnection db = new SqlConnection(_connectionString);
        string sql = @"select b.* from Buyers b
                       join Cities c on b.CityId = c.Id
                       join Countries co on c.CountryId = co.Id
                       where co.Name = @Country";

        var buyers = db.Query<Buyer>(sql, new { Country = countryName });

        Console.WriteLine($"\nПокупатели из страны {countryName}:");
        if (!buyers.Any())
            Console.WriteLine("Никого не найдено.");
        
        foreach (var b in buyers)
            Console.WriteLine($"- {b.FullName} ({b.Email})");
        Console.ReadKey();
    }
    private static void ShowPromotionsByCountry()
    {
        Console.Clear();
        Console.Write("Введите название страны для поиска активных акций: ");
        string? countryName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(countryName))
            return;

        using IDbConnection db = new SqlConnection(_connectionString);
        string sql = @"select p.* from PromotionalProducts p
                       join Countries c on p.CountryId = c.Id
                       where c.Name = @Country";

        var promotions = db.Query<PromotionalProduct>(sql, new { Country = countryName });

        Console.WriteLine($"\nАкционные предложения для страны {countryName}:");
        if (!promotions.Any()) 
            Console.WriteLine("Акций нет.");

        foreach (var p in promotions)
        {
            Console.WriteLine($"- {p.Name} (С {p.StartDate:dd.MM.yyyy} по {p.EndDate:dd.MM.yyyy})");
        }
        Console.ReadKey();
    }
}