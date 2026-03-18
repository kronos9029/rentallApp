# Screen Map

## 1) Public / Customer Screens

| Screen ID | Screen Name | Route (proposed) | Actor | Story IDs | Sprint |
|---|---|---|---|---|---|
| SCR-AUTH-01 | Register | `/auth/register` | Public | CUS-01 | S1 |
| SCR-AUTH-02 | Login | `/auth/login` | Public | CUS-02 | S1 |
| SCR-AUTH-03 | Forgot Password | `/auth/forgot-password` | Public | CUS-03 | S1 |
| SCR-AUTH-04 | Reset Password | `/auth/reset-password` | Public | CUS-03 | S1 |
| SCR-PRO-01 | Profile | `/account/profile` | Customer | CUS-04 | S1 |
| SCR-AVL-01 | Availability | `/booking/availability` | Public/Customer | CUS-06,CUS-09,PUB-01 | S2 |
| SCR-HOLD-01 | Hold Summary | `/booking/hold/{holdId}` | Customer | CUS-07,CUS-08,CUS-10 | S2 |
| SCR-CHK-01 | Checkout | `/checkout` | Customer | CUS-10,CUS-20 | S2 |
| SCR-PAY-01 | Payment Method | `/payment/method` | Customer | CUS-12,CUS-13 | S3/S4 |
| SCR-PAY-02 | Payment Status | `/payment/status/{orderId}` | Customer | CUS-14,CUS-15 | S3 |
| SCR-HIS-01 | Booking History | `/bookings` | Customer | CUS-05,CUS-14 | S3 |
| SCR-HIS-02 | Booking Detail | `/bookings/{bookingId}` | Customer | CUS-05,CUS-11 | S3 |
| SCR-CAN-01 | Cancel Booking | `/bookings/{bookingId}/cancel` | Customer | CUS-16,CUS-17 | S4 |

## 2) Admin Screens

| Screen ID | Screen Name | Route (proposed) | Actor | Story IDs | Sprint |
|---|---|---|---|---|---|
| SCR-ADM-01 | Admin Dashboard | `/admin` | Admin | ADM-06 | S4 |
| SCR-ADM-02 | Court Management | `/admin/courts` | Admin | ADM-02 | S4 |
| SCR-ADM-03 | Pricing Rules | `/admin/pricing` | Admin | ADM-03,ADM-04,ADM-05 | S4 |
| SCR-ADM-04 | Booking Management | `/admin/bookings` | Admin | ADM-06 | S4 |
| SCR-ADM-05 | Refund Approval | `/admin/refunds` | Admin | ADM-09,ADM-10 | S4 |
| SCR-ADM-06 | Check-in Validation | `/admin/checkin` | Admin | ADM-07,ADM-08 | S3 |
| SCR-ADM-07 | Revenue Report | `/admin/reports/revenue` | Admin | ADM-11 | S5 |
| SCR-ADM-08 | Export Report | `/admin/reports/export` | Admin | ADM-12 | S5 |
| SCR-ADM-09 | Audit Viewer | `/admin/audit` | Admin | ADM-13 | S5 |

## 3) Shared System Screens

| Screen ID | Screen Name | Route (proposed) | Actor |
|---|---|---|---|
| SCR-SYS-01 | 403 Unauthorized | `/403` | All |
| SCR-SYS-02 | 404 Not Found | `/404` | All |
| SCR-SYS-03 | 500 Error | `/500` | All |
| SCR-SYS-04 | Maintenance | `/maintenance` | All |
