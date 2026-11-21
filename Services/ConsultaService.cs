
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.DTOs;
using ClinicaMedicaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicaAPI.Services;

public class ConsultaService
{
    private readonly ClinicaDbContext _context;

    public ConsultaService(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task<(Consulta? Consulta, string? Erro)>
        AgendarAsync(AgendarConsultaDto dados)
    {
        // 1. Verificar data da consulta
        if (dados.DataHora <= DateTime.Now)
            return (null, "Informe uma data futura.");

        // 2. Buscar paciente e convênio
        var paciente = await _context.Pacientes
            .Include(p => p.Convenio)
            .FirstOrDefaultAsync(p => p.Id == dados.PacienteId);

        if (paciente == null)
            return (null, "Paciente não encontrado.");

        // 3. Buscar médico e especialidades
        var medico = await _context.Medicos
            .Include(m => m.Especialidades)
            .FirstOrDefaultAsync(m => m.Id == dados.MedicoId);

        if (medico == null || !medico.Ativo)
            return (null, "Médico inexistente ou inativo.");

        // 4. Verificar especialidade
        var especialidade = await _context.Especialidades
            .FindAsync(dados.EspecialidadeId);

        if (especialidade == null)
            return (null, "Especialidade não encontrada.");

        if (!medico.PossuiEspecialidade(especialidade.Id))
            return (null, "Médico não atende essa especialidade.");

        // 5. Validar faixa etária
        int idade = paciente.CalcularIdade();

        if (!especialidade.AtendeIdade(idade))
            return (null, "Paciente fora da faixa etária.");

        // 6. Bloqueio por faltas nos últimos 90 dias
        var limite = DateTime.Now.AddDays(-90);

        int faltas = await _context.Consultas.CountAsync(c =>
            c.PacienteId == paciente.Id &&
            c.Status == StatusConsulta.Falta &&
            c.DataHora >= limite);

        if (faltas >= 3)
            return (null, "Paciente bloqueado por faltas.");

        // 7. Validar horário de expediente
        var inicio = dados.DataHora.TimeOfDay;
        var fim = inicio.Add(
            TimeSpan.FromMinutes(medico.DuracaoConsultaMin));

        if (medico.DuracaoConsultaMin <= 0 ||
            inicio < medico.InicioExpediente ||
            fim > medico.FimExpediente)
        {
            return (null, "Horário fora do expediente.");
        }

        // 8. Verificar conflitos de agendamento
        var dia = dados.DataHora.Date;

        var agendamentos = await _context.Consultas
            .Where(c =>
                c.MedicoId == medico.Id &&
                c.DataHora >= dia &&
                c.DataHora < dia.AddDays(1) &&
                (c.Status == StatusConsulta.Agendada ||
                 c.Status == StatusConsulta.Confirmada))
            .Select(c => c.DataHora)
            .ToListAsync();

        bool ocupado = agendamentos.Any(horario =>
            dados.DataHora <
                horario.AddMinutes(medico.DuracaoConsultaMin) &&
            horario <
                dados.DataHora.AddMinutes(medico.DuracaoConsultaMin));

        if (ocupado)
            return (null, "Médico já possui consulta nesse horário.");

        // 9. Verificar direito a retorno
        var ultimaConsulta = await _context.Consultas
            .Where(c =>
                c.PacienteId == paciente.Id &&
                c.MedicoId == medico.Id &&
                c.EspecialidadeId == especialidade.Id &&
                c.Status == StatusConsulta.Realizada &&
                c.DataHora < dados.DataHora)
            .OrderByDescending(c => c.DataHora)
            .FirstOrDefaultAsync();

        bool ehRetorno = ultimaConsulta != null &&
            especialidade.EstaNoPrazoDeRetorno(
                ultimaConsulta.DataHora, dados.DataHora);

        // 10. Calcular valor
        decimal valor = especialidade.ValorConsulta;

        if (ehRetorno)
        {
            valor = 0;
        }
        else if (paciente.Convenio != null &&
                 paciente.Convenio.EstaVigente(dados.DataHora))
        {
            valor -= paciente.Convenio.CalcularValorCoberto(valor);
        }

        // 11. Registrar consulta
        var consulta = new Consulta
        {
            PacienteId = paciente.Id,
            MedicoId = medico.Id,
            EspecialidadeId = especialidade.Id,
            DataHora = dados.DataHora,
            Status = StatusConsulta.Agendada,
            Valor = valor,
            EhRetorno = ehRetorno
        };

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        return (consulta, null);
    }
}
