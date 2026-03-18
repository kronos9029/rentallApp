# Sprint 01 Backlog

## Sprint Goal
Hoan thanh foundation, identity, admin access, va pricing baseline de mo duong cho booking core.

## Committed Product Backlog Items
| PB ID | Story IDs | Item | SP |
|---|---|---|---:|
| PB-01 | SYS-09 | Project bootstrap + architecture skeleton | 8 |
| PB-02 | CUS-01,CUS-02,CUS-03 | Register/Login/Forgot password | 8 |
| PB-03 | CUS-04 | Profile management | 3 |
| PB-03A | ADM-01 | Admin access + area authorization baseline | 3 |
| PB-04 | SYS-01,SYS-02 | Core data model + migrations | 8 |
| PB-04A | ADM-03,ADM-04,ADM-05 | Default pricing seed + read-side price calculation baseline | 5 |

Total SP: 35

## Sprint Tasks
1. Setup solution structure theo SDD (Web/Application/Domain/Infrastructure/Worker).
2. Implement auth flow va role seed (Admin/Customer).
3. Implement admin area authorization conventions va role-based route protection baseline.
4. Implement DB migration cho users, roles, courts, time_buckets, court_buckets, pricing_rules.
5. Seed 9 courts va bo pricing/cancellation baseline cho local environment.
6. Add secure baseline: validation, CSRF, secure cookies, secure headers middleware.
7. Add sensitive config baseline: encrypted config bootstrap + AES key inject qua environment variable runtime.
8. Add CI pipeline + lint + unit test baseline.

## Acceptance Criteria
- Customer co the register/login/reset password.
- Admin co the dang nhap bang shared auth flow va chi truy cap duoc admin-protected areas.
- DB schema foundation apply duoc tren local environment, bao gom pricing tables.
- 9 courts va default pricing rules seed thanh cong.
- Price calculation baseline tra dung gia cho private/shared va weekday/weekend cases.
- Security baseline middleware active.
- Sensitive config khong nam o plaintext config cho production-like flow; encrypted config bootstrap va AES runtime key convention da duoc thiet lap.
