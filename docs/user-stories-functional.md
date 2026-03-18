# Functional User Stories

Nguon tong hop:
- `docs/project requirement.md`
- `docs/proposal.md`

Ghi chu dong bo:
- Theo `proposal.md` phien ban hien tai, vai tro check-in la `Admin` (khong con `Receptionist`).

## Customer Stories

| ID | User Story | Function (He thong) | API/Man hinh lien quan |
|---|---|---|---|
| CUS-01 | As a customer, I want to register an account with email and password so that I can use booking features. | Dang ky tai khoan | Auth/Register page |
| CUS-02 | As a customer, I want to sign in so that I can book and manage my orders. | Dang nhap | Auth/Login page |
| CUS-03 | As a customer, I want to reset my password via time-limited link so that I can recover account access. | Quen mat khau + reset token het han | Auth/ForgotPassword, Auth/ResetPassword |
| CUS-04 | As a customer, I want to update my profile so that my account information stays accurate. | Quan ly ho so ca nhan | Profile page |
| CUS-05 | As a customer, I want to view my booking history and re-open check-in codes so that I can track past sessions. | Xem lich su dat san va ma check-in | Booking history page |
| CUS-06 | As a customer, I want to browse court availability by date/time so that I can choose suitable slots. | Xem ton kho court_buckets (private/shared) | `GET /api/availability` |
| CUS-07 | As a customer, I want to book a private court by hour so that I can reserve the whole court. | Dat san rieng theo khung gio | `POST /api/holds` + checkout |
| CUS-08 | As a customer, I want to book shared slots so that I can join a time frame with limited capacity. | Dat san chung toi da 8 slot/khung gio | `POST /api/holds` |
| CUS-09 | As a customer, I want to see remaining shared slots (e.g. 5/8) so that I know if I can still book. | Hien thi suc chua con lai shared | Availability UI |
| CUS-10 | As a customer, I want to reserve multiple shared slots in one checkout so that payment is faster. | Dat nhieu slot trong mot lan thanh toan | `POST /api/holds`, `POST /api/checkout` |
| CUS-11 | As a customer, I want each booked slot to have its own check-in code so that each usage can be validated independently. | Sinh code theo booking_item | Payment confirm flow |
| CUS-12 | As a customer, I want to pay by VNPay so that I can complete booking online. | Thanh toan VNPay redirect + IPN | `POST /api/payments/vnpay/initiate`, `/payments/vnpay/return`, `/payments/vnpay/ipn` |
| CUS-13 | As a customer, I want to pay by dynamic bank QR so that I can transfer quickly from banking app. | Tao QR dong theo so tien va ref | `POST /api/payments/bankqr/create` |
| CUS-14 | As a customer, I want to see payment status (Pending/Paid/Failed) so that I know booking outcome. | Theo doi trang thai thanh toan | Payment status UI |
| CUS-15 | As a customer, I want booking to be confirmed only after successful payment so that billing is correct. | Payment-gated booking confirmation | Payment/IPN processing |
| CUS-16 | As a customer, I want to cancel booking >=2h before start time to receive refund according to policy. | Chinh sach huy co dieu kien hoan tien | `POST /api/bookings/{id}/cancel` |
| CUS-17 | As a customer, I want cancellation <2h to be blocked from refund so that policy is applied consistently. | Tu choi hoan tien neu qua han | Cancel booking rule engine |
| CUS-18 | As a customer, I want loyalty points after completed bookings so that I gain membership benefits. | Cong diem sau khi booking complete | Loyalty service/worker |
| CUS-19 | As a customer, I want tiering (Silver/Gold/Platinum) so that I can receive tier discounts. | Phan hang thanh vien + uu dai | Loyalty/tier module |
| CUS-20 | As a customer, I want booking/payment requests to be idempotent so that retries do not create duplicates. | Idempotency-Key + replay response | `POST /api/holds`, `POST /api/checkout`, payment APIs |

## Admin Stories

