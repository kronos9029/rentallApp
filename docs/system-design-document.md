# System Design Document (SDD)

## 1. Document Purpose and Scope

Tai lieu nay mo ta thiet ke he thong cho Pickleball Court Booking Web App, tong hop tu:
- `docs/project requirement.md`
- `docs/proposal.md`
- `docs/user-stories-functional.md`

Pham vi:
- 2 vai tro chinh: `Customer`, `Admin`.
- Nghiep vu cot loi: dat san private/shared, thanh toan VNPay va bank QR, check-in bang code, huy/hoan tien, loyalty, dashboard quan tri.
- He thong duoc xay dung theo ASP.NET Core MVC/Razor + API, Aurora MySQL-Compatible, worker bat dong bo.

## 2. Business Goals and Constraints

### 2.1 Business goals
- Ho tro dat 9 san voi 2 kieu dat: private theo gio va shared toi da 8 slot/khung gio.
- Dam bao khong overbooking trong dieu kien tranh chap cao.
- Ho tro thanh toan online (VNPay) va chuyen khoan QR dong.
- Sinh ma check-in chi sau khi thanh toan thanh cong.
- Ho tro chinh sach huy/hoan tien theo moc 2 gio.
- Ho tro loyalty va dashboard bao cao cho admin.
- Uu tien trai nghiem nguoi dung tren mobile vi day la nhom thiet bi su dung chinh.

### 2.2 Technical constraints
- Stack bat buoc: ASP.NET Core (.NET 10 LTS), Razor Pages/MVC.
- UI stack: Bootstrap/Tailwind cho frontend layer (Razor views/pages).
- UI phai mobile-first, responsive tu man hinh nho len man hinh lon (progressive enhancement).
- Target hieu nang mobile: toi uu payload, lazy load tai nguyen khong critical, giam blocking scripts/styles.
- DB: Aurora Serverless v2 (MySQL-compatible, InnoDB).
- Cache: in-memory cache per instance (`IMemoryCache`).
- Bao mat uu tien in-app (khong dung WAF), theo coding standards da chot.
- Secret handling: encrypted config + AES key tu environment variable runtime.

## 3. Actors and Context

### 3.1 Actors
- `Customer`: dang ky/dang nhap, dat san, thanh toan, xem lich su, huy booking.
- `Admin`: quan ly san, gia, booking, hoan tien, check-in, bao cao.
- `Payment Provider`: VNPay IPN, (tuy chon) bank QR callback.

### 3.2 External systems
- VNPay payment gateway.
- Banking QR rail (VietQR/tuong duong).
- AWS infrastructure services (ALB, ECS/Fargate, Aurora, S3).

## 4. Architectural Style and High-Level Design

### 4.1 Overall style
- Modular monolith theo domain modules trong mot web app runtime.
- Tach worker cho workload bat dong bo va scheduled jobs.
- Kien truc event-driven noi bo de giam coupling giua transactional flow va side effects.

### 4.2 Runtime components
- `Web App (MVC + Razor + API Controllers)`:
  - Xu ly request UI/API.
  - Thuc thi domain use-cases.
  - Tich hop VNPay/BankQR.
- `Background Worker`:
  - Expire hold.
  - Reconciliation thanh toan.
  - Award loyalty, refund processing.
  - End-of-day check-in code expiry.
- `Aurora MySQL-Compatible`:
  - Nguon du lieu giao dich chinh.
  - Khoa dong + transaction consistency.
- `In-memory cache`:
  - Cache availability short TTL, non-critical reads.

### 4.3 Logical module boundaries
- `Identity & Access`.
- `Court Availability & Inventory`.
- `Booking & Hold`.
- `Payment Integration`.
- `Check-in`.
- `Cancellation & Refund`.
- `Pricing`.
- `Loyalty`.
- `Reporting & Export`.
- `Audit & Security`.

### 4.4 UI strategy (Bootstrap/Tailwind)
- Muc tieu: tang toc do build UI va giu consistency cho web app.
- Quy uoc su dung:
  - `Bootstrap`: uu tien cho form-heavy pages va admin dashboard (table, modal, form validation states).
  - `Tailwind`: uu tien cho customer-facing pages can layout linh hoat va utility-first styling.
