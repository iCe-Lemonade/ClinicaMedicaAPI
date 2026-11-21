
namespace ClinicaMedicaAPI.Models;

public class Convenio
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string RegistroAns { get; set; } = string.Empty;

    public decimal PercentualCobertura { get; set; }

    public DateTime FimVigencia { get; set; }

    // Verifica se o contrato está vigente
    public bool EstaVigente(DateTime data)
    {
        return data.Date <= FimVigencia.Date;
    }

    // Calcula quanto o convênio cobre
    public decimal CalcularValorCoberto(decimal valor)
    {
        return Math.Round(
            valor * PercentualCobertura / 100m,
            2,
            MidpointRounding.AwayFromZero);
    }
}