| ID | User Story | Function (He thong) | API/Man hinh lien quan |
|---|---|---|---|
| ADM-01 | As an admin, I want to sign in with admin role so that I can access protected dashboard features. | Role-based access (Admin) | Admin auth + authorization |
| ADM-02 | As an admin, I want to enable/disable courts and set maintenance status so that operations are controlled. | Quan ly trang thai san | `PUT /admin/courts/{id}` |
| ADM-03 | As an admin, I want to configure pricing by time frame so that normal/peak-hour pricing is accurate. | Cau hinh gia theo khung gio | `PUT /admin/pricing/rules/{id}` |
| ADM-04 | As an admin, I want to configure weekend surcharge percent so that weekend pricing is automatic. | Phu thu cuoi tuan | Pricing rules |
| ADM-05 | As an admin, I want separate prices for private and shared bookings so that business policy is flexible. | Gia rieng cho private/shared | Pricing rules |
| ADM-06 | As an admin, I want to review bookings by date/month so that I can monitor operations. | Quan ly booking theo ngay/thang | Admin dashboard |
| ADM-07 | As an admin, I want to validate check-in codes manually so that players can check in on-site. | Check-in code redemption | `POST /api/checkin/validate` |
| ADM-08 | As an admin, I want used check-in codes to be deactivated immediately so that reuse is prevented. | Mark code USED + audit | Check-in transaction |
| ADM-09 | As an admin, I want to process exceptional/manual refunds so that special cases can be handled. | Duyet hoan thu cong | Refund admin workflow |
| ADM-10 | As an admin, I want to verify fallback bank transfers (dual-admin review for high value) so that payment confirmation is safe. | Manual bank transfer verification | Bank QR reconciliation fallback |
| ADM-11 | As an admin, I want to view revenue reports by day/month so that I can track business performance. | Bao cao doanh thu | Dashboard report module |
| ADM-12 | As an admin, I want to export reports to Excel so that I can share and archive data. | Xuat bao cao Excel | Export module (S3/output file) |
| ADM-13 | As an admin, I want auditable changes on pricing/court/refund actions so that governance is enforced. | Audit log cho thao tac quan tri | `audit_log` |

## Public/Provider Stories

| ID | User Story | Function (He thong) | API/Man hinh lien quan |
|---|---|---|---|
| PUB-01 | As a public visitor, I want to view availability without login so that I can decide whether to register/book. | Public availability read | `GET /api/availability` |
| PAY-01 | As VNPay provider, I want to send IPN callbacks so that payment state is synchronized server-to-server. | Xu ly webhook VNPay + verify signature | `POST /payments/vnpay/ipn` |
| PAY-02 | As bank QR provider (optional), I want to callback payment result so that booking can be confirmed automatically. | Xu ly bank QR callback + idempotency | `POST /payments/bankqr/callback` |

## System Stories (Automated Functions)

| ID | User Story | Function (He thong) | Trigger |
|---|---|---|---|
| SYS-01 | As the system, I want to prevent double booking for same court and same time so that inventory remains correct. | Row locking + invariant checks | Trong `POST /api/holds` transaction |
| SYS-02 | As the system, I want to enforce max 8 shared slots per time frame so that shared capacity is never exceeded. | shared_reserved <= 8 | Trong `POST /api/holds` transaction |
| SYS-03 | As the system, I want to expire stale holds automatically so that inventory is released promptly. | Hold expiration job | Background worker schedule |
| SYS-04 | As the system, I want to generate check-in codes only after successful payment so that fraud/race conditions are reduced. | Post-payment code generation | Payment confirmation flow |
| SYS-05 | As the system, I want to reset/expire end-of-day active check-in codes so that old codes cannot be reused. | End-of-day code expiry | Daily worker job |
| SYS-06 | As the system, I want payment/webhook handling to be idempotent so that duplicate callbacks do not duplicate bookings. | Provider dedupe + guarded state transition | IPN/callback processing |
| SYS-07 | As the system, I want to award loyalty points only when booking is completed so that rewards are fair. | Loyalty awarding job | Booking completion event |
| SYS-08 | As the system, I want to retry transient deadlocks safely so that booking creation remains robust under load. | Deadlock-safe retry with jitter | Booking/hold transaction layer |
| SYS-09 | As the system, I want to enforce secure coding controls (parameterized queries, model validation, CSRF, secure cookies, secure headers, rate limiting) so that app-layer security is consistent. | Security middleware + coding standards | Runtime middleware + code review gates |

## Mapping To Requirement Modules

- Module Tai khoan nguoi dung: `CUS-01..CUS-05`
- Module Dat san: `CUS-06..CUS-11`, `SYS-01..SYS-03`
- Module Cau hinh gia: `ADM-03..ADM-05`
- Module Thanh toan: `CUS-12..CUS-15`, `PAY-01..PAY-02`, `SYS-06`
- Module Check-in: `ADM-07..ADM-08`, `SYS-04..SYS-05`
- Module Chinh sach huy/hoan tien: `CUS-16..CUS-17`, `ADM-09..ADM-10`
- Module Tich diem & phan hang: `CUS-18..CUS-19`, `SYS-07`
- Module Dashboard quan tri: `ADM-02`, `ADM-06`, `ADM-11..ADM-13`
