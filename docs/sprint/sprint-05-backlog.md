# Sprint 05 Backlog

## Sprint Goal
Hoan thien loyalty, reporting, audit viewer, va security/resilience hardening.

## Committed Product Backlog Items
| PB ID | Story IDs | Item | SP |
|---|---|---|---:|
| PB-18 | CUS-18,CUS-19,SYS-07 | Loyalty points + tiering | 8 |
| PB-19 | ADM-11,ADM-12 | Revenue report + Excel export | 8 |
| PB-20 | ADM-13 | Audit viewer + search/filter/reporting | 5 |
| PB-21 | SYS-09 | Security hardening | 8 |
| PB-22 | SYS-08 | Extended resilience + concurrency/webhook replay regression pack | 5 |

Total SP: 34

## Sprint Tasks
1. Add loyalty account + ledger + tier update job.
2. Implement report query and export file generation.
3. Build audit viewer/filter/search va query support cho admin.
4. Complete security checklist: headers, rate limiting, cookie, CSRF, validation.
5. Expand resilience pack: concurrency regression, webhook replay tests, va retry/error-handling hardening.

## Acceptance Criteria
- Loyalty points chi cong khi booking completed.
- Report day/month va export Excel dung so lieu.
- Admin co the search/filter audit events cho pricing/court/refund/check-in actions.
- Security scan/QA checklist pass.
- Concurrency regression va webhook replay regression pass.
