namespace ClinicaMedicaAPI.Models;

public class Paciente
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    public string Telefone { get; set; } = string.Empty;

    public string? NumeroCarteirinha { get; set; }

    public int? ConvenioId { get; set; }

    public Convenio? Convenio { get; set; } 
    
    public int CalcularIdade()
    {
        var hoje = DateTime.Today;

        int idade = hoje.Year - DataNascimento.Year;

        if (DataNascimento.Date > hoje.AddYears(-idade))
        {
            idade--;
        }

        return idade;
    }
}