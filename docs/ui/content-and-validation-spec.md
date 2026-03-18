# Content and Validation Spec

## 1) Booking Form Validation
- `date`: required, must be valid future-or-allowed booking day.
- `bookingMode`: required (`PRIVATE` or `SHARED`).
- `courtId`: required.
- `slotQty`: required for shared, integer >=1 and <= available slots.

## 2) Checkout Validation
- Hold must be ACTIVE and not expired.
- Order total must be > 0.
- Payment method required.

## 3) Check-in Validation
- Code format: `PB-XXXXXX` (6-8 chars after prefix).
- Must exist.
- Must be ACTIVE and inside valid time window.

## 4) Pricing Rule Validation
- `effective_from` required.
- `effective_to` optional but must be greater than `effective_from`.
- `private_rate` and `shared_rate` > 0.
- `weekend_markup_pct` in range 0-100.

## 5) Suggested Error Messages (UI)
- "Selected slot is no longer available. Please refresh and try again."
- "Hold has expired. Please create a new hold."
- "Payment is still pending. Please wait or refresh status."
- "Invalid or expired check-in code."
- "Cancellation is only allowed at least 2 hours before start time."
