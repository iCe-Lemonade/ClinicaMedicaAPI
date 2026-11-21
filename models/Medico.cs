
namespace ClinicaMedicaAPI.Models;

public class Medico
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Crm { get; set; } = string.Empty;

    public string UfCrm { get; set; } = string.Empty;

    public TimeSpan InicioExpediente { get; set; }

    public TimeSpan FimExpediente { get; set; }

    public int DuracaoConsultaMin { get; set; }

    public bool Ativo { get; set; } = true;

    public List<Especialidade> Especialidades
        { get; set; } = new();

    // Verifica se o médico atende a especialidade
    public bool PossuiEspecialidade(int especialidadeId)
    {
        return Especialidades.Any(
            e => e.Id == especialidadeId);
    }
}
