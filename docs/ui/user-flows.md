# User Flows

## Flow 1: Customer Booking (Private/Shared)
1. Open Availability page.
2. Select date/time and booking mode (Private or Shared).
3. Select court/slot quantity and submit hold.
4. Review hold summary and proceed to checkout.
5. Choose payment method (VNPay or Bank QR).
6. Complete payment.
7. Return to Payment Status page.
8. If paid, booking is confirmed and check-in code appears in booking detail.

## Flow 2: Customer Cancel Booking
1. Open Booking Detail.
2. Click Cancel.
3. System checks policy (`>=2h` rule).
4. Show result:
- eligible: submit cancellation request / refund path.
- ineligible: show policy message.

## Flow 3: Admin Check-in
1. Open Admin Check-in page.
2. Input check-in code.
3. Submit validate.
4. System returns status: valid, used, expired, invalid.
5. For valid code: mark USED and show court/time confirmation.

## Flow 4: Admin Pricing Update
1. Open Pricing Rules.
2. Create or edit rule (time range, private/shared rates, weekend markup).
3. Save.
4. System validates overlap/effective range.
5. Audit log entry created.

## Flow 5: Admin Refund Approval
1. Open Refund Approval queue.
2. Review request details.
3. Approve or reject.
4. System updates refund/payment/booking status.
5. Audit log entry created.
