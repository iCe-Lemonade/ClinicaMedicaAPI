
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.DTOs;
using ClinicaMedicaAPI.Models;

namespace ClinicaMedicaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicosController : ControllerBase
{
    private readonly ClinicaDbContext _context;

    public MedicosController(ClinicaDbContext context)
    {
        _context = context;
    }

    // READ - Listar médicos
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var medicos = await _context.Medicos
            .Include(m => m.Especialidades)
            .AsNoTracking()
            .ToListAsync();

        return Ok(medicos.Select(Resposta));
    }

    // READ - Buscar médico pelo ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var medico = await _context.Medicos
            .Include(m => m.Especialidades)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medico == null)
            return NotFound("Médico não encontrado.");

        return Ok(Resposta(medico));
    }

    // CREATE - Cadastrar médico
    [HttpPost]
    public async Task<IActionResult> Post(MedicoDto dados)
    {
        if (!DadosValidos(dados))
            return BadRequest("Dados do médico inválidos.");

        var uf = dados.UfCrm.Trim().ToUpperInvariant();
        var crm = dados.Crm.Trim();

        if (await _context.Medicos.AnyAsync(
            m => m.Crm == crm && m.UfCrm == uf))
        {
            return Conflict("CRM já cadastrado nesta UF.");
        }

        var ids = dados.EspecialidadeIds.Distinct().ToList();

        var especialidades = await _context.Especialidades
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        if (especialidades.Count != ids.Count)
            return BadRequest("Especialidade não encontrada.");

        var medico = new Medico
        {
            Nome = dados.Nome.Trim(),
            Crm = crm,
            UfCrm = uf,
            InicioExpediente = dados.InicioExpediente,
            FimExpediente = dados.FimExpediente,
            DuracaoConsultaMin = dados.DuracaoConsultaMin,
            Ativo = dados.Ativo,
            Especialidades = especialidades
        };

        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = medico.Id },
            Resposta(medico));
    }

    // UPDATE - Atualizar médico
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, MedicoDto dados)
    {
        var medico = await _context.Medicos
            .Include(m => m.Especialidades)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (medico == null)
            return NotFound("Médico não encontrado.");

        if (!DadosValidos(dados))
            return BadRequest("Dados do médico inválidos.");

        var uf = dados.UfCrm.Trim().ToUpperInvariant();
        var crm = dados.Crm.Trim();

        if (await _context.Medicos.AnyAsync(
            m => m.Id != id &&
                 m.Crm == crm && m.UfCrm == uf))
        {
            return Conflict("CRM já cadastrado nesta UF.");
        }

        var ids = dados.EspecialidadeIds.Distinct().ToList();

        var especialidades = await _context.Especialidades
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        if (especialidades.Count != ids.Count)
            return BadRequest("Especialidade não encontrada.");

        medico.Nome = dados.Nome.Trim();
        medico.Crm = crm;
        medico.UfCrm = uf;
        medico.InicioExpediente = dados.InicioExpediente;
        medico.FimExpediente = dados.FimExpediente;
        medico.DuracaoConsultaMin = dados.DuracaoConsultaMin;
        medico.Ativo = dados.Ativo;

        medico.Especialidades.Clear();

        foreach (var especialidade in especialidades)
            medico.Especialidades.Add(especialidade);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE - Excluir médico
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);

        if (medico == null)
            return NotFound("Médico não encontrado.");

        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Validação básica
    private static bool DadosValidos(MedicoDto m)
    {
        return !string.IsNullOrWhiteSpace(m.Nome)
            && m.Nome.Length <= 150
            && !string.IsNullOrWhiteSpace(m.Crm)
            && m.Crm.Length <= 20
            && !string.IsNullOrWhiteSpace(m.UfCrm)
            && m.UfCrm.Trim().Length == 2
            && m.InicioExpediente >= TimeSpan.Zero
            && m.FimExpediente <= TimeSpan.FromDays(1)
            && m.FimExpediente > m.InicioExpediente
            && m.DuracaoConsultaMin > 0
            && m.DuracaoConsultaMin <=
                (m.FimExpediente - m.InicioExpediente)
                    .TotalMinutes
            && m.EspecialidadeIds != null
            && m.EspecialidadeIds.Count > 0
            && m.EspecialidadeIds.All(id => id > 0);
    }

    // Formata o retorno da API
    private static object Resposta(Medico medico)
    {
        return new
        {
            medico.Id,
            medico.Nome,
            medico.Crm,
            medico.UfCrm,
            medico.InicioExpediente,
            medico.FimExpediente,
            medico.DuracaoConsultaMin,
            medico.Ativo,
            Especialidades = medico.Especialidades.Select(e => new
            {
                e.Id,
                e.Nome
            })
        };
    }
}
