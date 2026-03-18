# UI Definition of Done (DoD)

## 1) Functional
- Story acceptance criteria pass.
- All required states implemented (loading, empty, error, success).
- Correct role-based visibility and action permissions.

## 2) Quality
- Responsive on key breakpoints (320, 375, 768, 1024).
- No horizontal overflow on mobile.
- Form validation messages are clear and actionable.

## 3) Accessibility
- Keyboard navigation works.
- Focus visible for interactive controls.
- Inputs have labels and accessible error text.

## 4) Security
- CSRF token present for state-changing form posts.
- Sensitive fields are not prefilled/logged in UI console.
- Error display does not leak sensitive internal details.

## 5) Integration
- API integration done with proper error handling and retries where needed.
- Idempotency-Key used for create/transition calls.
- Payment callback/update scenarios verified.

## 6) Testing
- UI unit/component tests (where applicable).
- E2E test coverage for critical mobile flows:
  - Availability -> Hold -> Checkout -> Payment Status
  - Booking Detail -> Check-in Code
  - Admin Check-in Validation

## 7) Documentation
- Screen changes reflected in `screen-map.md`.
- New/updated components reflected in `component-spec.md`.
- Validation/content changes reflected in `content-and-validation-spec.md`.
