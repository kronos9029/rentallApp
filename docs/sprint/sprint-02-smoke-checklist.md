# Sprint 02 Smoke Checklist

## Scope
- `Availability -> Hold -> Checkout`
- `Forgot Password -> Reset Password via email`
- `Hold expiry worker + inventory release`

## Environment
- Web: `https://localhost:7048`
- Worker: `RentalApp.Worker`
- Database: native MySQL `rental_app`
- Auth account smoke:
  - `customer@local.test / Customer123!`
  - `admin@local.test / Admin123!`

## Checklist

| Flow | Steps | Expected result | Status |
|---|---|---|---|
| Login customer | Dang nhap bang `customer@local.test` | Dang nhap thanh cong, vao Home/Profile duoc | PASS |
| Availability private | Mo `/Booking/Availability` voi private booking | Hien courts + slots + pricing weekday/weekend dung | PASS |
| Create private hold | Chon `C01`, 1 bucket, submit `Tao hold` | Redirect sang `/Booking/Hold/{holdId}`, tong tien tam tinh dung | PASS |
| Checkout from hold | Tu Hold Summary bam `Tiep tuc checkout`, submit `Tao checkout` | Tao `checkout_order` pending, hold chuyen `Converted`, button bi disable khi replay | PASS |
| Availability shared | Mo `/Booking/Availability` voi shared booking | Hien remaining shared slots | PASS |
| Create shared hold | Chon `C01`, 1 bucket, `slotQuantity=2`, submit hold | Hold summary hien `Shared`, `slotQty=2`, tong tien dung | PASS |
| Hold expiry worker | Worker tick tren DB that voi hold het han | Hold bi mark `Expired`, inventory duoc tra lai | PASS |
| Forgot password email | Submit forgot password voi email hop le | UI hien success generic message, backend gui mail | PASS |
| Reset password | Mo reset link trong email, dat password moi, login lai | Reset thanh cong, login bang password moi duoc | PASS |

## Evidence
- Build: `dotnet build D:\rentalApp\RentalApp.slnx`
- Test: `dotnet test D:\rentalApp\RentalApp.slnx`
- Migration: `20260318081607_Sprint02Part2ExpiryCheckout`
- Web log: `D:\rentalApp\output\run\s2-web.log`
- Worker log: `D:\rentalApp\output\run\s2-worker.log`

## Notes
- Worker polling interval hien tai la `30s`.
- Checkout Part 2 dung o muc tao `booking` pending + `checkout_order` pending. Payment se noi tiep o Sprint 3.
