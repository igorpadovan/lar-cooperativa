namespace LarCooperativa.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>;

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
