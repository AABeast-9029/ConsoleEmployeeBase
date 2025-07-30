using ConsoleEmployeeBase;
using Npgsql;
using System.Data.Common;
using static ConsoleEmployeeBase.Employee;

class Program
{
    private const string ConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=pdbadmin;Database=ConsoleEmployeeBase;";

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Такого режима не существует.");
            return;
        }

        switch (args[0])
        {
            case "myApp1":
                CreateEmployeeTable();
                break;
            case "myApp2":
                if (args[0].Length != 6)
                {
                    Console.WriteLine("Пожалуйста, предоставьте корректные данные: myApp2 \\\"Full Name\\\" \\\"Birth Date\\\" \\\"Gender\\\"\"");
                    return;
                }
                var employee = new Employee(args[1], DateTime.Parse(args[2]), args[3]);
                employee.SaveToDatabase(ConnectionString);
                Console.WriteLine("Сотрудник добавлен в базу.");
                break;
            case "myApp3":
                DisplayEmployees();
                break;
            case "myApp4":
                GenerateEmployees(1000000);
                break;
            case "myApp5":
                QueryEmployees();
                break;
            default:
                Console.WriteLine("Такого режима не существует.");
                break;
        }
    }

    private static void CreateEmployeeTable()
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            string tableName = "employees";
            string query = $"SELECT EXISTS (SELECT 1 FROM pg_tables WHERE tablename = '{tableName}');";

            using (var command = new NpgsqlCommand(query, connection))
            {
                bool exists = (bool)command.ExecuteScalar();
                if (exists)
                {
                    Console.WriteLine($"Таблица '{tableName}' существует.");
                }
                else
                {
                    Console.WriteLine($"Таблица '{tableName}' не найдена и будет создана.");
                    using (var cmd = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS employees (id SERIAL PRIMARY KEY, full_name VARCHAR(100), date_of_birth DATE, gender VARCHAR(10))", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine($"Таблица '{tableName}' успешно создана.");
                }
            }
        }
    }

    private static void DisplayEmployees()
    {
        using (var connection = new NpgsqlConnection(ConnectionString))
        {
            connection.Open();
            using (var cmd = new NpgsqlCommand("SELECT DISTINCT full_name, date_of_birth, gender FROM employees ORDER BY full_name", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var fullName = reader.GetString(0);
                    var dateOfBirth = reader.GetDateTime(1);
                    var gender = reader.GetString(2);
                    var age = new Employee(fullName, dateOfBirth, gender).Age;
                    Console.WriteLine($"{fullName}, {dateOfBirth.ToShortDateString()}, {gender}, {age}");
                }
            }
        }
    }

    // Реализация автоматического заполнения 1.000.000 сотрудников
    private static void GenerateEmployees(int count)
    {
        Console.WriteLine("Подождите, записи добавляються в таблицу. Примерное время ожидания 1 минута 30 секунд.");
        var generator = new EmployeeGenerator();
        var employees = generator.GenerateEmployees(count);
        generator.InsertEmployees(employees, ConnectionString);
    }

    private static void QueryEmployees()
    {
        // Реализация выборки с замером времени
    }
}