- Nguyen tac ky thuat:
  - Khong mix Bootstrap utility classes va Tailwind utility classes trong cung mot component/page.
  - Moi page chon 1 style system chinh de tranh conflict CSS va kho maintain.
  - Dung design tokens chung (colors, spacing, typography) de dam bao brand consistency giua 2 he thong.
  - UI components dung lai (button, input, badge, alert, card, modal wrapper) duoc dong goi thanh partial/component rieng.
  - Thiet ke mobile-first: default layout cho viewport nho, sau do mo rong qua breakpoints.
  - Tap trung touch UX: target tap size >= 44px, khoang cach thao tac ro rang, sticky CTA cho checkout/payment tren mobile.
  - Form booking/payment toi uu cho mobile: input ngan gon, keyboard-friendly type, han che scroll ngang.

### 4.5 Mobile-first UX priorities
- Luong uu tien mobile:
  - Browse availability.
  - Create hold.
  - Checkout + VNPay/Bank QR payment.
  - View booking status va check-in code.
- Nguyen tac UX mobile:
  - Toi da 1 cot chinh cho customer pages.
  - Navigation don gian, uu tien bottom/sticky actions cho luong dat san.
  - Giam so buoc checkout va giam so field bat buoc nhap tay.
  - Tai noi dung theo nhu cau (defer non-critical widgets/sections).

### 4.6 Mobile screen acceptance checklist

#### 4.6.1 Global checklist (ap dung cho tat ca man hinh mobile)
- [ ] Ho tro viewport tu `320px` tro len, khong bi vo layout.
- [ ] Khong co horizontal scroll o trang thai binh thuong.
- [ ] Tap target quan trong (button, tab, row action) >= `44x44px`.
- [ ] Text chinh doc duoc tren man hinh nho (khong can zoom tay).
- [ ] Thoi gian hien thi noi dung chinh dau tien o mang mobile o muc chap nhan duoc.
- [ ] Form input dung dung keyboard type (email, number) de giam loi nhap lieu.
- [ ] Error state hien ro rang, khong che mat CTA chinh.
- [ ] Secure headers/cookie/CSRF va validation van hoat dong day du tren mobile browser.

#### 4.6.2 Availability screen (CUS-06, CUS-09)
- [ ] Cho phep chon ngay/khung gio bang control than thien mobile.
- [ ] Hien thi ro private/shared va so slot con lai (vi du `5/8`).
- [ ] Loading state/skeleton hien dung khi dang tai availability.
- [ ] Neu het cho, UI the hien disabled state ro rang.
- [ ] CTA dat san luon de thao tac (sticky action neu can).

#### 4.6.3 Hold creation screen (CUS-07, CUS-08, CUS-10, SYS-01, SYS-02)
- [ ] Customer co the chon private hoac shared ma khong nham lan.
- [ ] Shared quantity khong vuot qua suc chua con lai.
- [ ] Khi submit, tao hold thanh cong thi hien `expires_at` ro rang.
- [ ] Neu hold that bai do conflict, hien thong bao de hieu va cho chon lai nhanh.
- [ ] Retry submit khong tao hold trung (idempotency hoat dong).

#### 4.6.4 Checkout screen (CUS-10, CUS-15, CUS-20)
- [ ] Tong tien, chi tiet slot, surcharge hien thi ro rang trong 1 cot.
- [ ] CTA thanh toan de thao tac bang 1 tay (thumb-friendly).
- [ ] Input lien quan thanh toan toi gian, khong co field thua.
- [ ] Submit checkout nhieu lan khong tao booking duplicate.
- [ ] Neu session/hold het han, redirect va thong bao ro rang.

#### 4.6.5 Payment method + payment status screen (CUS-12, CUS-13, CUS-14, PAY-01, PAY-02)
- [ ] Chon VNPay hoac Bank QR ro rang, khong mo ho.
- [ ] Luong redirect VNPay quay lai app an toan va cap nhat trang thai dung.
- [ ] QR dong hien du amount + reference de doi soat.
- [ ] Trang thai `Pending/Paid/Failed` duoc cap nhat dung voi callback/IPN.
- [ ] Webhook/callback duplicate khong gay duplicate transition.

