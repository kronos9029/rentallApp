# RentalApp

Huong dan setup va startup project tren may moi sau khi vua `git clone` / `pull`.

## 1. Tong quan local dev

Trang thai hien tai cua repo:

- `Backend/UI`: ASP.NET Core Razor Pages tren `.NET 10`
- `Database local`: MySQL / MariaDB native tren may, mac dinh `127.0.0.1:3306`
- `HTTPS local`: `https://localhost:7048`
- `HTTP local`: `http://localhost:5054`
- `Secrets`: giu trong `appsettings.{Environment}.json`, chi ma hoa cac value nhay cam bang marker `enc::...`

## 2. Prerequisites

Can co tren may:

- `git`
- `.NET SDK 10.0.201`
- `MySQL` hoac `MariaDB` native dang chay tren may
- `mysql` client command line
- `Node.js`/`npm` la tuy chon, chi can khi debug UI bang Playwright
- `OpenSSL` la tuy chon, chi can neu muon tao AES key bang lenh mau ben duoi

Kiem tra nhanh trong `cmd`:

```bat
git --version
dotnet --version
mysql --version
```

Version `.NET` duoc pin o [global.json](D:/rentalApp/global.json).

## 3. Clone repo va restore

```bat
git clone <repo-url>
cd rentalApp
dotnet tool restore
dotnet restore RentalApp.slnx
```

## 4. Trust HTTPS dev certificate

Chi can lam 1 lan tren moi may:

```bat
dotnet dev-certs https --trust
```

## 5. Chuan bi MySQL local

Project hien dang dung section `Database:MySql` trong [appsettings.json](D:/rentalApp/src/RentalApp.Web/appsettings.json):

```json
"Database": {
  "MySql": {
    "Host": "127.0.0.1",
    "Port": 3306,
    "Database": "",
    "User": "",
    "Password": "",
    "TreatTinyAsBoolean": true,
    "AllowPublicKeyRetrieval": true,
    "SslMode": "None"
  }
}
```

Neu may ban khong dung:

- host `127.0.0.1`
- port `3306`
- user `root`
- password rong

thi sua:

- [appsettings.json](D:/rentalApp/src/RentalApp.Web/appsettings.json)
- [appsettings.json](D:/rentalApp/src/RentalApp.Worker/appsettings.json)

## 6. Tao database va apply migrations

### Cach khuyen dung

Neu `mysql` client da co san:

```bat
mysql -h 127.0.0.1 -P 3306 -u root -e "CREATE DATABASE IF NOT EXISTS rental_app CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
dotnet tool restore
dotnet dotnet-ef database update --project src\RentalApp.Infrastructure\RentalApp.Infrastructure.csproj --startup-project src\RentalApp.Web\RentalApp.Web.csproj
```

### Cach dung helper script co san

Repo dang co helper PowerShell o [Initialize-Database.ps1](D:/rentalApp/build/scripts/Initialize-Database.ps1). Neu ban chap nhan dung PowerShell cho rieng buoc nay:

```bat
powershell -NoProfile -ExecutionPolicy Bypass -File build\scripts\Initialize-Database.ps1
```

Schema da implement hien tai duoc mo ta o [database-schema-implemented.sql](D:/rentalApp/docs/database-schema-implemented.sql).

## 7. Cau hinh secrets theo moi truong

Project hien tai dung 3 moi truong:

- `appsettings.Development.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`

Team convention hien tai:

- `Development`: dung `shared dev key` chung cho ca team
- `Staging`: dung key rieng cua moi truong staging
- `Production`: dung key rieng cua moi truong production

Nguyen tac:

- value thuong de plain text
- chi cac value nhay cam moi ma hoa
- value ma hoa duoc luu ngay trong `appsettings.{Environment}.json`
- app tu dong decrypt cac value bat dau bang `enc::`
- voi DB config, chi encrypt:
  - `Database:MySql:Database`
  - `Database:MySql:User`
  - `Database:MySql:Password`
- voi email config, encrypt:
  - `Email:Smtp:Username`
  - `Email:Smtp:Password`
  - `Email:Smtp:FromAddress`

### 7.1. Tao AES key

