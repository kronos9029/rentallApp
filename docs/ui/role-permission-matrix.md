# Role Permission Matrix

## 1) Pages

| Page/Feature | Public | Customer | Admin |
|---|---:|---:|---:|
| Register/Login/Forgot Password | Y | Y | Y |
| Availability View | Y | Y | Y |
| Create Hold | N | Y | N |
| Checkout/Payment | N | Y | N |
| Booking History/Detail | N | Y (own only) | N |
| Cancel Booking | N | Y (own only) | N |
| Admin Dashboard | N | N | Y |
| Court Management | N | N | Y |
| Pricing Rules | N | N | Y |
| Check-in Validation | N | N | Y |
| Refund Approval | N | N | Y |
| Revenue Reports | N | N | Y |
| Audit Logs | N | N | Y |

## 2) API Access Rules
- Customer APIs require authenticated customer session/token.
- Admin APIs require admin role.
- Public endpoints only for non-sensitive read/payment return hooks.
