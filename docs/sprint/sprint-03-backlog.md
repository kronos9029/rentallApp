# Sprint 03 Backlog

## Sprint Goal
Closed beta duoc luong payment VNPay, booking history/detail, va check-in end-to-end co audit baseline.

## Committed Product Backlog Items
| PB ID | Story IDs | Item | SP |
|---|---|---|---:|
| PB-08A | CUS-05 | Booking history + booking detail + re-open check-in codes | 5 |
| PB-09 | CUS-12,CUS-14,PAY-01 | VNPay integration | 13 |
| PB-10 | CUS-15,SYS-06 | Payment state machine + webhook dedupe | 8 |
| PB-11 | CUS-11,SYS-04 | Booking finalize + check-in code generation | 8 |
| PB-12 | ADM-07,ADM-08,SYS-05 | Admin check-in screen + redeem | 8 |
| PB-12A | ADM-13 | Baseline audit logging for critical admin actions | 3 |

Total SP: 45

## Sprint Tasks
1. Implement booking history/detail APIs va customer screens de xem booking va mo lai check-in code.
2. Implement VNPay initiate/return/ipn endpoints.
3. Validate webhook signature + amount + reference.
4. Implement payment idempotent transition PENDING -> PAID/FAILED va durable event write cho downstream jobs.
5. Generate checkin code after paid va persist status lifecycle.
6. Build admin check-in page, redeem transaction, va end-of-day expiry job.
7. Add baseline audit logging cho check-in, pricing, refund, va court actions.

## Acceptance Criteria
- Customer co the xem booking history/detail va mo lai check-in code theo status `ACTIVE/USED/EXPIRED`.
- VNPay callback duplicate khong duplicate booking.
- Check-in code chi duoc tao khi payment thanh cong.
- Code dung 1 lan duy nhat, sau do status USED.
- End-of-day expiry job chay dung.
- Critical admin actions va check-in redeem co audit log entry day du.