#### 4.6.6 Booking history screen (CUS-05, CUS-14)
- [ ] Danh sach booking hien thi gon tren mobile (date, court, status, amount).
- [ ] Co bo loc co ban (status/date) ma khong lam roi flow.
- [ ] Scroll dai van muot, phan trang/load-more hoat dong dung.
- [ ] Chon 1 booking mo duoc trang chi tiet.

#### 4.6.7 Booking detail + check-in code screen (CUS-11, SYS-04, SYS-05)
- [ ] Check-in code chi hien thi khi payment da `Paid`.
- [ ] Trang thai code (`ACTIVE/USED/EXPIRED`) hien thi ro va nhat quan.
- [ ] Code de copy/doi chieu nhanh tren mobile.
- [ ] Neu code het han/da dung, UI thong bao ro ly do.
- [ ] End-of-day expiry duoc phan anh dung tren UI sau khi dong bo du lieu.

#### 4.6.8 Profile/account screen (CUS-01..CUS-04)
- [ ] Register/Login/Forgot password forms de nhap va validation ro rang.
- [ ] Loi validation field hien tai cho, khong chi hien toast chung.
- [ ] Password rules va error message de hieu tren mobile.
- [ ] Update profile thanh cong co feedback ngay (toast/inline).

#### 4.6.9 Admin check-in screen (ADM-07, ADM-08)
- [ ] Admin submit code nhanh tren mobile tai quay check-in.
- [ ] Ket qua `valid/invalid/used/expired` tra ve duoi 1 trang thai de hieu.
- [ ] Sau khi redeem thanh cong, code khong dung lai duoc.
- [ ] Action check-in duoc ghi audit day du (`admin_user_id`, timestamp, booking ref).

## 5. Programming Paradigms Applied

### 5.1 MVC (Model-View-Controller)
- `View`: Razor Pages/MVC views cho booking, checkout, profile, admin dashboard.
- `Controller/PageModel`: tiep nhan request, validate input, goi application service.
- `Model`: domain entities/value objects/DTO.
- Rule: Controller khong chua business logic phuc tap; logic nam o service/use-case.

### 5.2 Convention over Configuration
- Theo conventions cua ASP.NET Core:
  - Folder structure theo feature/domain.
  - Routing conventions cho Razor Pages va API controllers.
  - Naming conventions cho DTO/command/query/handler.
- Chi override config khi co ly do nghiep vu hoac non-functional requirement ro rang.

### 5.3 Object-Oriented Programming (OOP)
- Domain model hoa thanh entities, value objects, aggregates.
- Encapsulation:
  - Invariants booking/shared capacity nam trong domain service/aggregate methods.
- Abstraction qua interfaces:
  - `IPaymentGateway`, `IBookingService`, `IRefundService`, `ICheckInService`.

### 5.4 Functional Programming style trong .NET
- Uu tien pure function cho:
  - Pricing calculation.
  - Cancellation eligibility.
  - Loyalty points calculation.
  - Signature validation utility.
- Immutability cho request/response DTO va event payload.
- Han che side effects trong ham tinh toan; side effects tap trung tai orchestration layer.

### 5.5 Event-Driven / Reactive concepts
- Domain events noi bo (publish sau commit):
  - `HoldCreated`, `PaymentConfirmed`, `BookingConfirmed`, `BookingCancelled`, `CheckInRedeemed`.
- Event handlers:
  - Tao check-in code sau `PaymentConfirmed`.
  - Cong diem loyalty sau `BookingCompleted`.
  - Trigger report counters/audit projections.
- Outbox pattern (khuyen nghi) de dam bao khong mat event khi co loi tai integration.

## 6. Design Principles Applied

### 6.1 Separation of Concerns (SoC)
- Presentation (Razor/API), Application (use-cases), Domain (rules), Infrastructure (DB/external providers) tach ro.
- Security middleware tach khoi business flow.

