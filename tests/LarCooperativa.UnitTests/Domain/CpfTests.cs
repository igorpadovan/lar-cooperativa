using LarCooperativa.Api.Domain;

namespace LarCooperativa.UnitTests.Domain;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725")]
    [InlineData("11144477735")]
    public void TryCreate_ComCpfValido_RetornaCpf(string valor)
    {
        var cpf = Cpf.TryCreate(valor);

        Assert.NotNull(cpf);
        Assert.Equal(valor, cpf.Valor);
    }

    [Fact]
    public void TryCreate_ComMascara_NormalizaParaSomenteDigitos()
    {
        var cpf = Cpf.TryCreate("529.982.247-25");

        Assert.NotNull(cpf);
        Assert.Equal("52998224725", cpf.Valor);
    }

    [Theory]
    [InlineData("52998224724")] // segundo dígito verificador incorreto
    [InlineData("52998224735")] // primeiro dígito verificador incorreto
    public void TryCreate_ComDigitoVerificadorInvalido_RetornaNull(string valor)
    {
        Assert.Null(Cpf.TryCreate(valor));
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void TryCreate_ComTodosDigitosIguais_RetornaNull(string valor)
    {
        Assert.Null(Cpf.TryCreate(valor));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("5299822472")]   // 10 dígitos
    [InlineData("529982247251")] // 12 dígitos
    [InlineData("5299822472a")]
    public void TryCreate_ComEntradaInvalida_RetornaNull(string? valor)
    {
        Assert.Null(Cpf.TryCreate(valor));
    }
}
