# 쿠폰 발급 API 가이드
## 개요
- Redis + MySQL을 이용한 선착순 쿠폰 발급 API입니다.
- Redis는 실시간 재고/중복 제어를 담당하고, MySQL은 실제 쿠폰 코드와 사용자 발급 이력을 보관합니다.
- 쿠폰 정의는 `appsettings*.json`의 `Coupons` 섹션에서 관리합니다.

## 로컬 개발 환경 준비
1. Docker Compose로 Redis와 MySQL(필요 시 RedisInsight까지)을 기동합니다.
   ```bash
   docker compose up -d redis mysql redisinsight
   ```
   - Redis: `localhost:6379`
   - MySQL: `localhost:3306` (`app / app_password`)
2. 처음 기동하면 애플리케이션 호스트 서비스가 자동으로:
   - MySQL에 `CouponCodes`, `CouponIssueHistory` 테이블을 생성합니다.
   - 쿠폰 정의에 맞춰 실제 쿠폰 번호를 시드합니다.
   - 해당 시점에 발급 가능한 수량만큼 Redis 재고 키를 초기화합니다.
3. .NET 패키지 복구 및 빌드
   ```bash
   dotnet restore
   dotnet build
   ```
4. 애플리케이션 실행 후 API를 호출합니다.

## 설정 항목
### ConnectionStrings
| 키 | 설명 |
| --- | --- |
| `Redis` | Redis 연결 문자열 (`redis:6379` / `localhost:6379`) |
| `MySql` | MySQL 연결 문자열 (`Server=...;Port=3306;Database=CouponDb;User=app;Password=app_password;SslMode=None`) |

### Coupons.Definitions
| 필드 | 설명 |
| --- | --- |
| `Id` | 쿠폰 ID (API 입력값) |
| `DisplayName` | 쿠폰 이름 |
| `TotalQuantity` | 전체 발급 가능한 쿠폰 코드 수량 |
| `CampaignStartDate` | (선택) 이벤트 시작 날짜. 주간 배포 시 기준일 |
| `WeeklyAllocation` | (선택) 주차별 발급 수량 배열. 예: `[50, 50, 50, 50]`은 캠페인 시작일부터 4주 동안 매주 50장씩 공개 |

> `WeeklyAllocation`을 설정하면 해당 주차 시작 전에는 MSSQL에서 `AvailableFrom`이 도래하지 않아 쿠폰 코드가 할당되지 않습니다. Redis 재고도 해당 시점까지 0으로 유지됩니다.

## 데이터베이스 구조
| 테이블 | 용도 | 주요 컬럼 |
| --- | --- | --- |
| `CouponCodes` | 쿠폰 코드 풀 | `Id`, `CouponId`, `CodeValue`, `Status(0=대기,1=발급)`, `AvailableFrom`, `UserId`, `IssuedAt` |
| `CouponIssueHistory` | 발급 이력 | `CouponCodeId`, `CouponId`, `CodeValue`, `UserId`, `IssuedAt` |

- 자동 시드 시 `CodeValue`는 `쿠폰ID-주차-순번` 형식으로 생성됩니다.
- 이력 테이블은 보고/감사를 위한 Append-only 구조입니다.

## API
### POST `/coupons/issue`
- **설명**: 주어진 `couponId`를 선착순으로 발급합니다.
- **요청 예시**
  ```http
  POST /coupons/issue
  Content-Type: application/json

  {
    "couponId": "WELCOME10",
    "userId": "user-123"
  }
  ```
- **성공 응답 (200)**
  ```json
  {
    "success": true,
    "message": "쿠폰 WELCOME10 발급이 완료되었습니다.",
    "remainingQuantity": 49,
    "couponCode": "WELCOME10-01-0001",
    "issuedAt": "2024-11-04T09:00:00+00:00"
  }
  ```
- **주요 실패 케이스**

  | HTTP | 메시지 | 설명 |
  | --- | --- | --- |
  | 404 | `쿠폰 ... 정보를 찾을 수 없습니다.` | 쿠폰 ID 미정의 |
  | 409 | `쿠폰 재고가 모두 소진되었습니다.` | 현재까지 오픈된 재고 모두 소진 |
| 409 | `이미 해당 쿠폰을 발급받았습니다.` | 동일 사용자 중복 요청 |
 | 503 | `현재는 쿠폰 코드가 열려 있지 않습니다.` | 다음 주차 오픈 전 |
| 500 | `쿠폰 데이터베이스에 연결할 수 없습니다.` | MySQL 장애 |

## RedisInsight 로컬 확인
1. RedisInsight 컨테이너가 기동되지 않았다면 `docker compose up -d redis redisinsight`로 올립니다.
2. 브라우저에서 `http://localhost:5540` 접속 후 "Add Redis Database"를 선택합니다.
3. Connection Type은 `Standalone`, `Host`는 `redis`, `Port`는 `6379`로 입력하고 저장합니다. (필요 시 이름은 자유롭게 지정)
4. 이후 RedisInsight 좌측 탐색기에서 `coupon:*` 키를 열어 재고 수량, 중복 제어용 세트 등을 실시간으로 확인할 수 있습니다.
5. RedisInsight 상태를 초기화하려면 `redisinsight-data` 볼륨을 삭제한 뒤 다시 기동합니다.

## k6 부하 테스트
1. 부하 테스트 스크립트는 `tests/load/coupon-issue.js`에 있습니다.
2. Docker Compose의 `load-test` 프로파일을 사용해 k6 컨테이너를 실행합니다.
   ```bash
   docker compose --profile load-test run --rm k6
   ```
3. 기본값은 `BASE_URL=http://host.docker.internal:5000`, `COUPON_ID=WELCOME10`입니다. 필요하면 실행 시 환경 변수를 덮어씌웁니다.
   ```bash
   BASE_URL=http://host.docker.internal:5080 COUPON_ID=EVENT50 \
     docker compose --profile load-test run --rm k6
   ```
4. 시나리오 강도를 조절하려면 `START_RATE`, `RAMP_TARGET`, `PEAK_TARGET` 등 추가 환경 변수를 지정합니다. 예: `START_RATE=20 PEAK_TARGET=200`.
5. 테스트 중 RedisInsight, 애플리케이션 로그, MySQL 통계를 함께 모니터링하여 재고 소진/중복 제어가 의도대로 동작하는지 확인합니다.

## 운영 팁
- Redis에서 재고/중복을 처리 후 MySQL에 확정 기록 → DB 쓰기 실패 시 Redis 재고를 자동 복구합니다.
- 이벤트가 종료되면 `docker compose down --volumes --remove-orphans`로 컨테이너/볼륨을 정리하고, 필요 시 Redis 키(`coupon:*`), MySQL 데이터 정리로 마무리할 수 있습니다.
- 주차별 재고를 조정하려면 `appsettings`에서 `WeeklyAllocation`을 수정한 뒤, DB에서 기존 레코드를 정리하거나 새로 시드하도록 구현부를 보완하면 됩니다.
