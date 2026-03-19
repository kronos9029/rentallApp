# Sprint Scope Coverage Matrix

Ma tran nay map task trong `docs/sprint/sprint-01-tasks.md`, `docs/sprint/sprint-02-tasks.md`, va `docs/sprint/sprint-02-smoke-checklist.md` sang test automation trong workspace Playwright nay.

Ky hieu:

- `Direct`: co test assert truc tiep
- `Indirect`: duoc cover qua artifact/command/behavior lien quan
- `Env-gated`: co test, nhung can bat env/phu tro runtime de chay that

## Sprint 01

| Task | Coverage | Test/Note |
|---|---|---|
| S1-T01 | Direct | `TC-FOUND-01` |
| S1-T02 | Indirect | `TC-FOUND-06`, `TC-SEC-01`, `TC-AUTH-01..03` |
| S1-T03 | Direct | `TC-FOUND-03` |
| S1-T04 | Direct | `TC-FOUND-04` |
| S1-T05 | Direct | `TC-FOUND-02` |
| S1-T06 | Direct | `TC-AUTH-02`, `TC-AUTH-04` |
| S1-T07 | Direct | `TC-AUTH-02`, `TC-AUTH-05`, `TC-AUTH-06` |
| S1-T08 | Direct/Env-gated | `TC-EMAIL-01`, `TC-EMAIL-02`, `TC-EMAIL-03` |
| S1-T09 | Direct | `TC-AUTH-02`, `TC-PRO-01..03` |
| S1-T10 | Direct | `TC-AUTH-03`, `TC-AUTH-07..09` |
| S1-T11 | Direct | `TC-AUTH-03`, `TC-AUTH-09` |
| S1-T12 | Direct | `TC-BOOT-01`, `TC-BOOT-02`, `TC-FOUND-06` |
| S1-T13 | Direct | `TC-MIG-01`, `TC-FOUND-06` |
| S1-T14 | Direct | `TC-MIG-01`, `TC-FOUND-06` |
| S1-T15 | Direct | `TC-MIG-01`, `TC-API-01`, `TC-BOOK-11` |
| S1-T16 | Indirect | booking/API suite dung `courts[0]`, `TC-BOOK-02`, `TC-BOOK-API-*` |
| S1-T17 | Direct | `TC-API-01`, `TC-BOOK-11` |
| S1-T18 | Direct | `TC-API-01`, `TC-BOOK-11` |
| S1-T19 | Direct | `TC-SEC-01`, `TC-SEC-02`, `TC-SEC-03`, `TC-AUTH-09` |
| S1-T20 | Direct | `TC-FOUND-05` |
| S1-T21 | Direct/Env-gated | `TC-FOUND-05`, `TC-FOUND-06` |
| S1-T22 | Direct/Env-gated | `TC-FOUND-06` |

## Sprint 02

| Task | Coverage | Test/Note |
|---|---|---|
| S2-T01 | Direct | `TC-API-01`, `TC-API-02` |
| S2-T02 | Direct | `TC-MIG-01`, `TC-FOUND-06` |
| S2-T03 | Direct | `TC-API-01`, `TC-BOOK-02`, `TC-BOOK-09` |
| S2-T04 | Direct | `TC-API-01`, `TC-BOOK-11` |
| S2-T05 | Indirect | availability suite lap lai request/runtime; chua assert cache internals truc tiep |
| S2-T06 | Direct | `TC-BOOK-02`, `TC-BOOK-09`, `TC-MOB-01` |
| S2-T07 | Direct | `TC-BOOK-API-03`, `TC-BOOK-API-04`, `TC-BOOK-05`, `TC-BOOK-06` |
| S2-T08 | Direct | `TC-BOOK-API-01`, `TC-API-03` |
| S2-T09 | Direct | `TC-RETRY-01`, `TC-RETRY-02`, `TC-FOUND-06` |
| S2-T10 | Direct | `TC-BOOK-09`, `TC-BOOK-11` |
| S2-T11 | Direct | `TC-BOOK-01`, `TC-BOOK-09` |
| S2-T12 | Direct/Env-gated | `TC-WORKER-01` |
| S2-T13 | Direct/Env-gated | `TC-WORKER-01` |
| S2-T14 | Direct | `TC-BOOK-01`, `TC-BOOK-10`, `TC-BOOK-API-02` |
| S2-T15 | Direct | `TC-BOOK-API-02`, `TC-API-04` |
| S2-T16 | Direct | `TC-BOOK-01`, `TC-BOOK-10` |
| S2-T17 | Direct | `TC-SMTP-01`, `TC-SMTP-02`, `TC-SMTP-04` |
| S2-T18 | Direct/Env-gated | `TC-EMAIL-01`, `TC-EMAIL-03` |
| S2-T19 | Direct/Env-gated | `TC-EMAIL-02`, `TC-EMAIL-03` |
| S2-T20 | Direct | `TC-SMTP-03`, `TC-SMTP-04`, `TC-EMAIL-03` |
| S2-T21 | Direct/Env-gated | `TC-FOUND-06`, full regression pack |
| S2-T22 | Direct | smoke suite + `TC-EMAIL-01..03`, `TC-WORKER-01` |

## Sprint 02 Smoke Checklist Mapping

| Checklist Flow | Coverage |
|---|---|
| Login customer | `TC-AUTH-02`, smoke auth flow |
| Availability private | `TC-BOOK-01` |
| Create private hold | `TC-BOOK-01` |
| Checkout from hold | `TC-BOOK-01`, `TC-BOOK-10` |
| Availability shared | `TC-BOOK-02`, `TC-BOOK-09`, `TC-MOB-01` |
| Create shared hold | `TC-BOOK-09`, `TC-BOOK-11` |
| Hold expiry worker | `TC-WORKER-01` |
| Forgot password email | `TC-EMAIL-01`, `TC-EMAIL-03` |
| Reset password | `TC-EMAIL-02`, `TC-EMAIL-03` |
