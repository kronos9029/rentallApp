# Product Backlog

Nguon can cu:
- docs/project requirement.md
- docs/proposal.md
- docs/system-design-document.md
- docs/user-stories-functional.md
- docs/database-design-document.md

## 1. Product Goal
Xay dung Pickleball Court Booking Web App cho Customer va Admin, ho tro:
- booking private/shared (max 8 shared slots)
- payment VNPay + Bank QR
- check-in code sau payment thanh cong
- cancellation/refund policy
- loyalty + admin dashboard

## 2. Planning Assumptions
- Sprint length: 2 weeks
- Team: 1 PM, 1 BA, 3 Dev, 1 QA
- Capacity tham chieu: 35-45 story points/sprint
- Priority levels: P0 (critical), P1 (high), P2 (medium)

## 3. Definition of Ready
- Story co mo ta ro rang, actor, business value
- Co acceptance criteria testable
- Co dependencies va impact data model ro rang
- UI/API contract duoc xac dinh

## 4. Definition of Done
- Code + unit/integration tests pass
- Security controls bat buoc da ap dung (input validation, CSRF, secure headers, idempotency)
- API docs / release note cap nhat
- QA verified tren mobile-first flows

## 5. Prioritized Backlog

| PB ID | Epic | Story IDs | Backlog Item | Priority | SP | Target Sprint |
|---|---|---|---|---|---:|---|
| PB-01 | Foundation | SYS-09 | Project bootstrap (.NET MVC + API + DB migration + CI) | P0 | 8 | S1 |
| PB-02 | Identity | CUS-01,CUS-02,CUS-03 | Register/Login/Forgot password | P0 | 8 | S1 |
| PB-03 | Identity | CUS-04 | Profile management | P1 | 3 | S1 |
| PB-03A | Admin Access | ADM-01 | Admin access + area authorization baseline | P0 | 3 | S1 |
| PB-04 | Core Data | SYS-01,SYS-02 | Court/time bucket/inventory data model + constraints | P0 | 8 | S1 |
| PB-04A | Pricing Foundation | ADM-03,ADM-04,ADM-05 | Default pricing seed + read-side price calculation baseline | P0 | 5 | S1 |
| PB-05 | Availability | CUS-06,CUS-09,PUB-01 | Availability API + mobile UI | P0 | 8 | S2 |
| PB-06 | Hold | CUS-07,CUS-08,SYS-01,SYS-02 | Create hold with transactional locking | P0 | 13 | S2 |
| PB-07 | Hold | CUS-10,SYS-03 | Multi-slot hold + hold expiry worker | P0 | 8 | S2 |
| PB-08 | Checkout | CUS-10,CUS-20 | Checkout flow + idempotency key support | P0 | 8 | S2 |
| PB-08C | Identity Delivery | CUS-03 | SMTP/email dispatch for password reset with expiring reset link | P1 | 3 | S2 |
| PB-08A | Booking Read | CUS-05 | Booking history + booking detail + re-open check-in codes | P1 | 5 | S3 |
| PB-08B | Reliability Baseline | SYS-08 | Deadlock-safe retry baseline for booking/hold flow | P0 | 5 | S2 |
| PB-09 | Payment VNPay | CUS-12,CUS-14,PAY-01 | VNPay initiate + return + IPN verify | P0 | 13 | S3 |
| PB-10 | Payment Confirm | CUS-15,SYS-06 | Payment state machine + dedupe webhook + durable event write | P0 | 8 | S3 |
| PB-11 | Booking Finalize | CUS-11,SYS-04 | Confirm booking items + generate check-in code | P0 | 8 | S3 |
| PB-12 | Check-in Admin | ADM-07,ADM-08,SYS-05 | Admin check-in validation + code redeem | P0 | 8 | S3 |
| PB-12A | Audit Baseline | ADM-13 | Baseline audit logging for check-in, pricing, refund, court actions | P0 | 3 | S3 |
| PB-13 | Bank QR | CUS-13,PAY-02 | Dynamic QR payment + callback endpoint | P1 | 8 | S4 |
| PB-14 | Cancellation | CUS-16,CUS-17 | Cancellation eligibility engine >=2h | P0 | 8 | S4 |
| PB-15 | Refund | ADM-09,ADM-10 | Manual refund approval workflow | P1 | 8 | S4 |
| PB-16 | Pricing | ADM-03,ADM-04,ADM-05 | Pricing rules admin management + overlap validation | P0 | 8 | S4 |
| PB-17 | Admin Ops | ADM-02,ADM-06 | Court management + booking management view | P1 | 8 | S4 |
| PB-18 | Loyalty | CUS-18,CUS-19,SYS-07 | Loyalty points + tiering | P1 | 8 | S5 |
| PB-19 | Reporting | ADM-11,ADM-12 | Revenue report + Excel export | P1 | 8 | S5 |
| PB-20 | Audit | ADM-13 | Audit viewer + search/filter/reporting | P1 | 5 | S5 |
| PB-21 | Security Hardening | SYS-09 | Full secure headers, cookie hardening, rate limiting tuning | P0 | 8 | S5 |
| PB-22 | Reliability Hardening | SYS-08 | Extended resilience + concurrency/webhook replay regression pack | P1 | 5 | S5 |
| PB-23 | Performance | CUS-06,CUS-14 | Mobile performance optimization + cache strategy | P1 | 8 | S6 |
| PB-24 | Data/Async | SYS-03,SYS-05,SYS-07 | Async reliability hardening + worker retry/poison handling | P1 | 8 | S6 |
| PB-25 | UAT/Release | ALL | End-to-end regression + prod readiness checklist | P0 | 13 | S6 |

## 6. Dependency Highlights
- PB-04/PB-04A truoc PB-05/06/07/08
- PB-02 truoc PB-08C
- PB-03A truoc PB-12/16/17
- PB-08/PB-08B truoc PB-09/10/11
- PB-10 truoc PB-11/12/08A
- PB-12A truoc cac admin actions yeu cau audit trong PB-15/16/17
- PB-14 truoc PB-15
- PB-20/21/22/24 can song song nhung complete truoc release

## 7. Release Milestones
- End S3: Booking + VNPay + booking detail/history + check-in ready cho closed beta
- End S4: Full payment options + cancellation/refund + admin pricing/ops
- End S6: Production-ready release
