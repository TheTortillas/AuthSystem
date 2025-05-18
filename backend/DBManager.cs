using backend.DTOs.Users;
using MySql.Data.MySqlClient;
using System.Data;

namespace backend
{
    public class DBManager
    {
        private readonly IConfiguration _config;

        // Constructor
        public DBManager(IConfiguration config)
        {
            _config = config;
        }

        // Método para registrar un usuario
        public async Task<bool> SignUpAsync(UserDTO request)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_insert_user", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Configurar parámetros de entrada
                    cmd.Parameters.AddWithValue("p_username", request.Username);
                    cmd.Parameters.AddWithValue("p_email", request.Email);
                    cmd.Parameters.AddWithValue("p_given_names", request.GivenNames);
                    cmd.Parameters.AddWithValue("p_p_surname", request.PSurname);
                    cmd.Parameters.AddWithValue("p_m_surname", request.MSurname ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("p_phone_number", request.PhoneNumber);
                    cmd.Parameters.AddWithValue("p_password_hash", request.Password);
                    cmd.Parameters.AddWithValue("p_password_salt", request.Salt);

                    // Configurar parámetros de salida
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
                    return statusCode == 200; // Código 200 indica éxito
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        // Método para iniciar sesión
        public async Task<UserDTO?> SignInAsync(string username, string passwordHash)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_login_user", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Configurar parámetros de entrada
                    cmd.Parameters.AddWithValue("p_username", username);
                    cmd.Parameters.AddWithValue("p_password_hash", passwordHash);

                    // Configurar parámetros de salida
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
                            return new UserDTO
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                GivenNames = reader.GetString("given_names"),
                                PSurname = reader.GetString("p_surname"),
                                MSurname = reader.IsDBNull(reader.GetOrdinal("m_surname")) ? null : reader.GetString("m_surname"),
                                Email = reader.GetString("email"),
                                PhoneNumber = reader.GetString("phone_number"),
                                CreatedAt = reader.GetDateTime("created_at"),
                                LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? (DateTime?)null : reader.GetDateTime("last_login")
                            };
                        }
                    }

                    int statusCode = Convert.ToInt32(statusParam.Value);
                    if (statusCode != 200)
                    {
                        Console.WriteLine($"Error: {messageParam.Value}");
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

        public async Task<UserDTO?> FindByEmailAsync(string email)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(_config.GetConnectionString("default")))
                {
                    MySqlCommand cmd = new MySqlCommand("sp_get_user_by_email", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Configurar parámetros de entrada
                    cmd.Parameters.AddWithValue("p_email", email);

                    // Configurar parámetros de salida
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
                            return new UserDTO
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
                                LastLogin = reader.IsDBNull(reader.GetOrdinal("last_login")) ? (DateTime?)null : reader.GetDateTime("last_login")
                            };
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

        // Método para incrementar los intentos fallidos
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

        // Método para actualizar el último inicio de sesión y reiniciar intentos fallidos
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