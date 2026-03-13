# MMG_DC DI 전략

이 문서는 MMG_DC 서버 프로젝트의 **의존성 주입(DI) 전략**을 정리한 것이다.  
면접·온보딩·코드 리뷰 시 “우리 프로젝트의 DI는 이렇게 설계했다”라고 설명할 때 참고한다.

---

## 1. 원칙

| 원칙 | 설명 |
|------|------|
| **인터페이스에 의존** | API·서비스 레이어는 구체 타입이 아니라 **인터페이스**에만 의존한다. |
| **Composition Root에서만 등록** | DI 등록(바인딩)은 **호스트 한 곳**(DCWebServer `Program.cs`)에서만 한다. |
| **테스트 시 교체 가능** | 단위 테스트에서는 동일 인터페이스에 Mock/Fake를 등록해 실제 DB·외부 연동을 대체할 수 있게 한다. |
| **레이어 역전** | 고수준(웹·비즈니스)이 저수준(DB·인프라)을 직접 참조하지 않고, 인터페이스를 통해 사용한다. |

---

## 2. 수명(Service Lifetime) 정책

| 수명 | 대상 예시 | 이유 |
|------|-----------|------|
| **Singleton** | `IDbConnectionFactory`, 캐시·설정 서비스, 앱 전역 매니저 | 앱 전체에 인스턴스 하나만 필요하고, 무상태이거나 스레드 안전하게 설계된 경우. |
| **Scoped** | 요청 단위 리포지토리·서비스, `DbContext`(EF 사용 시) | HTTP 요청당 하나의 인스턴스로, 요청 종료 시 함께 정리되도록 할 때. |
| **Transient** | 무상태 헬퍼·계산기·팩토리(매번 새로 만들어도 되는 것) | 호출할 때마다 새 인스턴스가 필요하고, 상태를 공유할 필요가 없을 때. |

- **IDbConnectionFactory**  
  - Factory는 **연결 문자열만 보관**하고, `CreateConnection()` 시 **매번 새 연결**을 만든다.  
  - 따라서 **Singleton**으로 등록한다. (Scoped/Transient로 해도 동작은 하지만, 이점 없이 인스턴스만 늘어난다.)

---

## 3. 레이어별 역할

```
DCWebServer (호스트)     → DI 등록, 미들웨어, API 진입점
       ↓
DCServerCore            → 비즈니스 로직, 서비스(인터페이스·구현)
       ↓
DCData                  → DB 접근, ConnectionFactory, 리포지토리 구현
       ↓
DCProtocol              → 공유 DTO·메시지
```

- **인터페이스 위치**  
  - DB·인프라: DCData(예: `IDbConnectionFactory`).  
  - 비즈니스 서비스: DCServerCore(예: `IUserService` 등).  
- **구현체**  
  - 각 레이어 내부에 두고, **등록은 DCWebServer `Program.cs`**에서만 한다.  
- **참조 방향**  
  - DCWebServer → DCServerCore, DCData, DCProtocol.  
  - DCServerCore → DCData, DCProtocol.  
  - DCData는 외부 레이어를 참조하지 않는다(필요 시 DCProtocol만).

---

## 4. 등록 위치 및 방식

- **등록은 DCWebServer `Program.cs` 한 곳에서만 수행한다.**
- `builder.Services.Add...` 호출을 **섹션 주석**으로 구분해 가독성을 유지한다.

예시:

```csharp
// ── Infrastructure ────────────────────────────────────────────────
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

// ── Repositories ─────────────────────────────────────────────────
// builder.Services.AddScoped<IUserRepository, UserRepository>();

// ── API ──────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

- 서비스·리포지토리가 늘어나면 `AddDcData()`, `AddDcServices()` 같은 **확장 메서드**로 묶어서 한 줄로 등록하는 방식으로 확장할 수 있다.

---

## 5. 현재 프로젝트 적용 예시

| 서비스 | 수명 | 비고 |
|--------|------|------|
| `IDbConnectionFactory` → `MySqlConnectionFactory` | Singleton | 연결 문자열 보관, `CreateConnection()`으로 매번 새 연결 생성. |
| (추가 시) `IUserRepository` → `UserRepository` | Scoped 또는 Transient | 테이블·집합 단위 접근을 한 곳에서 제공. |
| (추가 시) `IXXXService` | Scoped 또는 Singleton | 비즈니스 규칙에 따라 수명 결정. |

- **DB 연결 사용 패턴**  
  - Controller/Service에서는 `IDbConnectionFactory`를 주입받고,  
  - 사용 시 `using var conn = _factory.CreateConnection();` 으로 연결을 열어 사용한 뒤 곧바로 정리한다.  
  - 연결 수명은 “요청/작업 단위”로 코드에서 제어하고, Factory 자체는 Singleton으로 둔다.

---

## 6. 테이블·리포지토리 확장 시 (선택)

- “테이블마다 뭔가를 해서 애플리케이션 개발자가 쓰기 쉽게 한다”는 전략은 예를 들어 다음과 같이 적용할 수 있다.  
  - **테이블(또는 집합) 단위로 리포지토리 인터페이스** (`IUserRepository`, `IGameResultRepository` 등)를 두고,  
  - 구현체는 DCData에 두며, **Program.cs에서는 인터페이스–구현체만 Scoped/Transient로 등록**한다.  
- 이렇게 하면 애플리케이션 레이어(DCServerCore, DCWebServer)는 **테이블 구조를 모른 채** 리포지토리 메서드만 호출해 사용할 수 있다.  
- MMG_DC에서는 필요해질 때 위 원칙과 수명 정책에 맞춰 리포지토리/서비스를 추가하면 된다.

---

*문서 버전: 1.0 (MMG_DC 서버 구조 기준)*
