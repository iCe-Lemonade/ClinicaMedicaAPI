
namespace ClinicaMedicaAPI.Models;

public class Especialidade
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal ValorConsulta { get; set; }

    public int IdadeMinima { get; set; }

    public int IdadeMaxima { get; set; }

    public int DiasParaRetorno { get; set; }

    // Verifica se a especialidade atende
    // a idade informada.
    public bool AtendeIdade(int idade)
    {
        return idade >= IdadeMinima &&
               idade <= IdadeMaxima;
    }

    // Verifica se a nova consulta está
    // dentro do prazo permitido para retorno.
    public bool EstaNoPrazoDeRetorno(
        DateTime ultima,
        DateTime nova)
    {
        if (nova < ultima)
            return false;

        return nova <= ultima.AddDays(DiasParaRetorno);
    }
}
