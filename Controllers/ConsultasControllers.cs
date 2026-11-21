
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.DTOs;
using ClinicaMedicaAPI.Models;
using ClinicaMedicaAPI.Services;

namespace ClinicaMedicaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultasController : ControllerBase
{
    private readonly ClinicaDbContext _context;
    private readonly ConsultaService _service;

    public ConsultasController(
        ClinicaDbContext context,
        ConsultaService service)
    {
        _context = context;
        _service = service;
    }

    // READ - Listar todas as consultas
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var consultas = await _context.Consultas
            .AsNoTracking()
            .OrderBy(c => c.DataHora)
            .Select(c => new
            {
                c.Id,
                c.PacienteId,
                Paciente = c.Paciente.Nome,
                c.MedicoId,
                Medico = c.Medico.Nome,
                c.EspecialidadeId,
                Especialidade = c.Especialidade.Nome,
                c.DataHora,
                c.Status,
                c.Valor,
                c.EhRetorno,
                c.Observacoes,
                c.MotivoCancelamento
            })
            .ToListAsync();

        return Ok(consultas);
    }

    // READ - Buscar consulta por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var consulta = await _context.Consultas
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.PacienteId,
                Paciente = c.Paciente.Nome,
                c.MedicoId,
                Medico = c.Medico.Nome,
                c.EspecialidadeId,
                Especialidade = c.Especialidade.Nome,
                c.DataHora,
                c.Status,
                c.Valor,
                c.EhRetorno,
                c.Observacoes,
                c.MotivoCancelamento
            })
            .FirstOrDefaultAsync();

        if (consulta == null)
            return NotFound("Consulta não encontrada.");

        return Ok(consulta);
    }

    // CREATE - Agendar consulta
    [HttpPost]
    public async Task<IActionResult> Post(
        [FromBody] AgendarConsultaDto dados)
    {
        var (consulta, erro) =
            await _service.AgendarAsync(dados);

        if (consulta == null)
            return BadRequest(new { mensagem = erro });

        return CreatedAtAction(
            nameof(GetById),
            new { id = consulta.Id },
            new
            {
                consulta.Id,
                consulta.PacienteId,
                consulta.MedicoId,
                consulta.EspecialidadeId,
                consulta.DataHora,
                consulta.Status,
                consulta.Valor,
                consulta.EhRetorno
            });
    }

    // UPDATE - Confirmar consulta
    [HttpPatch("{id}/confirmar")]
    public async Task<IActionResult> Confirmar(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound("Consulta não encontrada.");

        if (consulta.Status != StatusConsulta.Agendada)
            return BadRequest(
                "Somente consultas agendadas podem ser confirmadas.");

        consulta.Status = StatusConsulta.Confirmada;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // UPDATE - Cancelar consulta
 
    // UPDATE - Cancelar consulta
    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(
        int id,
        [FromBody] CancelarConsultaDto dados)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound("Consulta não encontrada.");

        // Verifica se a consulta já foi cancelada
        if (consulta.Status == StatusConsulta.Cancelada)
            return BadRequest(
                "Esta consulta já foi cancelada.");

        // Verifica se o status permite cancelamento
        if (consulta.Status != StatusConsulta.Agendada &&
            consulta.Status != StatusConsulta.Confirmada)
        {
            return BadRequest(
                "O status atual não permite cancelamento.");
        }

        // Valida o motivo
        if (string.IsNullOrWhiteSpace(dados.Motivo))
            return BadRequest(
                "Informe o motivo do cancelamento.");

        if (dados.Motivo.Length > 500)
            return BadRequest(
                "O motivo deve ter no máximo 500 caracteres.");

        // Verifica a antecedência mínima
        if (!consulta.PodeSerCancelada(DateTime.Now))
            return BadRequest(
                "O cancelamento exige pelo menos " +
                "24 horas de antecedência.");

        consulta.Cancelar(dados.Motivo);

        await _context.SaveChangesAsync();

        return NoContent();
    }


    // UPDATE - Registrar falta
    [HttpPatch("{id}/falta")]
    public async Task<IActionResult> RegistrarFalta(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound("Consulta não encontrada.");

        if (consulta.Status != StatusConsulta.Agendada &&
            consulta.Status != StatusConsulta.Confirmada)
        {
            return BadRequest(
                "O status não permite registrar falta.");
        }

        if (DateTime.Now < consulta.DataHora)
            return BadRequest(
                "Não é possível registrar falta " +
                "antes do horário da consulta.");

        consulta.RegistrarFalta();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // UPDATE - Finalizar atendimento
    [HttpPatch("{id}/finalizar")]
    public async Task<IActionResult> Finalizar(
        int id,
        [FromBody] FinalizarConsultaDto dados)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound("Consulta não encontrada.");

        if (consulta.Status != StatusConsulta.Agendada &&
            consulta.Status != StatusConsulta.Confirmada)
        {
            return BadRequest(
                "Essa consulta não pode ser finalizada.");
        }

        if (DateTime.Now < consulta.DataHora)
            return BadRequest(
                "Não é possível finalizar uma consulta futura.");

        if ((dados.Observacoes?.Length ?? 0) > 2000)
            return BadRequest(
                "As observações devem ter no máximo 2000 caracteres.");

        consulta.Finalizar(dados.Observacoes ?? "");

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
