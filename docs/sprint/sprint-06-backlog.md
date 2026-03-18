# Sprint 06 Backlog

## Sprint Goal
Stabilization, measurable performance optimization, va readiness for production release.

## Committed Product Backlog Items
| PB ID | Story IDs | Item | SP |
|---|---|---|---:|
| PB-23 | CUS-06,CUS-14 | Mobile performance optimization | 8 |
| PB-24 | SYS-03,SYS-05,SYS-07 | Async reliability hardening + worker retry/poison handling | 8 |
| PB-25 | ALL | UAT, bug-fix, release readiness | 13 |

Total SP: 29

## Sprint Tasks
1. Tune mobile critical flows (availability, checkout, payment status, booking detail).
2. Finalize async reliability: worker retry/poison handling, replay/recovery utilities, va operational visibility.
3. Full E2E regression (Customer + Admin flows).
4. Production checklist: migration, backup, monitoring, alerting, rollback plan.
5. UAT fix cycle and release sign-off.

## Acceptance Criteria
- Tren profile mobile tam trung + 4G tham chieu, `LCP <= 2.5s`, `INP <= 200ms`, `CLS <= 0.1` cho availability, checkout, va payment status.
- Async jobs/events khong bi mat trong test failure scenarios; poison items duoc quarantine va recover duoc.
- UAT pass cho all P0/P1 stories, bao gom booking history/detail va admin ops.
- Release artifacts complete: migration plan, backup/restore checklist, monitoring/alert config, rollback steps.
