# Component Specification

## 1) Base Components

| Component | Usage | Required States |
|---|---|---|
| Button | primary actions, secondary actions | default, hover, focus, disabled, loading |
| Input/TextField | forms | default, focus, error, disabled |
| Select | option pickers | default, focus, error, disabled |
| Date Picker | booking date | default, selected, disabled date |
| Time Slot Card | slot selection | available, selected, unavailable |
| Badge | status labels | info, success, warning, danger |
| Alert | feedback | success, warning, error |
| Modal | confirmation dialogs | open, close, loading |
| Table | admin lists/reports | loading, empty, populated, error |
| Pagination | list navigation | default, active, disabled |
| Skeleton | loading placeholders | page, list, card |
| Toast | transient feedback | success, warning, error |

## 2) Booking-specific Components

| Component | Purpose |
|---|---|
| AvailabilityFilterBar | date/mode/court filters |
| CourtAvailabilityGrid | private/shared availability matrix |
| HoldSummaryCard | hold details + expiration countdown |
| PriceBreakdownCard | subtotal, surcharge, total |
| PaymentMethodSelector | VNPay vs Bank QR selection |
| QrPaymentPanel | render QR + amount + reference |
| BookingStatusTimeline | pending -> paid -> confirmed |
| CheckinCodeCard | code display + status |

## 3) Admin-specific Components

| Component | Purpose |
|---|---|
| AdminKpiCards | KPI summary on dashboard |
| BookingManagementTable | booking list/filter/actions |
| PricingRuleForm | rule CRUD |
| CheckinValidationPanel | code input + validation output |
| RefundApprovalTable | approve/reject actions |
| AuditLogTable | audit browsing/filter |

## 4) Accessibility Rules
- All interactive elements keyboard accessible.
- Visible focus ring for all controls.
- Color contrast should meet WCAG AA minimum.
- Form controls must have labels and error text bound to input.
