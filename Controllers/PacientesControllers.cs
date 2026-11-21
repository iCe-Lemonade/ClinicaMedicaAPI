
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.Models;

namespace ClinicaMedicaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly ClinicaDbContext _context;

    public PacientesController(ClinicaDbContext context)
    {
        _context = context;
    }

    // READ - Listar todos os pacientes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Paciente>>> Get()
    {
        return Ok(await _context.Pacientes
            .AsNoTracking()
            .ToListAsync());
    }

    // READ - Buscar paciente pelo ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Paciente>> Get(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);

        if (paciente == null)
            return NotFound("Paciente não encontrado.");

        return Ok(paciente);
    }

    // CREATE - Cadastrar paciente
    [HttpPost]
    public async Task<ActionResult<Paciente>> Post(Paciente paciente)
    {
        if (string.IsNullOrWhiteSpace(paciente.Nome) ||
            string.IsNullOrWhiteSpace(paciente.Cpf) ||
            paciente.Cpf.Length != 11 ||
            paciente.DataNascimento == default ||
            paciente.DataNascimento.Date > DateTime.Today)
        {
            return BadRequest("Dados do paciente inválidos.");
        }

        if (await _context.Pacientes
            .AnyAsync(p => p.Cpf == paciente.Cpf))
        {
            return Conflict("CPF já cadastrado.");
        }

        paciente.Id = 0;

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Get),
            new { id = paciente.Id },
            paciente);
    }

    // UPDATE - Atualizar paciente
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Paciente dados)
    {
        var paciente = await _context.Pacientes.FindAsync(id);

        if (paciente == null)
            return NotFound("Paciente não encontrado.");

        if (string.IsNullOrWhiteSpace(dados.Nome) ||
            string.IsNullOrWhiteSpace(dados.Cpf) ||
            dados.Cpf.Length != 11 ||
            dados.DataNascimento == default ||
            dados.DataNascimento.Date > DateTime.Today)
        {
            return BadRequest("Dados do paciente inválidos.");
        }

        if (await _context.Pacientes
            .AnyAsync(p => p.Cpf == dados.Cpf && p.Id != id))
        {
            return Conflict("CPF já cadastrado.");
        }

        paciente.Nome = dados.Nome;
        paciente.Cpf = dados.Cpf;
        paciente.DataNascimento = dados.DataNascimento;
        paciente.Telefone = dados.Telefone;
        paciente.NumeroCarteirinha = dados.NumeroCarteirinha;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - Excluir paciente
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var paciente = await _context.Pacientes.FindAsync(id);

        if (paciente == null)
            return NotFound("Paciente não encontrado.");

        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
