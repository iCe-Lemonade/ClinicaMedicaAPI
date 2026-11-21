
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.Models;

namespace ClinicaMedicaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EspecialidadesController : ControllerBase
{
    private readonly ClinicaDbContext _context;

    public EspecialidadesController(ClinicaDbContext context)
    {
        _context = context;
    }

    // READ - Listar especialidades
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Especialidade>>> Get()
    {
        return Ok(await _context.Especialidades
            .AsNoTracking()
            .ToListAsync());
    }

    // READ - Buscar especialidade pelo ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Especialidade>> Get(int id)
    {
        var especialidade =
            await _context.Especialidades.FindAsync(id);

        if (especialidade == null)
            return NotFound("Especialidade não encontrada.");

        return Ok(especialidade);
    }

    // CREATE - Cadastrar especialidade
    [HttpPost]
    public async Task<ActionResult<Especialidade>> Post(
        Especialidade especialidade)
    {
        if (!DadosValidos(especialidade))
            return BadRequest("Dados da especialidade inválidos.");

        if (await _context.Especialidades
            .AnyAsync(e => e.Nome == especialidade.Nome))
        {
            return Conflict("Especialidade já cadastrada.");
        }

        especialidade.Id = 0;

        _context.Especialidades.Add(especialidade);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Get),
            new { id = especialidade.Id },
            especialidade);
    }

    // UPDATE - Atualizar especialidade
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(
        int id, Especialidade dados)
    {
        var especialidade =
            await _context.Especialidades.FindAsync(id);

        if (especialidade == null)
            return NotFound("Especialidade não encontrada.");

        if (!DadosValidos(dados))
            return BadRequest("Dados da especialidade inválidos.");

        if (await _context.Especialidades.AnyAsync(
            e => e.Nome == dados.Nome && e.Id != id))
        {
            return Conflict("Especialidade já cadastrada.");
        }

        especialidade.Nome = dados.Nome;
        especialidade.ValorConsulta = dados.ValorConsulta;
        especialidade.IdadeMinima = dados.IdadeMinima;
        especialidade.IdadeMaxima = dados.IdadeMaxima;
        especialidade.DiasParaRetorno = dados.DiasParaRetorno;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - Excluir especialidade
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var especialidade =
            await _context.Especialidades.FindAsync(id);

        if (especialidade == null)
            return NotFound("Especialidade não encontrada.");

        _context.Especialidades.Remove(especialidade);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Validação dos dados
    private static bool DadosValidos(Especialidade e)
    {
        return !string.IsNullOrWhiteSpace(e.Nome)
            && e.Nome.Length <= 100
            && e.ValorConsulta >= 0
            && e.ValorConsulta <= 99999999.99m
            && e.IdadeMinima >= 0
            && e.IdadeMaxima >= e.IdadeMinima
            && e.IdadeMaxima <= 130
            && e.DiasParaRetorno >= 0
            && e.DiasParaRetorno <= 3650;
    }
}
