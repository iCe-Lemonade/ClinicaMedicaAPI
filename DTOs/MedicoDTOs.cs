
namespace ClinicaMedicaAPI.DTOs;

public class MedicoDto
{
    public string Nome { get; set; } = string.Empty;
    public string Crm { get; set; } = string.Empty;
    public string UfCrm { get; set; } = string.Empty;

    public TimeSpan InicioExpediente { get; set; }
    public TimeSpan FimExpediente { get; set; }

    public int DuracaoConsultaMin { get; set; }
    public bool Ativo { get; set; } = true;

    public List<int> EspecialidadeIds { get; set; } = new();
}
