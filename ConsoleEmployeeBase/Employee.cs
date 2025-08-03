using Npgsql;
using System.Data;

namespace ConsoleEmployeeBase
{
    public class Employee
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public int Age => CalculateAge();

        public Employee(string fullName, DateTime dateOfBirth, string gender)
        {
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
        }

        private int CalculateAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth > today.AddYears(-age)) age--;
            return age;
        }

        public void SaveToDatabase(string connectionString)
        {
            using (var connection = new Npgsql.NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new Npgsql.NpgsqlCommand("INSERT INTO employees (full_name, date_of_birth, gender) VALUES (@fullName, @dateOfBirth, @gender)", connection))
                {
                    cmd.Parameters.AddWithValue("fullName", FullName);
                    cmd.Parameters.AddWithValue("dateOfBirth", DateOfBirth);
                    cmd.Parameters.AddWithValue("gender", Gender);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public class EmployeeGenerator
        {
            private Random _random = new Random();
            private string[] _maleFirstNames = { "Ivan", "Petr", "Dmitry", "Alexander" };
            private string[] _femaleFirstNames = { "Svetlana", "Anna", "Alexandra", "Maria" };
            private string[] _maleLastNames = { "Ivanov", "Petrov", "Sidorov", "Smirnov", "Kuznetsov", "Fedorov", "Osipov", "Filatov" };
            private string[] _femaleLastNames = { "Ivanova", "Petrova", "Sidorova", "Smirnova", "Kuznetsova", "Fedorova", "Osipova", "Filatova" };
            private string[] _maleMiddleNames = { "Ivanovich", "Petrovich", "Fedorovich", "Аlexandrovich", "Dmitrievich", "Olegovich", "Kirillovich", "Sergeevich" };
            private string[] _femaleMiddleNames = { "Ivanovna", "Petrovna", "Fedorovna", "Аlexandrovna", "Dmitrievna", "Olegovna", "Kirillovna", "Sergeevna" };


            public List<Employee> GenerateEmployees(int count)
            {
                var employees = new List<Employee>();
                var random = new Random();

                static DateTime GenerateRandomBirthDate(Random random)
                {
                    int year = random.Next(1950, 2005); // Годы от 1950 до 2005
                    int month = random.Next(1, 13);
                    int day = random.Next(1, DateTime.DaysInMonth(year, month) + 1);
                    return new DateTime(year, month, day);
                }

                for (int i = 0; i < count; i++)
                {
                    var gender = _random.Next(2) == 0 ? "Male" : "Female"; // Равномерное распределение пола

                    var FullName = "";
                    if (gender == "Male")
                    {
                        var maleFirstName = _maleFirstNames[_random.Next(_maleFirstNames.Length)];
                        var maleLastName = _maleLastNames[_random.Next(_maleLastNames.Length)];
                        var maleMiddleName = _maleMiddleNames[_random.Next(_maleMiddleNames.Length)];
                        FullName = $"{maleLastName} {maleFirstName} {maleMiddleName}";
                    }
                    else
                    {
                        var femaleFirstName = _femaleFirstNames[_random.Next(_femaleFirstNames.Length)];
                        var femaleLastName = _femaleLastNames[_random.Next(_femaleLastNames.Length)];
                        var femaleMiddleName = _femaleMiddleNames[_random.Next(_femaleMiddleNames.Length)];
                        FullName = $"{femaleLastName} {femaleFirstName} {femaleMiddleName}";
                    }
                    var birthDate = GenerateRandomBirthDate(random);

                    employees.Add(new Employee(FullName, birthDate, gender));
                }

                // Добавление 100 сотрудников с мужским полом и фамилией, начинающейся с "F"
                for (int i = 0; i < 100; i++)
                {
                    var firstName = _maleFirstNames[_random.Next(_maleFirstNames.Length)];
                    var lastNamesF = _maleLastNames.Where(name => name.StartsWith("F")).ToList();
                    var lastName = "";
                    if (lastNamesF.Count > 0)
                    {
                        var randomIndex = _random.Next(lastNamesF.Count);
                        lastName = lastNamesF[randomIndex];
                    }
                    else
                    {
                        Console.WriteLine("Нет фамилий начинаюихся на букву F");
                    }

                    var middleName = _maleMiddleNames[_random.Next(_maleMiddleNames.Length)];
                    var FullName = $"{lastName} {firstName} {middleName}";
                    var birthDate = GenerateRandomBirthDate(random);
                    var gender = "Мale";
                    employees.Add(new Employee(FullName, birthDate, gender));
                }

                return employees;
            }

            public void InsertEmployees(List<Employee> employees, string connectionString)
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    int batchSize = 1000;

                    for (int i = 0; i < employees.Count; i += batchSize)
                    {
                        using (var transaction = connection.BeginTransaction())
                        {
                            using (var cmd = new NpgsqlCommand())
                            {
                                cmd.Connection = connection;
                                cmd.Transaction = transaction;

                                cmd.CommandText = "INSERT INTO employees (full_name, date_of_birth, gender) VALUES (@fullName, @dateOfBirth, @gender)";

                                cmd.Parameters.Add(new NpgsqlParameter("fullName", NpgsqlTypes.NpgsqlDbType.Varchar));
                                cmd.Parameters.Add(new NpgsqlParameter("dateOfBirth", NpgsqlTypes.NpgsqlDbType.Date));
                                cmd.Parameters.Add(new NpgsqlParameter("gender", NpgsqlTypes.NpgsqlDbType.Varchar));

                                int endIndex = Math.Min(i + batchSize, employees.Count);

                                for (int j = i; j < endIndex; j++)
                                {
                                    var employee = employees[j];
                                    cmd.Parameters["fullName"].Value = employee.FullName;
                                    cmd.Parameters["dateOfBirth"].Value = employee.DateOfBirth;
                                    cmd.Parameters["gender"].Value = employee.Gender;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
        }
    }
}
