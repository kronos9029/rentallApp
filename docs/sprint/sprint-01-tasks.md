# Sprint 01 Task Breakdown

## Sprint Goal
Hoan thanh foundation, identity, admin access, va pricing baseline de mo duong cho booking core.

## Scope
- `PB-01`: Project bootstrap + architecture skeleton
- `PB-02`: Register/Login/Forgot password
- `PB-03`: Profile management
- `PB-03A`: Admin access + area authorization baseline
- `PB-04`: Core data model + migrations
- `PB-04A`: Default pricing seed + read-side price calculation baseline

## Part 1
Foundation, DevOps, Auth, Admin Access

| Task ID | PB ID | Task | Output mong doi | Dependency | Status |
|---|---|---|---|---|---|
| S1-T01 | PB-01 | Tao solution structure theo SDD (`Web/Application/Domain/Infrastructure/Worker/tests/build`) | Solution skeleton chay duoc, folder structure khop SDD | None | DONE |
| S1-T02 | PB-01 | Cau hinh base app startup, DI, logging, env config | `Program.cs` bootstrap duoc cho web app va worker | S1-T01 | DONE |
| S1-T03 | PB-01 | Tao Dockerfile baseline cho web app va worker | Co `web.Dockerfile` va `worker.Dockerfile` build duoc | S1-T01 | DONE |
| S1-T04 | PB-01 | Tao local container/dev environment (`docker-compose` hoac script tuong duong) | Local stack co the boot app + DB phuc vu dev/test | S1-T03 | DONE |
| S1-T05 | PB-01 | Tao CI/CD pipeline baseline | Pipeline co build, test, image build, artifact/package step | S1-T01,S1-T03 | DONE |
| S1-T06 | PB-02 | Implement register flow | Customer dang ky duoc voi email/password | S1-T02 | DONE |
| S1-T07 | PB-02 | Implement login flow | Customer dang nhap duoc | S1-T06 | DONE |
| S1-T08 | PB-02 | Implement forgot/reset password flow | Link reset password het han hoat dong dung | S1-T06 | DONE |
| S1-T09 | PB-03 | Implement profile read/update | Customer xem/sua profile duoc | S1-T07 | DONE |
| S1-T10 | PB-03A | Seed role `Admin` va `Customer` | Role seed local environment thanh cong | S1-T02 | DONE |
| S1-T11 | PB-03A | Cau hinh area authorization cho admin pages/routes | Admin area bi chan voi user khong co role | S1-T10 | DONE |

## Part 2
Database, Pricing, Security, Encrypted Config, Test Baseline

| Task ID | PB ID | Task | Output mong doi | Dependency | Status |
|---|---|---|---|---|---|
| S1-T12 | PB-04 | Tao database bootstrap script/config | Local environment tao duoc database app va ket noi duoc tu app/container | S1-T02,S1-T04 | DONE |
| S1-T13 | PB-04 | Tao migrations cho `users`, `roles`, `user_profiles`, `password_reset_tokens` | Schema identity apply duoc local | S1-T12 | DONE |
| S1-T14 | PB-04 | Tao migrations cho `courts`, `time_buckets`, `court_buckets` | Schema booking foundation apply duoc local | S1-T13 | DONE |
| S1-T15 | PB-04A | Tao migrations cho `pricing_rules` va `cancellation_policies` | Schema pricing/policy apply duoc local | S1-T13 | DONE |
| S1-T16 | PB-04 | Seed 9 courts | Du lieu `C01..C09` co san trong local DB | S1-T14 | DONE |
| S1-T17 | PB-04A | Seed pricing/cancellation baseline | Co bo rule mac dinh cho private/shared, weekday/weekend | S1-T15 | DONE |
| S1-T18 | PB-04A | Implement read-side price calculation service | Service tra dung gia cho private/shared va weekday/weekend | S1-T17 | DONE |
| S1-T19 | PB-01 | Add secure baseline: validation, CSRF, secure cookies, secure headers | Middleware/security config active | S1-T02 | DONE |
| S1-T20 | PB-01 | Define sensitive config strategy | Chot convention: sensitive config encrypted, AES key inject qua env runtime, khong commit plaintext secrets | S1-T02 | DONE |
| S1-T21 | PB-01 | Implement encrypted config bootstrap | App/worker doc va bootstrap code doc duoc encrypted config o local/prod-like environment | S1-T20 | DONE |
| S1-T22 | PB-01 | Viet unit/integration test baseline cho auth, pricing, security, config bootstrap | Test baseline co the chay trong CI | S1-T05,S1-T18,S1-T19,S1-T21 | DONE |

## Suggested Sequence
1. Part 1: hoan thanh skeleton, Docker, CI/CD, auth, role/authorization (`S1-T01..S1-T11`).
2. Part 2: hoan thanh database bootstrap, pricing, security, encrypted config, va test baseline (`S1-T12..S1-T22`).

## Current Progress
- Part 1 da co skeleton solution, web/worker Dockerfile, `docker-compose`, CI workflow, va auth baseline cho login/register/admin access.
- Part 1 da co forgot/reset password flow voi token reset in-memory co expiry va page reset password dung duoc trong moi truong dev.
- Part 1 da co profile read/update va refresh lai auth claims sau khi cap nhat ten hien thi.
- Part 2 da co EF Core persistence foundation, migration `Sprint01Foundation`, local DB bootstrap script, va local native MySQL bootstrap qua host port `3306`.
- Part 2 da seed duoc `Admin/Customer`, `C01..C09`, pricing baseline weekday/weekend, va cancellation policy baseline.
- Part 2 da co pricing read-side service, encrypted config loader dung AES key tu env runtime, unit tests, integration smoke tests, va xac minh migration apply thanh cong tren local DB.
- Auth baseline hien da duoc chuyen sang DB-backed persistence that cho login/register/profile/forgot-reset password va khong con phu thuoc `DevelopmentAuthStore`.

## Done Checklist
- Customer co the register/login/reset password.
- Admin dang nhap duoc va khong truy cap duoc area admin neu thieu role.
- Local database duoc tao/bootstrap duoc cho moi truong dev.
- Docker environment co the dung de chay local stack co ban.
- CI/CD pipeline baseline chay duoc build, test, va package/image step.
- Schema Sprint 1 apply duoc tren local environment.
- Seed 9 courts va pricing baseline thanh cong.
- Price calculation baseline pass test cho private/shared va weekday/weekend.
- Security baseline active.
- Sensitive config strategy duoc chot ro: encrypted config cho secret nhay cam, AES key inject qua environment variable runtime.
