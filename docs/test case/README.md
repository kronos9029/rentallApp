# Playwright Automation Test Cases

Thu muc nay chua bo test case automation bang Playwright cho cac flow da duoc implement trong `RentalApp.Web` den het Sprint 2.

## Scope mac dinh

- Smoke suite:
  - auth critical path
  - admin access happy path
  - private hold -> checkout
- Regression suite:
  - bao gom smoke + negative/secondary flow
  - duplicate register, invalid login, open redirect guard
  - navbar authorization va profile validation
  - public/shared availability
  - redirect login khi tao hold luc chua auth
  - hold validation, fake route access, checkout reload
  - shared boundary `slotQuantity = 1` va `8`
  - API contract va idempotency-header guardrails

## Khong dua vao smoke mac dinh

- Forgot password va reset password full flow

Ly do: flow nay dang phu thuoc SMTP that, repo chua co mailbox dev hoac email catcher on-disk/on-screen de assert mot cach on dinh trong automation suite.

## Prerequisites

- Node.js co `npm`
- .NET SDK theo `global.json`
- MySQL local da co schema `rental_app`
- Migration da duoc apply cho web app

## Cach chay

Tai folder nay:

```powershell
cmd /c npm.cmd install
cmd /c npm.cmd run install:browsers
cmd /c npm.cmd run test:smoke
```

Lenh tren mac dinh:

- dung `BASE_URL=https://localhost:7048`
- tu auto start web app bang `dotnet run` neu chua co server local
- dung `ASPNETCORE_ENVIRONMENT=Testing` de align voi config test hien co
- khong dung launch profile, URL duoc ep bang `ASPNETCORE_URLS` de tranh bi override ve `Development`

Neu muon tro vao mot app dang chay san:

```powershell
$env:BASE_URL="https://localhost:7048"
$env:PLAYWRIGHT_START_WEB_SERVER="false"
cmd /c npm.cmd test
```

## Cach chia suite

- `tests/smoke`: fast gate cho critical path
- `tests/regression`: smoke + flow bo sung va negative path
- `tests/page-objects`: page object model de giam duplicate selector va de maintain

So luong hien tai:

- Smoke: `4` test
- Regression: `50` test, trong do mot so test la `env-gated` cho Docker/MySQL/SMTP/worker

Tham chieu coverage task-level:

- [SCOPE_COVERAGE.md](D:/rentalApp/docs/test case/SCOPE_COVERAGE.md)

Lenh goi nhanh:

```powershell
cmd /c npm.cmd run test:smoke
cmd /c npm.cmd run test:regression
```

## Tai khoan mac dinh

- Customer: `customer@local.test / Customer123!`
- Admin: `admin@local.test / Admin123!`

## Cau truc

- `TEST_CASE_CATALOG.md`: danh muc test case va mapping business
- `tests/page-objects/`: page object model
- `tests/smoke/`: smoke suite
- `tests/regression/`: regression suite
- `tests/helpers/test-data.ts`: du lieu test va generator
