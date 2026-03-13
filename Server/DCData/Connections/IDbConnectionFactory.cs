using System.Data;

namespace DCData.Connections;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();

    /// <summary>
    /// 비동기로 연결을 생성하고 Open한 뒤 반환. 호출 측에서 연결 수명(Dispose) 관리.
    /// </summary>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}