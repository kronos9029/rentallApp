# Sprint 02 Backlog

## Sprint Goal
Hoan thanh availability + hold + checkout voi pricing dung va concurrency baseline tin cay.

## Committed Product Backlog Items
| PB ID | Story IDs | Item | SP |
|---|---|---|---:|
| PB-05 | CUS-06,CUS-09,PUB-01 | Availability API + mobile UI | 8 |
| PB-06 | CUS-07,CUS-08,SYS-01,SYS-02 | Hold creation with locking | 13 |
| PB-07 | CUS-10,SYS-03 | Multi-slot hold + expiry worker | 8 |
| PB-08 | CUS-10,CUS-20 | Checkout + idempotency | 8 |
| PB-08C | CUS-03 | SMTP/email dispatch for password reset with expiring reset link | 3 |
| PB-08B | SYS-08 | Deadlock-safe retry baseline for booking/hold flow | 5 |

Total SP: 45

## Sprint Tasks
1. Build availability query + in-memory cache short TTL.
2. Integrate price calculation vao hold summary va checkout dua tren default pricing rules.
3. Implement hold transaction (`SELECT ... FOR UPDATE`) and inventory invariants.
4. Add deadlock-safe retry with jitter cho booking/hold transaction layer.
5. Build hold expiry background job.
6. Implement checkout API and idempotency key persistence.
7. Implement SMTP/email sender for forgot password and send reset link co expiry thay cho dev-only on-screen link.
8. Mobile-first screens: availability, hold summary, checkout.

## Acceptance Criteria
- No double booking under concurrent requests.
- Shared booking khong vuot qua 8 slots.
- Hold co `expires_at` va auto release inventory khi expired.
- Retry hold/checkout khong tao duplicate va deadlock transient duoc retry an toan.
- Hold summary va checkout total khop pricing rules cho private/shared va weekday/weekend cases.
- Forgot password gui email reset link co thoi han va reset link het han bi tu choi dung.
