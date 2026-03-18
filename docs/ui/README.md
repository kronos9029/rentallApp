# UI Documentation Index

Purpose: provide all inputs needed for UI development without waiting for backend completion.

## Files
- `docs/ui/screen-map.md`: full page inventory and route mapping.
- `docs/ui/user-flows.md`: primary user flows (Customer + Admin).
- `docs/ui/component-spec.md`: reusable component rules and states.
- `docs/ui/design-tokens-and-style-guide.md`: color, spacing, typography, breakpoints, Bootstrap/Tailwind usage.
- `docs/ui/api-contract-for-ui.md`: endpoint contract for UI integration.
- `docs/ui/content-and-validation-spec.md`: field labels, validation, error messages.
- `docs/ui/role-permission-matrix.md`: page/action access by role.
- `docs/ui/ui-definition-of-done.md`: acceptance criteria for UI delivery.

## UI Tech Stack Decision
- Rendering: ASP.NET Core Razor Pages / MVC.
- Customer pages: Tailwind-first.
- Admin pages: Bootstrap-first.
- Rule: do not mix Bootstrap and Tailwind utility classes in the same page/component.

## Sprint Mapping (from Product Backlog)
- Sprint 1: UI foundation, shared auth/profile screens, admin access guard baseline.
- Sprint 2: availability, hold summary, checkout, price breakdown validation.
- Sprint 3: payment status, booking history/detail, admin check-in, audit baseline hooks.
- Sprint 4: admin pricing/court/booking/refund pages.
- Sprint 5-6: audit viewer, polish, optimization, accessibility/security/performance hardening.