### 6.2 SOLID
- `S`: moi service phu trach 1 concern (booking, payment, pricing, check-in).
- `O`: them provider payment moi qua abstraction, khong sua core flow.
- `L`: implementation payment provider thay the duoc qua interface.
- `I`: interface nho, theo capability (`ISignatureVerifier`, `IRefundExecutor`).
- `D`: dependency theo abstraction; DI container resolve concretions.

### 6.3 High Cohesion, Low Coupling
- Module inventory giu tron logic inventory.
- Payment module khong can biet chi tiet persistence cua booking.
- Event-based integration giam coupling truc tiep.

### 6.4 Composition over Inheritance
- Build service bang composition:
  - validation + policy + repository + gateway.
- Tranh deep inheritance hierarchy trong domain/application services.

### 6.5 DRY
- Reuse chung:
  - idempotency filter/middleware.
  - secure header middleware.
  - webhook signature verification utility.
  - common result/error model.

### 6.6 KISS
- Giu modular monolith thay vi microservices o quy mo hien tai.
- Chon in-memory cache cho read-optimization don gian.
- Chi duy tri 1 luong booking/hold/payment core, tranh branching phuc tap.

### 6.7 YAGNI
- Chua tach thanh microservices khi chua co nhu cau scale/doc lap deploy.
- Chua mo rong workflow engine phuc tap cho refund neu rule hien tai du.
- Chua them distributed cache khi read pressure chua dat nguong.

## 7. Functional Design by Module

### 7.1 Identity & Account
- Register/login/forgot password/reset password.
- Profile management.
- Role-based authorization (`Customer`, `Admin`).

### 7.2 Court Availability & Booking
- Availability read theo date/time.
- Hold inventory voi TTL.
- Private booking va shared booking.
- Rule bat buoc:
  - no double booking same court same time.
  - shared capacity <= 8.

### 7.3 Payment
- VNPay redirect + IPN signature verification.
- Bank QR dynamic amount + reference.
- Payment status machine: Pending/Paid/Failed.
- Idempotent provider callback processing.

### 7.4 Check-in
- Generate random check-in code sau payment success.
- Admin redeem code atomically (FOR UPDATE).
- Mark code used/expired.
- End-of-day expiry job.

### 7.5 Cancellation & Refund
- Eligibility rule: huy truoc >=2h moi du dieu kien hoan.
- Refund strategy configurable (100%, 95%, wallet/internal) theo policy.
- Manual admin approval path cho truong hop dac biet.

### 7.6 Pricing
- Time-frame pricing.
- Peak/off-peak.
- Weekend surcharge.
- Private/shared distinct pricing.

### 7.7 Loyalty
- Conversion rule: 10,000 VND = 1 point.
- Award points only when booking completed.
- Tier model: Silver/Gold/Platinum.

### 7.8 Reporting
- Dashboard bookings/revenue.
- Export Excel.
- Audit log-based traceability.

## 8. Data and Transaction Design

### 8.1 Core entities
- `courts`, `time_buckets`, `court_buckets`
- `holds`, `hold_items`
- `bookings`, `booking_items`
- `payments`, `payment_events`, `refunds`
- `checkin_codes`
- `pricing_rules`, `loyalty_ledger`, `audit_log`

### 8.2 Transaction boundaries
- `CreateHold`: lock `court_buckets` rows with deterministic order + update invariants + insert hold items.
- `ConfirmPayment`: idempotent transition + create booking items + generate check-in codes.
- `RedeemCheckIn`: lock code row + validate + mark used + audit.

### 8.3 Consistency strategy
- Strong consistency cho inventory/payment transition qua ACID transaction.
- Eventual consistency cho non-critical side effects qua background events/jobs.

## 9. API and Interaction Design

### 9.1 Representative APIs
- Availability: `GET /api/availability`
- Holds: `POST /api/holds`, `DELETE /api/holds/{id}`
- Checkout: `POST /api/checkout`
- VNPay: `POST /api/payments/vnpay/initiate`, `POST /payments/vnpay/ipn`
- Bank QR: `POST /api/payments/bankqr/create`, `POST /payments/bankqr/callback`
- Check-in: `POST /api/checkin/validate` (Admin)
- Admin config: `PUT /admin/courts/{id}`, `PUT /admin/pricing/rules/{id}`

