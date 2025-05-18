using backend.DTOs.Users;
using backend.Repositories.Interfaces;
using MySql.Data.MySqlClient;
using System.Data;

namespace backend.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _config;

        public UserRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> CreateUserAsync(UserDTO user)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_insert_user", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("p_username", user.Username);
                    cmd.Parameters.AddWithValue("p_email", user.Email);
                    cmd.Parameters.AddWithValue("p_given_names", user.GivenNames);
                    cmd.Parameters.AddWithValue("p_p_surname", user.PSurname);
                    cmd.Parameters.AddWithValue("p_m_surname", user.MSurname ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("p_phone_number", user.PhoneNumber);
                    cmd.Parameters.AddWithValue("p_password_hash", user.Password);
                    cmd.Parameters.AddWithValue("p_password_salt", user.Salt);

                    MySqlParameter statusParam = new MySqlParameter("p_status_code", MySqlDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    MySqlParameter messageParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    int statusCode = Convert.ToInt32(statusParam.Value);
                    return statusCode == 200;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_get_user_by_email", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("p_email", email);

                    MySqlParameter statusParam = new MySqlParameter("p_status_code", MySqlDbType.Int32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    MySqlParameter messageParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                    {
                        Direction = ParameterDirection.Output
                    };

                    cmd.Parameters.Add(statusParam);
                    cmd.Parameters.Add(messageParam);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var user = new UserDTO
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                GivenNames = reader.GetString("given_names"),
                                PSurname = reader.GetString("p_surname"),
                                MSurname = reader.IsDBNull(reader.GetOrdinal("m_surname")) ? null : reader.GetString("m_surname"),
                                Email = reader.GetString("email"),
                                PhoneNumber = reader.GetString("phone_number"),
                                Password = reader.GetString("password_hash"),
                                Salt = reader.GetString("password_salt"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? (DateTime?)null : reader.GetDateTime("last_login"),
                                // Default to 0 if column doesn't exist
                                FailedAttempts = 0
                            };

                            // Try to get failed_attempts if it exists
                            try
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    if (reader.GetName(i).Equals("failed_attempts", StringComparison.OrdinalIgnoreCase))
                                    {
                                        user.FailedAttempts = reader.GetInt32(i);
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Warning: Failed to get failed_attempts: {ex.Message}");
                            }

                            return user;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IncrementFailedAttemptsAsync(int userId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    await con.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand("UPDATE users SET failed_attempts = failed_attempts + 1 WHERE id = @userId", con);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateLoginSuccessAsync(int userId)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    await con.OpenAsync();
                    MySqlCommand cmd = new MySqlCommand("UPDATE users SET last_login = CURRENT_TIMESTAMP, failed_attempts = 0 WHERE id = @userId", con);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
