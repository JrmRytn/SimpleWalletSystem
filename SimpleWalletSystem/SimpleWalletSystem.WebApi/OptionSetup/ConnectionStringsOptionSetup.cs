namespace SimpleWalletSystem.WebApi.OptionSetup;
public class ConnectionStringsOptionSetup : IConfigureOptions<ConnectionStrings>
{
    private const string SectionName = "ConnectionStrings";
    private readonly IConfiguration _configuration;
    public ConnectionStringsOptionSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void Configure(ConnectionStrings options)
    {
        _configuration.GetSection(SectionName).Bind(options);
    }
}