using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : Controller
    {
        private readonly string _connectionString;

        // Construtor que recebe a configuração para obter a string de conexão com o banco de dados
        public MediaController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // Endpoint para obter todos os usuários
        [HttpGet("Listar")]
        public ActionResult<IEnumerable<Usuario>> GetUsuarios()
        {
            try
            {
                // Abre uma conexão com o banco de dados PostgreSQL
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Seleciona todos os usuários da tabela
                    using (var command = new NpgsqlCommand("SELECT * FROM censo.usuarios", connection))
                    {
                        var usuarios = new List<Usuario>();

                        // Executa a consulta e mapeia os resultados para objetos Usuario
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                usuarios.Add(new Usuario
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Nome = reader["nome"].ToString(),
                                    Raca = reader["raca"].ToString(),
                                    Renda = Convert.ToDecimal(reader["renda"])
                                });
                            }
                        }

                        // Retorna a lista de usuários como resposta HTTP 200 (OK)
                        return Ok(usuarios);
                    }
                }
            }
            catch (Exception ex)
            {
                // Retorna uma resposta HTTP 400 (BadRequest) com a mensagem de erro da exceção
                return BadRequest(ex.Message);
            }
        }

        // Endpoint para calcular a média de renda por raça
        [HttpGet("Calcular_media")]
        public IActionResult CalcularMedia()
        {
            try
            {
                // Abre uma conexão com o banco de dados PostgreSQL
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Calcula a média de renda por raça na tabela
                    using (var command = new NpgsqlCommand("SELECT LOWER(raca) AS raca, AVG(renda) AS media_renda FROM censo.usuarios GROUP BY LOWER(raca)", connection))
                    {
                        var medias = new List<MediaRenda>();

                        // Executa a consulta e mapeia os resultados para objetos MediaRenda
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                medias.Add(new MediaRenda
                                {
                                    Raca = reader["raca"].ToString(),
                                    MediaRendaValor = Math.Round(Convert.ToDecimal(reader["media_renda"]), 2)
                                });
                            }
                        }

                        // Retorna a lista de médias como resposta HTTP 200 (OK)
                        return Ok(medias);
                    }
                }
            }
            catch (Exception ex)
            {
                // Retorna uma resposta HTTP 400 (BadRequest) com a mensagem de erro da exceção
                return BadRequest(ex.Message);
            }
        }
        // Endpoint para deletar um usuário por ID
        [HttpDelete("Deletar")]
        public IActionResult DeleteUsuario(int id)
        {
            try
            {
                // Abre uma conexão com o banco de dados PostgreSQL
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Exclui um usuário da tabela
                    using (var command = new NpgsqlCommand("DELETE FROM censo.usuarios WHERE id = @id", connection))
                    {
                        // Adiciona parâmetro para o ID à consulta SQL
                        command.Parameters.AddWithValue("@id", id);

                        // Executa a consulta e verifica se alguma linha foi afetada
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return NotFound(); // Retorna uma resposta HTTP 404 (NotFound) se nenhum usuário for encontrado com o ID fornecido

                        // Retorna uma resposta HTTP 204 (NoContent) indicando sucesso sem conteúdo adicional
                        return NoContent();
                    }
                }
            }
            catch (Exception ex)
            {
                // Retorna uma resposta HTTP 400 (BadRequest) com a mensagem de erro da exceção
                return BadRequest(ex.Message);
            }
        }

    }
}