Neu may co `openssl`:

```bat
openssl rand -base64 32
```

Vi du:

```text
Kv6BVpY35nwtzRjRZQ6B/xN74Nm4YiRPUyvuGXguzos=
```

Luu y:

- voi `Development`, neu trong repo da co san cac value `enc::...` thi member moi **khong tu tao key moi**
- ho phai dung **dung shared dev key cua team**
- neu tu tao key khac, app se khong decrypt duoc cac secret da commit san

### 7.2. Set AES key cho may local

```bat
setx RENTALAPP__AES_KEY "YOUR_BASE64_AES_KEY"
```

AES key se duoc luu trong `user environment variable` cua Windows:

- `HKCU\Environment`
- ten bien: `RENTALAPP__AES_KEY`

Kiem tra nhanh:

```bat
echo %RENTALAPP__AES_KEY%
reg query HKCU\Environment /v RENTALAPP__AES_KEY
```

Sau do dong terminal cu, mo terminal moi.

## 7.2A. Cach onboard cho member moi

Khi member khac pull code ve:

1. Lay `shared dev key` tu kenh chia se noi bo an toan cua team
2. Set vao may local:

```bat
setx RENTALAPP__AES_KEY "TEAM_SHARED_DEV_KEY"
```

3. Dong terminal cu, mo terminal moi
4. Chay:

```bat
dotnet build RentalApp.slnx
dotnet test RentalApp.slnx
dotnet run --project src\RentalApp.Web --launch-profile https
```

Khong can re-encrypt lai cac value trong `appsettings.Development.json` neu da dung dung `shared dev key`.

### 7.3. Encrypt tung secret value

Repo da co helper `cmd` o [Protect-Secrets.cmd](D:/rentalApp/build/scripts/Protect-Secrets.cmd).

Vi du ma hoa Gmail App Password:

```bat
build\scripts\Protect-Secrets.cmd --value "your-gmail-app-password" --key-base64 "YOUR_BASE64_AES_KEY"
```

Output se co dang:

```text
enc::BASE64_PAYLOAD_HERE
```

### 7.4. Paste encrypted value vao file moi truong

Vi du trong [appsettings.Development.json](D:/rentalApp/src/RentalApp.Web/appsettings.Development.json):

```json
{
  "Email": {
    "Smtp": {
      "Username": "enc::BASE64_ENCRYPTED_SMTP_USERNAME",
      "Password": "enc::BASE64_ENCRYPTED_SMTP_PASSWORD",
      "FromAddress": "enc::BASE64_ENCRYPTED_FROM_ADDRESS",
      "FromDisplayName": "RentalApp Development"
    }
  }
}
```

Vi du cho `Staging` hoac `Production`:

```json
{
  "Database": {
    "MySql": {
      "Host": "staging-db-host",
      "Port": 3306,
      "Database": "enc::BASE64_ENCRYPTED_DATABASE_NAME",
      "User": "enc::BASE64_ENCRYPTED_DB_USER",
      "Password": "enc::BASE64_ENCRYPTED_DB_PASSWORD",
      "TreatTinyAsBoolean": true,
      "AllowPublicKeyRetrieval": true,
      "SslMode": "Preferred"
    }
  },
  "Email": {
    "Smtp": {
      "Username": "enc::BASE64_ENCRYPTED_SMTP_USERNAME",
      "Password": "enc::BASE64_ENCRYPTED_SMTP_PASSWORD",
      "FromAddress": "enc::BASE64_ENCRYPTED_FROM_ADDRESS",
      "FromDisplayName": "RentalApp Staging"
    }
  }
}
```

Ban co the ap dung cach nay cho:

- `Database:MySql:Database`
- `Database:MySql:User`
- `Database:MySql:Password`
- `Email:Smtp:Username`
- `Email:Smtp:Password`
- `Email:Smtp:FromAddress`
- API keys
- webhook secrets

Luu y:

- voi `Development`, chi re-encrypt lai khi team chu dong rotate `shared dev key`
- `Password` cua Gmail phai la `Gmail App Password`
- `Database:MySql:Host`, `Database:MySql:Port`, `Database:MySql:SslMode` de plain text
- `Email:Smtp:Host`, `Email:Smtp:Port`, `Email:Smtp:EnableSsl` de plain text
- `Email:Smtp:FromDisplayName` co the de plain text
- file `appsettings.Staging.json` va `appsettings.Production.json` da duoc tao san, ban chi can dien gia tri vao

## 8. Build va test

```bat
dotnet build RentalApp.slnx
dotnet test RentalApp.slnx
```

## 9. Chay web app

```bat
dotnet run --project src\RentalApp.Web --launch-profile https
```

App se len:

- [https://localhost:7048](https://localhost:7048)
- [http://localhost:5054](http://localhost:5054)

Health check:

- [https://localhost:7048/healthz](https://localhost:7048/healthz)
- [https://localhost:7048/readyz](https://localhost:7048/readyz)

## 10. Tai khoan seed local

Sau khi apply migration, local DB se co san:

- `admin@local.test` / `Admin123!`
- `customer@local.test` / `Customer123!`

## 11. Smoke check nhanh

### Auth

1. Mo [https://localhost:7048/Auth/Login](https://localhost:7048/Auth/Login)
2. Dang nhap bang `admin@local.test`
3. Mo [https://localhost:7048/Admin](https://localhost:7048/Admin)
4. Thu `Forgot Password`

### Booking

1. Mo [https://localhost:7048/Booking/Availability?bookingDate=2026-03-20&bookingMode=Shared&slotQuantity=3](https://localhost:7048/Booking/Availability?bookingDate=2026-03-20&bookingMode=Shared&slotQuantity=3)
2. Chon court + bucket
3. Tao hold
4. Kiem tra Hold Summary

## 12. Worker

Worker host da co san nhung Sprint 2 Part 2 dang tiep tuc hoan thien.

Chay worker:

```bat
dotnet run --project src\RentalApp.Worker
```

Neu worker can doc encrypted config giong web app, no se dung cung AES key nay:

- `RENTALAPP__AES_KEY`
- `RENTALAPP__ENCRYPTED_CONFIG_PATH`

## 13. Troubleshooting

### `dotnet build` bi lock file DLL

Thu dong web app/worker dang chay roi build lai:

```bat
taskkill /IM RentalApp.Web.exe /F
taskkill /IM dotnet.exe /F
```

Can than khi dung `taskkill /IM dotnet.exe /F` neu may dang chay process `.NET` khac.

### Khong vao duoc HTTPS local

Chay lai:

```bat
dotnet dev-certs https --trust
```

### App bao thieu encrypted config key

Kiem tra:

```bat
echo %RENTALAPP__AES_KEY%
```

Neu vua moi `setx`, hay mo terminal moi roi chay lai app.

### Forgot password khong gui duoc email

Kiem tra:

- value secret trong `appsettings.{Environment}.json` da co dang `enc::...` chua
- `RENTALAPP__AES_KEY` dung khong
- Gmail dang dung `App Password` hay chi la mat khau thuong

## 14. File quan trong

- [RentalApp.slnx](D:/rentalApp/RentalApp.slnx)
- [appsettings.json](D:/rentalApp/src/RentalApp.Web/appsettings.json)
- [Program.cs](D:/rentalApp/src/RentalApp.Web/Program.cs)
- [EncryptedConfigurationExtensions.cs](D:/rentalApp/src/RentalApp.Infrastructure/Security/Secrets/EncryptedConfigurationExtensions.cs)
- [Initialize-Database.ps1](D:/rentalApp/build/scripts/Initialize-Database.ps1)
- [Protect-Secrets.cmd](D:/rentalApp/build/scripts/Protect-Secrets.cmd)
- [appsettings.Development.json](D:/rentalApp/src/RentalApp.Web/appsettings.Development.json)
- [appsettings.Staging.json](D:/rentalApp/src/RentalApp.Web/appsettings.Staging.json)
- [appsettings.Production.json](D:/rentalApp/src/RentalApp.Web/appsettings.Production.json)
- [database-schema-implemented.sql](D:/rentalApp/docs/database-schema-implemented.sql)
- [sprint-02-tasks.md](D:/rentalApp/docs/sprint/sprint-02-tasks.md)
