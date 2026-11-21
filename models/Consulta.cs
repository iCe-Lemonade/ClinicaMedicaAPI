
namespace ClinicaMedicaAPI.Models;

public class Consulta
{
    public int Id { get; set; }

    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;

    public int MedicoId { get; set; }
    public Medico Medico { get; set; } = null!;

    public int EspecialidadeId { get; set; }
    public Especialidade Especialidade { get; set; } = null!;

    public DateTime DataHora { get; set; }

    public StatusConsulta Status { get; set; }
        = StatusConsulta.Agendada;

    public decimal Valor { get; set; }

    public bool EhRetorno { get; set; }

    public string? Observacoes { get; set; }

    public string? MotivoCancelamento { get; set; }

    // Verifica se a consulta pode ser cancelada
    public bool PodeSerCancelada(DateTime agora)
    {
        return (Status == StatusConsulta.Agendada ||
                Status == StatusConsulta.Confirmada)
            && DataHora - agora >= TimeSpan.FromHours(24);
    }

    // Cancela a consulta
    public void Cancelar(string motivo)
    {
        if (!PodeSerCancelada(DateTime.Now))
            throw new InvalidOperationException(
                "Consulta fora do prazo de cancelamento.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException(
                "Informe o motivo do cancelamento.");

        MotivoCancelamento = motivo;
        Status = StatusConsulta.Cancelada;
    }

    // Registra ausência do paciente
    public void RegistrarFalta()
    {
        if (Status != StatusConsulta.Agendada &&
            Status != StatusConsulta.Confirmada)
            throw new InvalidOperationException(
                "Status não permite registrar falta.");

        if (DateTime.Now < DataHora)
            throw new InvalidOperationException(
                "A consulta ainda não começou.");

        Status = StatusConsulta.Falta;
    }

    // Finaliza o atendimento
    public void Finalizar(string observacoes)
    {
        if (Status != StatusConsulta.Agendada &&
            Status != StatusConsulta.Confirmada)
            throw new InvalidOperationException(
                "Status não permite finalizar a consulta.");

        Observacoes = observacoes;
        Status = StatusConsulta.Realizada;
    }
}
