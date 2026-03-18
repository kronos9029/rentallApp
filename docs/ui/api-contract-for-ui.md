# API Contract for UI

## 1) Auth and Profile
- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/forgot-password`
- `POST /auth/reset-password`
- `GET /api/account/profile`
- `PUT /api/account/profile`

## 2) Availability and Hold
- `GET /api/availability?date=YYYY-MM-DD`
- `POST /api/holds` (requires `Idempotency-Key`)
- `DELETE /api/holds/{holdId}`

## 3) Checkout and Payment
- `POST /api/checkout` (requires `Idempotency-Key`)
- `POST /api/payments/vnpay/initiate` (requires `Idempotency-Key`)
- `GET /payments/vnpay/return`
- `POST /payments/vnpay/ipn`
- `POST /api/payments/bankqr/create` (requires `Idempotency-Key`)
- `POST /payments/bankqr/callback`

## 4) Booking and Check-in
- `GET /api/bookings`
- `GET /api/bookings/{id}`
- `POST /api/bookings/{id}/cancel` (requires `Idempotency-Key`)
- `POST /api/checkin/validate` (Admin)

## 5) Admin Operations
- `PUT /admin/courts/{id}`
- `PUT /admin/pricing/rules/{id}`
- `GET /admin/reports/revenue`
- `GET /admin/reports/export`

## 6) Response Envelope (recommended)

```json
{
  "success": true,
  "message": "optional",
  "data": {},
  "errors": []
}
```

## 7) Common Error Codes
- `400_VALIDATION_ERROR`
- `401_UNAUTHORIZED`
- `403_FORBIDDEN`
- `404_NOT_FOUND`
- `409_CONFLICT`
- `422_BUSINESS_RULE_VIOLATION`
- `429_RATE_LIMITED`
- `500_INTERNAL_ERROR`

## 8) UI Integration Notes
- All create/transition requests must send `Idempotency-Key`.
- UI should support duplicated callback scenario (do not assume single webhook hit).
- Payment status screen must be polling-friendly when callback delays occur.
