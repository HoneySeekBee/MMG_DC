using SqlKata.Execution;

namespace DCData.Querying;

public interface IQueryFactoryProvider
{
    Task<QueryFactory> CreateAsync();
}