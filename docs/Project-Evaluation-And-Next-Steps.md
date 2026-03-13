# MMG_DC 프로젝트 평가 및 이후 작업 순서

## 1. 현재까지 프로젝트 평가

### ✅ 잘 갖춰진 부분

| 항목 | 상태 | 비고 |
|------|------|------|
| **솔루션·프로젝트 구조** | ✅ | DCWebServer → DCServerCore → DCData, DCProtocol 분리 적절 |
| **참조 관계** | ✅ | 웹 → 코어 → 데이터/프로토콜, 계층 역전 유지 |
| **패키지 버전** | ✅ | .NET 8.0 기준, Pomelo/EF 8.x, MySqlConnector, SqlKata, Dapper |
| **Connection** | ✅ | IDbConnectionFactory + MySqlConnectionFactory, connection string 한 곳(appsettings) |
| **Querying** | ✅ | IQueryFactoryProvider + QueryFactoryProvider (SqlKata QueryFactory 생성) |
| **DI 등록** | ✅ | Program.cs에서 Infrastructure 섹션으로 IDbConnectionFactory 등록 |
| **설정** | ✅ | appsettings.json ConnectionStrings:MainDatabase |
| **문서** | ✅ | docs/DI-Strategy.md, Cursor 규칙(확인 후 변경 등) |

### ⚠️ 보완 필요

| 항목 | 내용 |
|------|------|
| **IDbConnectionFactory 비동기** | QueryFactoryProvider가 `CreateConnectionAsync()`를 쓰는데, 현재 인터페이스에는 `CreateConnection()`만 있음. 비동기 열기 필요 시 인터페이스/구현체에 `CreateConnectionAsync()` 추가 필요. |
| **IQueryFactoryProvider DI 등록** | Program.cs에 `IQueryFactoryProvider` / `QueryFactoryProvider` 등록이 없음. SqlKata 쓸 서비스/리포지토리에서 주입받으려면 등록 필요. |
| **실제 DB** | MySQL에 `mmg_dc` DB 및 테이블 미생성. 로그인 등 기능 구현 전 DB 생성 + 마이그레이션 필요. |
| **DbContext / 엔티티** | EF Core용 DbContext·엔티티·마이그레이션 미작성. 로그인 테이블 설계 후 여기서 진행. |

### 🔲 아직 없는 것 (이후 단계)

- MySQL 실제 DB 생성 및 마이그레이션
- EF Core DbContext + 엔티티(로그인 관련 테이블)
- 리포지토리/서비스 레이어 (로그인 등)
- 로그인 API 엔드포인트 및 인증 처리

---

## 2. “지금 셋팅은 다 된 건지?” 요약

- **구조·참조·패키지·Connection·Querying·DI·설정·문서**까지는 **필요한 셋팅은 대부분 된 상태**다.
- **반드시 손보면 좋은 것**  
  - `IDbConnectionFactory`와 `QueryFactoryProvider` 시그니처 맞추기 (동기/비동기 통일).  
  - Program.cs에 `IQueryFactoryProvider` 등록 추가.  
- **이후에 할 것**  
  - MySQL DB 생성 → EF Core DbContext/엔티티/마이그레이션 → 로그인 테이블·리포지토리·API 순으로 진행.

---

## 3. 이후 작업 순서 (MySQL DB 생성 ~ 로그인 DB/API)

아래 순서대로 진행하면 정리하기 좋다.

| 순서 | 작업 | 설명 |
|------|------|------|
| **1** | MySQL 서버 확인 및 DB 생성 | 로컬 MySQL 실행, `CREATE DATABASE mmg_dc;` (또는 툴로 생성). appsettings 연결 문자열과 일치하는지 확인. |
| **2** | EF Core DbContext + 엔티티 정의 | DCData(또는 별도 프로젝트)에 DbContext 추가. 로그인용 엔티티(예: User, LoginAttempt 등) 정의. |
| **3** | 첫 마이그레이션 생성·적용 | `dotnet ef migrations add InitialCreate -p DCData -s DCWebServer` 후 `dotnet ef database update` 로 테이블 생성. |
| **4** | (선택) 로그인 테이블만 추가 마이그레이션 | 이미 InitialCreate에 포함했으면 생략. 나중에 컬럼/테이블 추가 시 마이그레이션 추가. |
| **5** | DbContext DI 등록 | Program.cs에 `AddDbContext<YourDbContext>(...)` 등록. 수명은 Scoped 권장. |
| **6** | 로그인용 리포지토리/서비스 | DCServerCore 또는 DCData에 IUserRepository / UserRepository(또는 로그인 전용 서비스). SqlKata/Dapper/EF 중 선택해 구현. |
| **7** | 로그인 API | DCWebServer 컨트롤러에서 위 서비스/리포지토리 호출, 로그인 요청/응답 처리. |
| **8** | (선택) DI 전략 문서 반영 | docs/DI-Strategy.md에 DbContext·리포지토리 수명, 로그인 흐름 한 줄씩 추가. |

---

## 4. 바로 고치면 좋은 것 (코드)

1. **IDbConnectionFactory**  
   - QueryFactoryProvider가 `CreateConnectionAsync()`를 쓰므로, 인터페이스에 `Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);` 추가.  
   - MySqlConnectionFactory에서 `new MySqlConnection(...)` 생성 후 `OpenAsync()` 호출해 반환. (호출 측에서 연결 수명 관리.)
2. **Program.cs**  
   - `builder.Services.AddSingleton<IQueryFactoryProvider, QueryFactoryProvider>();` (또는 수명 정책에 맞게 Scoped/Transient) 추가.

이 두 가지 반영 후, 위 “이후 작업 순서”대로 DB 생성 → EF → 로그인 DB/API 순으로 진행하면 된다.