### 9.2 Idempotency contract
- Bat buoc `Idempotency-Key` cho create/transition endpoints.
- Persist tuple:
  - `user_id`, `endpoint`, `idempotency_key`, `request_hash`, `response_snapshot`.
- Duplicate request tra lai stored response.

## 10. Security Architecture (In-App First)

### 10.1 Mandatory secure coding controls
1. Parameterized queries only.
2. Razor default HTML encoding (review strict voi `Html.Raw`).
3. Input model validation (DataAnnotations + server-side checks).
4. CSRF protection (`AutoValidateAntiforgeryToken`).
5. Secure cookie config (`Secure`, `HttpOnly`, `SameSite`, TLS only).
6. Rate limiting (global + endpoint policies).
7. Secure headers (CSP, nosniff, frame controls, referrer policy, cache-control).
8. Validate payment webhook signature before state transition.
9. Idempotency for booking and payment.

### 10.2 Secrets and config
- Khong ghi key vao logs hoac plaintext config.
- Sensitive config duoc ma hoa (AES).
- AES key inject qua environment variable chi tai runtime (ECS task/container env).

## 11. Deployment and Operations

### 11.1 Deployment topology
- ALB -> ECS/Fargate Web App tasks.
- ECS/Fargate Worker tasks.
- Aurora Serverless v2.
- S3 for export/artifacts.

### 11.2 Runtime operations
- Health endpoints: `/healthz`, `/readyz`.
- Structured logs + retention policy.
- Theo doi mobile web metrics (LCP, INP, CLS) trong qua trinh van hanh.
- Daily jobs:
  - expire holds.
  - expire old check-in codes.

### 11.3 Reliability tactics
- Retry voi jitter cho deadlock/transient DB errors.
- Payment callback dedupe.
- Graceful degradation neu bank callback delayed (polling/manual fallback).

## 12. Traceability to User Stories

- Customer stories: `CUS-01..CUS-20` (tham chieu `user-stories-functional.md`).
- Admin stories: `ADM-01..ADM-13`.
- Provider/public stories: `PUB-01`, `PAY-01..PAY-02`.
- System automation: `SYS-01..SYS-09`.

Muc tieu la moi module trong SDD nay phai map duoc toi it nhat 1 user story va 1 business rule trong requirement.

## 13. Suggested Project Structure (Coding by Convention)

```text
src/
  RentalApp.Web/
    Areas/
      Admin/
        Pages/
        ViewModels/
      Customer/
        Pages/
        ViewModels/
    Controllers/
      Api/
    Views/
      Shared/
    ViewComponents/
    Middleware/
    Filters/
    UI/
      Bootstrap/
        Admin/
      Tailwind/
        Customer/
      Tokens/
      Components/
    wwwroot/
      css/
      js/
      images/
    Program.cs
    appsettings.json

  RentalApp.Application/
    Common/
      Abstractions/
      Behaviors/
      Exceptions/
      Models/
    Features/
      Availability/
      Booking/
      Payment/
      CheckIn/
      Pricing/
      Loyalty/
      Refund/

  RentalApp.Domain/
    Common/
    Entities/
    ValueObjects/
    Enums/
    Events/
    Policies/
    Services/

  RentalApp.Infrastructure/
    Persistence/
      Configurations/
      Repositories/
      Migrations/
    Integrations/
      VNPay/
      BankQr/
    Security/
      Cryptography/
      Secrets/
    Caching/
    Idempotency/
    BackgroundProcessing/

  RentalApp.Worker/
    Jobs/
      Holds/
      Payments/
      Loyalty/
      CheckIn/
    EventHandlers/
    Program.cs

tests/
  RentalApp.UnitTests/
  RentalApp.IntegrationTests/
  RentalApp.ArchitectureTests/

build/
  docker/
    web.Dockerfile
    worker.Dockerfile
  scripts/

docs/
  adr/
  api/
```

## 14. Out of Scope (Current Phase)

- Microservices decomposition.
- Distributed cache.
- Advanced CQRS/read replica segregation.
- Real-time push notification phuc tap.

Tai lieu nay la baseline de team implementation, test plan, va architecture review co cung ngon ngu thiet ke.
