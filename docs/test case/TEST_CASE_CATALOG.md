# Test Case Catalog

Catalog nay liet ke cac suite Playwright hien co. Mapping task-level day du hon nam o [SCOPE_COVERAGE.md](D:/rentalApp/docs/test case/SCOPE_COVERAGE.md).

## Smoke Suite

| Suite | Case IDs |
|---|---|
| `tests/smoke/auth-smoke.spec.ts` | `TC-AUTH-01`, `TC-AUTH-02`, `TC-AUTH-03` |
| `tests/smoke/booking-smoke.spec.ts` | `TC-BOOK-01` |

Tong: `4` test

## Regression Suite

| Suite | Case IDs |
|---|---|
| `tests/regression/api-regression.spec.ts` | `TC-API-01`, `TC-API-02`, `TC-API-03`, `TC-API-04` |
| `tests/regression/auth-regression.spec.ts` | `TC-AUTH-04`, `TC-AUTH-05`, `TC-AUTH-06`, `TC-AUTH-07`, `TC-AUTH-08`, `TC-AUTH-09`, `TC-PRO-01`, `TC-PRO-02`, `TC-PRO-03` |
| `tests/regression/booking-api-regression.spec.ts` | `TC-BOOK-API-01`, `TC-BOOK-API-02`, `TC-BOOK-API-03`, `TC-BOOK-API-04` |
| `tests/regression/booking-regression.spec.ts` | `TC-BOOK-02`, `TC-BOOK-03`, `TC-BOOK-04`, `TC-BOOK-05`, `TC-BOOK-06`, `TC-BOOK-07`, `TC-BOOK-08`, `TC-BOOK-09`, `TC-BOOK-10`, `TC-BOOK-11` |
| `tests/regression/email-worker-regression.spec.ts` | `TC-EMAIL-01`, `TC-EMAIL-02`, `TC-EMAIL-03`, `TC-WORKER-01` |
| `tests/regression/foundation-regression.spec.ts` | `TC-FOUND-01`, `TC-FOUND-02`, `TC-FOUND-03`, `TC-FOUND-04`, `TC-FOUND-05`, `TC-FOUND-06` |
| `tests/regression/platform-contract-regression.spec.ts` | `TC-BOOT-01`, `TC-BOOT-02`, `TC-MIG-01`, `TC-RETRY-01`, `TC-RETRY-02`, `TC-SMTP-01`, `TC-SMTP-02`, `TC-SMTP-03`, `TC-SMTP-04` |
| `tests/regression/security-regression.spec.ts` | `TC-SEC-01`, `TC-SEC-02`, `TC-SEC-03`, `TC-MOB-01` |

Tong: `50` test

## Ghi chu QA

- `Env-gated` test can bat bang cac bien moi truong:
  - `ENABLE_INFRA_COMMAND_TESTS=true`
  - `ENABLE_DOTNET_COMMAND_TESTS=true`
  - `ENABLE_MAILHOG_TESTS=true`
  - `ENABLE_WORKER_RUNTIME_TESTS=true`
- Test email/worker can MySQL/SMTP runtime that.
- `TC-BOOK-03` trong booking regression duoc `skip` co chu thich, vi seeded environment hien tai auto-provision availability moi ngay nen empty-state khong reach duoc neu khong thao tac them vao env.
