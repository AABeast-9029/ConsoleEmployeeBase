using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleEmployeeBase
{
    public class DatabaseManager
    {
        private readonly NpgsqlConnection _connection;

        public DatabaseManager(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
        }
    }
}
