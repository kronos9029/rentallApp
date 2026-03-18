# Sprint 04 Backlog

## Sprint Goal
Hoan thien payment option thu 2, cancellation/refund, va admin pricing/ops.

## Committed Product Backlog Items
| PB ID | Story IDs | Item | SP |
|---|---|---|---:|
| PB-13 | CUS-13,PAY-02 | Bank QR dynamic payment | 8 |
| PB-14 | CUS-16,CUS-17 | Cancellation policy engine | 8 |
| PB-15 | ADM-09,ADM-10 | Manual refund workflow | 8 |
| PB-16 | ADM-03,ADM-04,ADM-05 | Pricing rules management | 8 |
| PB-17 | ADM-02,ADM-06 | Court & booking admin operations | 8 |

Total SP: 40

## Sprint Tasks
1. Implement bank QR create/callback and reconciliation status.
2. Implement cancellation eligibility check (>=2h) va block refund path khi <2h.
3. Add cancellation request/refund request + admin approval flow.
4. Build pricing rules CRUD voi overlap validation va effective window.
5. Build admin pages: court active/maintenance + booking list management tren audit baseline.

## Acceptance Criteria
- Cancellation eligibility va refund amount khop active policy cho cac truong hop >=2h, <2h, full/partial/wallet.
- Bank QR co the di qua `PENDING/PAID/FAILED/EXPIRED` ma khong duplicate transition.
- Pricing rule overlap/effective range duoc validate va pricing update co hieu luc trong booking price calculation.
- Admin co the process manual refund, update court status, va quan ly booking; critical actions deu co audit.
