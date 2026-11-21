
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.Models;

namespace ClinicaMedicaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConveniosController : ControllerBase
{
    private readonly ClinicaDbContext _context;

    public ConveniosController(ClinicaDbContext context)
    {
        _context = context;
    }

    // READ - Listar todos os convênios
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var convenios = await _context.Convenios
            .AsNoTracking()
            .ToListAsync();

        return Ok(convenios);
    }

    // READ - Buscar convênio pelo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var convenio = await _context.Convenios.FindAsync(id);

        if (convenio == null)
            return NotFound("Convênio não encontrado.");

        return Ok(convenio);
    }

    // CREATE - Cadastrar convênio
    [HttpPost]
    public async Task<IActionResult> Post(Convenio dados)
    {
        if (!DadosValidos(dados))
            return BadRequest("Dados do convênio inválidos.");

        if (await _context.Convenios
            .AnyAsync(c => c.RegistroAns == dados.RegistroAns))
        {
            return Conflict("Registro ANS já cadastrado.");
        }

        dados.Id = 0;

        _context.Convenios.Add(dados);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = dados.Id },
            dados);
    }

    // UPDATE - Atualizar convênio
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Convenio dados)
    {
        var convenio = await _context.Convenios.FindAsync(id);

        if (convenio == null)
            return NotFound("Convênio não encontrado.");

        if (!DadosValidos(dados))
            return BadRequest("Dados do convênio inválidos.");

        if (await _context.Convenios.AnyAsync(
            c => c.Id != id &&
                 c.RegistroAns == dados.RegistroAns))
        {
            return Conflict("Registro ANS já cadastrado.");
        }

        convenio.Nome = dados.Nome;
        convenio.RegistroAns = dados.RegistroAns;
        convenio.PercentualCobertura = dados.PercentualCobertura;
        convenio.FimVigencia = dados.FimVigencia;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - Excluir convênio
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var convenio = await _context.Convenios.FindAsync(id);

        if (convenio == null)
            return NotFound("Convênio não encontrado.");

        _context.Convenios.Remove(convenio);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static bool DadosValidos(Convenio c)
    {
        return !string.IsNullOrWhiteSpace(c.Nome)
            && c.Nome.Length <= 150
            && !string.IsNullOrWhiteSpace(c.RegistroAns)
            && c.RegistroAns.Length <= 20
            && c.PercentualCobertura >= 0
            && c.PercentualCobertura <= 100
            && c.FimVigencia != default;
    }
}
