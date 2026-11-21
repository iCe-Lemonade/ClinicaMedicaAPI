
namespace ClinicaMedicaAPI.DTOs;

public class AgendarConsultaDto
{
    public int PacienteId { get; set; }
    public int MedicoId { get; set; }
    public int EspecialidadeId { get; set; }
    public DateTime DataHora { get; set; }
}

