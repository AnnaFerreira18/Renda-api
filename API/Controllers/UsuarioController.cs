
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CidadãosController : ControllerBase
    {
        private readonly string _connectionString;

        // Construtor que recebe a configuração para obter a string de conexão com o banco de dados
        public CidadãosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Endpoint para criar um novo usuário
        [HttpPost("NovoUsuario")]
        public IActionResult PostUsuario([FromBody] Usuario usuario)
        {
            try
            {
                // Abre uma conexão com o banco de dados PostgreSQL
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Insere um novo usuário na tabela e retorna o ID gerado
                    using (var command = new NpgsqlCommand("INSERT INTO censo.usuarios (nome, raca, renda) VALUES (@nome, LOWER(TRIM(@raca)), @renda) RETURNING id", connection))
                    {
                        // Adiciona parâmetros à consulta SQL
                        command.Parameters.AddWithValue("@nome", usuario.Nome);
                        command.Parameters.AddWithValue("@raca", usuario.Raca);
                        command.Parameters.AddWithValue("@renda", usuario.Renda);

                        // Executa a consulta e obtém o ID gerado
                        var id = command.ExecuteScalar();
                        usuario.Id = Convert.ToInt32(id);
                    }
                }

                // Retorna uma resposta HTTP 201 (Created) com o novo usuário
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                // Retorna uma resposta HTTP 400 (BadRequest) com a mensagem de erro da exceção
                return BadRequest(ex.Message);
            }
        }

        // Endpoint para obter um usuário específico por ID
        [HttpGet("Busca por ID")]
        public ActionResult<Usuario> GetUsuario(int id)
        {
            try
            {
                // Abre uma conexão com o banco de dados PostgreSQL
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Seleciona um usuário específico com base no ID
                    using (var command = new NpgsqlCommand("SELECT * FROM censo.usuarios WHERE id = @id", connection))
                    {
                        // Adiciona parâmetro para o ID à consulta SQL
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Mapeia os resultados para um objeto Usuario
                                var usuario = new Usuario
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    Nome = reader["nome"].ToString(),
                                    Raca = reader["raca"].ToString(),
                                    Renda = Convert.ToDecimal(reader["renda"])
                                };

                                // Retorna o usuário como resposta HTTP 200 (OK)
                                return Ok(usuario);
                            }
                        }
                    }

                    // Retorna uma resposta HTTP 404 (NotFound) se nenhum usuário for encontrado com o ID fornecido
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                // Retorna uma resposta HTTP 400 (BadRequest) com a mensagem de erro da exceção
                return BadRequest(ex.Message);
            }
        }

        // Endpoint para atualizar um usuário existente
        [HttpPut("Update")]
        public IActionResult PutUsuario(int id, [FromBody] Usuario usuario)
        {
            try
            {
                // Abre uma conexão com o banco de dados PostgreSQL
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Atualiza os dados do usuário na tabela
                    using (var command = new NpgsqlCommand("UPDATE censo.usuarios SET nome = @nome, raca = @raca, renda = @renda WHERE id = @id", connection))
                    {
                        // Adiciona parâmetros à consulta SQL
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@nome", usuario.Nome);
                        command.Parameters.AddWithValue("@raca", usuario.Raca);
                        command.Parameters.AddWithValue("@renda", usuario.Renda);

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
