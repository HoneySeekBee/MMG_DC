using DCData.Connections;
using SqlKata.Execution;
using SqlKata.Compilers;

namespace DCData.Querying;

public sealed class QueryFactoryProvider : IQueryFactoryProvider
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public QueryFactoryProvider(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<QueryFactory> CreateAsync()
    {
        var connection = await _connectionFactory.CreateConnectionAsync();
        var compiler = new MySqlCompiler();

        return new QueryFactory(connection, compiler);
    }
}