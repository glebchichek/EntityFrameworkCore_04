using EntFram_04.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using Z.Dapper.Plus;

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
            Console.WriteLine("10. Массовое добавление данных");
            Console.WriteLine("11. Массовое обновление данных");
            Console.WriteLine("12. Массовое удаление данных");
            Console.WriteLine("13. Показать города по стране");
            Console.WriteLine("14. Показать категории по покупателю");
            Console.WriteLine("15. Показать акции по категории");
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
                case "10": RunBatchInsert(); break;
                case "11": RunBatchUpdate(); break;
                case "12": RunBatchDelete(); break;
                case "13": ShowCitiesByCountry(); break;
                case "14": ShowCategoriesByBuyer(); break;
                case "15": ShowPromotionsByCategory(); break;
                case "0": return;
                default: 
                    Console.WriteLine("Неверный выбор. Нажмите любую клавишу...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    
    static bool TestConnection()
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
    
    static void ShowAllBuyers()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var buyers = db.Query<Buyer>("select * from Buyers");
        
        foreach (var b in buyers)
        {
            Console.WriteLine($"ФИО - {b.FullName} | Дата рождения: {b.BirthDate:dd.MM.yyyy} | Пол: {b.Gender} | Email - {b.Email}");
        }
        Console.ReadKey();
    }
    
    static void ShowAllEmails()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var emails = db.Query<string>("select Email from Buyers");
        
        Console.WriteLine("Email адреса всех покупателей:");
        foreach (var email in emails) 
            Console.WriteLine($"- {email}");
        Console.ReadKey();
    }
    
    static void ShowAllCategories()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var categories = db.Query<Category>("select * from Categories");
        
        Console.WriteLine("Доступные разделы товаров:");
        foreach (var c in categories) 
            Console.WriteLine($"ID: {c.Id} | Название: {c.Name}");
        Console.ReadKey();
    }
    
    static void ShowAllPromotionalProducts()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var products = db.Query<PromotionalProduct>("select * from PromotionalProducts");
        
        foreach (var p in products)
        {
            Console.WriteLine($"Название акции - {p.Name} | Начало: {p.StartDate:dd.MM.yyyy} | Конец: {p.EndDate:dd.MM.yyyy}");
        }
        Console.ReadKey();
    }
    
    static void ShowAllCities()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var cities = db.Query<City>("select * from Cities");
        
        foreach (var c in cities) 
            Console.WriteLine($"- {c.Name}");
        Console.ReadKey();
    }
    
    static void ShowAllCountries()
    {
        Console.Clear();
        using IDbConnection db = new SqlConnection(_connectionString);
        var countries = db.Query<Country>("select * from Countries");
        
        foreach (var c in countries) 
            Console.WriteLine($"- {c.Name}");
        Console.ReadKey();
    }

    static void ShowBuyersByCity()
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
    
    static void ShowBuyersByCountry()
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
    
    static void ShowPromotionsByCountry()
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

    //Вспомогательные методы для получения айди с БД
    static int GetCountryId(string countryName)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        return db.QueryFirstOrDefault<int>("select Id from Countries where Name = @Name", new { Name = countryName });
    }

    static int GetCityId(string cityName)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        return db.QueryFirstOrDefault<int>("select Id from Cities where Name = @Name", new { Name = cityName });
    }

    static int GetCategoryId(string categoryName)
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        return db.QueryFirstOrDefault<int>("select Id from Categories where Name = @Name", new { Name = categoryName });
    }

    static void BulkInsertCountry()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Country>().Table("Countries").Identity(x => x.Id);

        var countries = new List<Country> 
        { 
            new Country { Name = "Russia" },
            new Country { Name = "Italy" } 
        };
        db.BulkInsert(countries);
        Console.WriteLine($"Добавлено стран: {countries.Count}");
    }

    static void BulkInsertCity()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<City>().Table("Cities").Identity(x => x.Id);

        int russiaId = GetCountryId("Russia");
        int italyId = GetCountryId("Italy");

        if (russiaId == 0 || italyId == 0)
        {
            Console.WriteLine("Ошибка: страны не найдены. Сначала добавьте страны.");
            return;
        }

        var cities = new List<City> 
        { 
            new City { Name = "Moscow", CountryId = russiaId },
            new City { Name = "Rome", CountryId = italyId } 
        };
        db.BulkInsert(cities);
        Console.WriteLine($"Добавлено городов: {cities.Count}");
    }

    static void BulkInsertCategory()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Category>().Table("Categories").Identity(x => x.Id);

        var categories = new List<Category> 
        { 
            new Category { Name = "Headphones" },
            new Category { Name = "Sweets" } 
        };
        db.BulkInsert(categories);
        Console.WriteLine($"Добавлено категорий: {categories.Count}");
    }

    static void BulkInsertBuyers()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Buyer>().Table("Buyers").Identity(x => x.Id);

        int moscowId = GetCityId("Moscow");
        int romeId = GetCityId("Rome");

        if (moscowId == 0 || romeId == 0)
        {
            Console.WriteLine("Ошибка: города не найдены. Сначала добавьте города.");
            return;
        }

        var buyers = new List<Buyer> 
        { 
            new Buyer { FullName = "Bones Jones", BirthDate = DateTime.Parse("1982-09-17"), Gender = "Male", Email = "bones@email.com", CityId = moscowId },
            new Buyer { FullName = "Morozova Sonya", BirthDate = DateTime.Parse("1989-09-17"), Gender = "Female", Email = "morozova@email.com", CityId = romeId } 
        };
        db.BulkInsert(buyers);
        Console.WriteLine($"Добавлено покупателей: {buyers.Count}");
    }

    static void BulkInsertPromotions()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<PromotionalProduct>().Table("PromotionalProducts").Identity(x => x.Id);

        int headphonesId = GetCategoryId("Headphones");
        int sweetsId = GetCategoryId("Sweets");
        int russiaId = GetCountryId("Russia");

        if (headphonesId == 0 || sweetsId == 0 || russiaId == 0)
        {
            Console.WriteLine("Ошибка: категории или страны не найдены.");
            return;
        }

        var promotionalProducts = new List<PromotionalProduct> 
        { 
            new PromotionalProduct { Name = "Candy Sales", CategoryId = sweetsId, CountryId = russiaId, StartDate = DateTime.Parse("2026-06-23"), EndDate = DateTime.Parse("2026-06-30") },
            new PromotionalProduct { Name = "Apple Headphones Offer", CategoryId = headphonesId, CountryId = russiaId, StartDate = DateTime.Parse("2026-07-01"), EndDate = DateTime.Parse("2026-08-01") } 
        };
        db.BulkInsert(promotionalProducts);
        Console.WriteLine($"Добавлено акций: {promotionalProducts.Count}");
    }

    static void BulkUpdateBuyers()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Buyer>().Table("Buyers");

        var existingBuyers = db.Query<Buyer>("select top 2 * from Buyers").ToList();
        if (existingBuyers.Count < 2)
        {
            Console.WriteLine("Недостаточно покупателей для обновления");
            return;
        }

        var buyers = new List<Buyer>
        {
            new Buyer { Id = existingBuyers[0].Id, FullName = "Bones Jones Updated", BirthDate = DateTime.Parse("1982-09-17"), Gender = "Male", Email = "bones_new@email.com", CityId = existingBuyers[0].CityId },
            new Buyer { Id = existingBuyers[1].Id, FullName = "Morozova Sonya Updated", BirthDate = DateTime.Parse("1989-09-17"), Gender = "Female", Email = "morozova_new@email.com", CityId = existingBuyers[1].CityId }
        };
        db.BulkUpdate(buyers);
        Console.WriteLine($"Обновлено покупателей: {buyers.Count}");
    }

    static void BulkUpdateCountries()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Country>().Table("Countries");

        
        var countries = db.Query<Country>("select * from Countries").ToList();
        if (countries.Count == 0) return;

        foreach (var country in countries)
        {
            if (country.Name == "Russia")
                country.Name = "Russian Federation";
            else if (country.Name == "Italy")
                country.Name = "Italian Republic";
        }
        
        db.BulkUpdate(countries);
        Console.WriteLine($"Обновлено стран: {countries.Count}");
    }

    static void BulkUpdateCities()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<City>().Table("Cities");

        var cities = db.Query<City>("select * from Cities").ToList();
        if (cities.Count == 0) return;

        foreach (var city in cities)
        {
            if (city.Name == "Moscow")
                city.Name = "Moscow City";
            else if (city.Name == "Rome")
                city.Name = "Rome City";
        }
        
        db.BulkUpdate(cities);
        Console.WriteLine($"Обновлено городов: {cities.Count}");
    }

    static void BulkUpdateCategories()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Category>().Table("Categories");

        var categories = db.Query<Category>("select * from Categories").ToList();
        if (categories.Count == 0) return;

        foreach (var category in categories)
        {
            if (category.Name == "Headphones")
                category.Name = "Wireless Headphones";
            else if (category.Name == "Sweets")
                category.Name = "Chocolate Sweets";
        }
        
        db.BulkUpdate(categories);
        Console.WriteLine($"Обновлено категорий: {categories.Count}");
    }

    static void BulkUpdatePromotionalProducts()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<PromotionalProduct>().Table("PromotionalProducts");

        var promotions = db.Query<PromotionalProduct>("select * from PromotionalProducts").ToList();
        if (promotions.Count == 0) return;

        foreach (var p in promotions)
        {
            if (p.Name.Contains("Candy"))
            {
                p.Name = "Candy Super Sale";
                p.EndDate = DateTime.Parse("2026-07-15");
            }
            else if (p.Name.Contains("Headphones"))
            {
                p.Name = "Headphones Discount";
                p.EndDate = DateTime.Parse("2026-09-01");
            }
        }
        
        db.BulkUpdate(promotions);
        Console.WriteLine($"Обновлено акций: {promotions.Count}");
    }

    static void BulkDeleteBuyers()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<Buyer>().Table("Buyers");

        var buyers = db.Query<Buyer>("select top 2 * from Buyers").ToList();
        if (buyers.Count == 0)
        {
            Console.WriteLine("Нет покупателей для удаления");
            return;
        }
        
        db.BulkDelete(buyers);
        Console.WriteLine($"Удалено покупателей: {buyers.Count}");
    }

    static void BulkDeleteCountries()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        
        db.Execute("delete from PromotionalProducts where CountryId in (select Id from Countries where Name in ('Russia', 'Italy'))");
        db.Execute("delete from Buyers where CityId in (select Id from Cities where CountryId in (select Id from Countries where Name in ('Russia', 'Italy')))");
        db.Execute("delete from Cities where CountryId in (select Id from Countries where Name in ('Russia', 'Italy'))");
        
        DapperPlusManager.Entity<Country>().Table("Countries");
        var countries = db.Query<Country>("select * from Countries where Name in ('Russia', 'Italy')").ToList();
        
        db.BulkDelete(countries);
        Console.WriteLine($"Удалено стран: {countries.Count}");
    }

    static void BulkDeleteCities()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        
        db.Execute("delete from Buyers where CityId in (select Id from Cities where Name in ('Moscow', 'Rome'))");
        
        DapperPlusManager.Entity<City>().Table("Cities");
        var cities = db.Query<City>("select * from Cities where Name in ('Moscow', 'Rome')").ToList();
        
        db.BulkDelete(cities);
        Console.WriteLine($"Удалено городов: {cities.Count}");
    }

    static void BulkDeleteCategories()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        
        db.Execute("delete from PromotionalProducts where CategoryId in (select Id from Categories where Name in ('Headphones', 'Sweets'))");
        
        DapperPlusManager.Entity<Category>().Table("Categories");
        var categories = db.Query<Category>("select * from Categories where Name in ('Headphones', 'Sweets')").ToList();
        
        db.BulkDelete(categories);
        Console.WriteLine($"Удалено категорий: {categories.Count}");
    }

    static void BulkDeletePromotionalProducts()
    {
        using IDbConnection db = new SqlConnection(_connectionString);
        DapperPlusManager.Entity<PromotionalProduct>().Table("PromotionalProducts");

        var promotions = db.Query<PromotionalProduct>("select * from PromotionalProducts").ToList();
        if (promotions.Count == 0)
        {
            Console.WriteLine("Нет акций для удаления");
            return;
        }
        
        db.BulkDelete(promotions);
        Console.WriteLine($"Удалено акций: {promotions.Count}");
    }

    static void ShowCitiesByCountry()
    {
        Console.Clear();
        Console.Write("Введите название страны: ");
        string? countryName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(countryName)) 
            return;

        using IDbConnection db = new SqlConnection(_connectionString);
        string sql = @"select c.* from Cities c
                       join Countries co on c.CountryId = co.Id
                       where co.Name = @Country";

        var cities = db.Query<City>(sql, new { Country = countryName });

        Console.WriteLine($"\nГорода из страны {countryName}:");
        if (!cities.Any())
            Console.WriteLine("Ничего не найдено.");
        
        foreach (var c in cities)
            Console.WriteLine($"- {c.Name}");
        Console.ReadKey();
    }

    static void ShowCategoriesByBuyer()
    {
        Console.Clear();
        Console.Write("Введите имя покупателя: ");
        string? buyerName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(buyerName)) 
            return;
        using IDbConnection db = new SqlConnection(_connectionString);
        string sql = @"select distinct cat.* 
               from Categories cat
               join PromotionalProducts p ON cat.Id = p.CategoryId
               join Countries co ON p.CountryId = co.Id
               join Cities ci ON co.Id = ci.CountryId
               join Buyers b ON ci.Id = b.CityId
               where b.FullName = @FullName";
        var categories = db.Query<Category>(sql, new { FullName = buyerName });

        Console.WriteLine($"\nРазделы покупателя {buyerName}:");
        if (!categories.Any())
            Console.WriteLine("Ничего не найдено.");
        
        foreach (var c in categories)
            Console.WriteLine($"- {c.Name}");
        Console.ReadKey();
    }

    static void ShowPromotionsByCategory()
    {
        Console.Clear();
        Console.Write("Введите название раздела: ");
        string? categoryName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(categoryName)) 
            return;
        using IDbConnection db = new SqlConnection(_connectionString);
        string sql = @"select p.* from PromotionalProducts p
                        join Categories c on c.Id = p.CategoryId
                       where c.Name = @Name";
        var promotions = db.Query<PromotionalProduct>(sql, new { Name = categoryName });

        Console.WriteLine($"\nАкции раздела {categoryName}:");
        if (!promotions.Any())
            Console.WriteLine("Ничего не найдено.");
        
        foreach (var p in promotions)
            Console.WriteLine($"- {p.Name} (С {p.StartDate:dd.MM.yyyy} по {p.EndDate:dd.MM.yyyy})");
        Console.ReadKey();
    }
    //вспомогательные методы для меню
    static void RunBatchInsert()
    {
        Console.Clear();
        BulkInsertCountry();
        BulkInsertCity();
        BulkInsertCategory();
        BulkInsertBuyers();
        BulkInsertPromotions();
        Console.WriteLine("Массовое добавление завершено. Нажмите любую клавишу...");
        Console.ReadKey();
    }

    static void RunBatchUpdate()
    {
        Console.Clear();
        BulkUpdateCountries();
        BulkUpdateCities();
        BulkUpdateCategories();
        BulkUpdateBuyers();
        BulkUpdatePromotionalProducts();
        Console.WriteLine("Массовое обновление завершено. Нажмите любую клавишу...");
        Console.ReadKey();
    }

    static void RunBatchDelete()
    {
        Console.Clear();
        BulkDeletePromotionalProducts();
        BulkDeleteCategories();
        BulkDeleteBuyers();
        BulkDeleteCities();
        BulkDeleteCountries();
        Console.WriteLine("Массовое удаление завершено. Нажмите любую клавишу...");
        Console.ReadKey();
    }
}