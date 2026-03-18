# Design Tokens and Style Guide

## 1) Style System Policy
- Customer pages: Tailwind-first.
- Admin pages: Bootstrap-first.
- Do not mix utility classes from both systems in same page/component.

## 2) Breakpoints (mobile-first)
- xs: 320-479
- sm: 480-767
- md: 768-1023
- lg: 1024-1279
- xl: 1280+

## 3) Spacing Scale
- 4, 8, 12, 16, 20, 24, 32, 40, 48

## 4) Radius
- sm: 6
- md: 10
- lg: 14

## 5) Typography
- Base body: 14-16px (mobile)
- Heading scale:
  - H1: 28
  - H2: 24
  - H3: 20
  - H4: 18

## 6) Semantic Colors
- `--color-primary`: brand primary action
- `--color-success`: paid/confirmed/success states
- `--color-warning`: pending states
- `--color-danger`: failed/error/rejected states
- `--color-neutral-*`: text/surface/border

## 7) Status Mapping
- Booking:
  - Pending -> warning
  - Confirmed -> success
  - Cancelled -> danger
  - Completed -> info/success
- Payment:
  - Pending -> warning
  - Paid -> success
  - Failed -> danger
- Check-in code:
  - ACTIVE -> info
  - USED -> success
  - EXPIRED -> neutral/danger

## 8) Mobile UX Rules
- Main CTA must be reachable by thumb.
- Tap target >= 44px.
- Avoid horizontal scrolling.
- Keep key booking actions in first viewport.
