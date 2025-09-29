using Microsoft.Data.SqlClient;

namespace UserManagement.Services
{
    public class UserService
    {
        private readonly string _connectionString = "Server=prod-db01;Database=UserDB;User Id=sa;Password=MySecretPassword123!;";

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                Console.WriteLine("test");

                var query = $"SELECT * FROM Users WHERE Id = {userId}";
                using var command = new SqlCommand(query, connection);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = (int)reader["Id"],
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(), // Returning password hash
                        CreatedDate = (DateTime)reader["CreatedDate"],
                        IsActive = (bool)reader["IsActive"],
                        Role = reader["Role"].ToString()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log exception somewhere
                throw ex;
            }
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = $"SELECT COUNT(*) FROM Users WHERE Username = '{username}' AND Password = '{password}'";
            using var command = new SqlCommand(query, connection);

            var result = (int)command.ExecuteScalar();
            return result > 0;
        }

        public async Task<User> CreateUserAsync(string username, string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var insertQuery = $"INSERT INTO Users (Username, Email, Password, CreatedDate, IsActive) VALUES ('{username}', '{email}', '{password}', GETDATE(), 1)";
            using var command = new SqlCommand(insertQuery, connection);

            command.ExecuteNonQuery();

            // Get the newly created user
            var selectQuery = $"SELECT TOP 1 * FROM Users WHERE Username = '{username}' ORDER BY CreatedDate DESC";
            using var selectCommand = new SqlCommand(selectQuery, connection);
            using var reader = selectCommand.ExecuteReader();

            if (reader.Read())
            {
                return new User
                {
                    Id = (int)reader["Id"],
                    Username = reader["Username"].ToString(),
                    Email = reader["Email"].ToString(),
                    Password = reader["Password"].ToString(),
                    CreatedDate = (DateTime)reader["CreatedDate"],
                    IsActive = (bool)reader["IsActive"]
                };
            }

            return null;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; }
    }
}
