# Sprint 02 Task Breakdown

## Sprint Goal
Hoan thanh availability + hold + checkout voi pricing dung, concurrency baseline tin cay, va email reset password co thoi han.

## Scope
- `PB-05`: Availability API + mobile UI
- `PB-06`: Create hold with transactional locking
- `PB-07`: Multi-slot hold + hold expiry worker
- `PB-08`: Checkout flow + idempotency key support
- `PB-08C`: SMTP/email dispatch for password reset with expiring reset link
- `PB-08B`: Deadlock-safe retry baseline for booking/hold flow

## Part 1
Availability, Hold, Concurrency Baseline

| Task ID | PB ID | Task | Output mong doi | Dependency | Status |
|---|---|---|---|---|---|
| S2-T01 | PB-05 | Chot request/response contract cho availability query (`date`, `bucket`, `booking_mode`, `slot_qty`) | Contract ro rang cho UI va API, map duoc toi `court_buckets` va pricing summary | None | DONE |
| S2-T02 | PB-06 | Tao migration cho `holds`, `hold_items`, `idempotency_keys` | Schema Sprint 2 apply duoc tren local DB | S2-T01 | DONE |
| S2-T03 | PB-05 | Implement availability read query tren `courts`, `time_buckets`, `court_buckets` | API/read service tra ve courts/slots kha dung theo ngay-gio-mode | S2-T02 | DONE |
| S2-T04 | PB-05 | Tich hop pricing projection vao availability result | Availability/summary hien dung gia theo private/shared va weekday/weekend | S2-T03 | DONE |
| S2-T05 | PB-05 | Them short TTL cache cho availability query | Query availability co cache ngan, invalidate/expire an toan | S2-T03 | DONE |
| S2-T06 | PB-05 | Implement mobile-first Availability page (`/booking/availability`) | Screen availability chay duoc tren mobile, submit duoc len hold flow | S2-T01,S2-T03,S2-T04 | DONE |
| S2-T07 | PB-06 | Implement hold command service voi lock deterministic `court_buckets` va validate invariants | Tao hold an toan, khong double booking, shared khong vuot qua 8 slots | S2-T02,S2-T03 | DONE |
| S2-T08 | PB-06 | Persist `Idempotency-Key` cho create-hold endpoint | Retry submit hold khong tao hold trung | S2-T02,S2-T07 | DONE |
| S2-T09 | PB-08B | Them deadlock-safe retry voi jitter cho transaction layer cua hold flow | Deadlock/transient conflict duoc retry an toan va co gioi han | S2-T07 | DONE |
| S2-T10 | PB-07 | Ho tro multi-slot hold va luu `hold_items` day du | Hold co the chua nhieu slot/court buckets dung business rules | S2-T07 | DONE |
| S2-T11 | PB-06 | Build Hold Summary page (`/booking/hold/{holdId}`) | User xem duoc `expires_at`, item summary, tong tien tam tinh, va conflict/error state ro rang | S2-T07,S2-T10 | DONE |

## Part 2
Expiry Worker, Checkout, Identity Delivery, Test Baseline

| Task ID | PB ID | Task | Output mong doi | Dependency | Status |
|---|---|---|---|---|---|
| S2-T12 | PB-07 | Implement hold expiry background job | Worker quet hold expired theo schedule va mark expired dung | S2-T02,S2-T10 | DONE |
| S2-T13 | PB-07 | Implement inventory release khi hold het han | Slot inventory duoc tra lai dung va an toan sau expiry | S2-T12 | DONE |
| S2-T14 | PB-08 | Implement checkout command/query layer tu hold summary | Checkout dung duoc hold hop le, tinh tong tien dung, va chan hold het han | S2-T10,S2-T11 | DONE |
| S2-T15 | PB-08 | Persist `Idempotency-Key` cho checkout endpoint | Retry checkout khong tao duplicate state/response | S2-T02,S2-T14 | DONE |
| S2-T16 | PB-08 | Build Checkout page (`/checkout`) | User di tu hold summary sang checkout, thay tong tien va trang thai ro rang | S2-T11,S2-T14 | DONE |
| S2-T17 | PB-08C | Chot SMTP config contract va secret keys cho local/prod-like environment | App co config ro rang cho host/port/account/from-address, secrets doc tu encrypted config/env | None | DONE |
| S2-T18 | PB-08C | Implement email sender infrastructure va forgot-password dispatch | Forgot password gui duoc reset link qua email thay cho dev-only on-screen link | S2-T17 | DONE |
| S2-T19 | PB-08C | Refactor reset password flow de dung expiring reset link qua email | Reset link co thoi han, generic success message, khong lo email existence | S2-T18 | DONE |
| S2-T20 | PB-08C | Them email template va local delivery/testing strategy | Co template email reset password va cach verify local/dev an toan | S2-T18 | DONE |
| S2-T21 | PB-05,PB-06,PB-07,PB-08,PB-08B,PB-08C | Viet unit/integration/concurrency test baseline cho availability, hold, expiry, checkout, email reset | Test baseline chay duoc trong CI va cover acceptance Sprint 2 | S2-T05,S2-T09,S2-T13,S2-T15,S2-T19 | DONE |
| S2-T22 | PB-05,PB-06,PB-07,PB-08,PB-08C | Tao smoke checklist cho mobile-first availability -> hold -> checkout va forgot/reset password qua email | QA/manual smoke co checklist ro rang de verify Sprint 2 | S2-T16,S2-T19,S2-T21 | DONE |

## Suggested Sequence
1. Hoan thanh schema Sprint 2, availability read/query, pricing projection, va mobile availability UI (`S2-T01..S2-T06`).
2. Hoan thanh hold transaction, idempotency, retry baseline, multi-slot hold, va hold summary (`S2-T07..S2-T11`).
3. Hoan thanh hold expiry worker va inventory release (`S2-T12..S2-T13`).
4. Hoan thanh checkout command/query, idempotency, va checkout UI (`S2-T14..S2-T16`).
5. Hoan thanh SMTP config, email sender, expiring reset link, va bo email template (`S2-T17..S2-T20`).
6. Chot test baseline va smoke checklist (`S2-T21..S2-T22`).

## Current Progress
- Part 1 da xong: schema `holds/hold_items/idempotency_keys`, availability service/API/UI, hold service, deadlock retry baseline, va hold summary page.
- Sprint 1 da co pricing baseline, DB-backed auth, native MySQL local, va HTTPS local smoke test pass.
- Sprint 2 se tai su dung pricing baseline, encrypted config bootstrap, worker host, va security baseline tu Sprint 1.
- Build/test da pass va migration `Sprint02Part1AvailabilityHold` + `Sprint02Part2ExpiryCheckout` da apply tren native MySQL local.
- SMTP contract + forgot-password email dispatch da duoc implement. Local secret hien dang nam trong `appsettings.Development.json` duoi dang encrypted scalar `enc::...`; app decrypt qua `RENTALAPP__AES_KEY`, va `user-secrets` cua web app da duoc don sach.
- Local smoke cho forgot-password da gui mail thanh cong qua Gmail SMTP voi app password, va backend log xac nhan dispatch success.
- Hold expiry worker da duoc noi vao `RentalApp.Worker`, release inventory khi het han, va `HoldService` / `CheckoutService` deu chan hold het han truoc khi thao tac tiep.
- Checkout Part 2 da duoc implement voi `bookings`, `booking_items`, `checkout_orders`, `checkout_order_items`, idempotency cho `/api/checkout`, va page `/Checkout`.
- Smoke runtime da pass cho flow `login -> availability -> hold -> checkout` tren HTTPS local + native MySQL. Worker log cung xac nhan tick expiry tren DB that.

## Done Checklist
- Availability query va UI hoat dong dung tren mobile-first flow.
- Hold creation dung transactional locking va khong double booking duoi concurrent requests.
- Shared booking khong vuot qua 8 slots.
- Hold co `expires_at` va inventory duoc auto release khi expired.
- Retry hold/checkout khong tao duplicate va deadlock transient duoc retry an toan.
- Hold summary va checkout total khop pricing rules cho private/shared va weekday/weekend.
- Forgot password gui email reset link co thoi han va reset link het han bi tu choi dung.
- UI/API contract va smoke checklist duoc cap nhat cho flow availability -> hold -> checkout.
