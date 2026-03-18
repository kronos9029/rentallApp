param(
    [string]$DatabaseName = "rental_app",
    [string]$Server = "127.0.0.1",
    [int]$Port = 3306,
    [string]$User = "root",
    [string]$Password = ""
)

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path

function Resolve-MySqlCli {
    $candidates = @(
        "mysql",
        "C:\xampp\mysql\bin\mysql.exe"
    )

    foreach ($candidate in $candidates) {
        $command = Get-Command $candidate -ErrorAction SilentlyContinue
        if ($command) {
            return $command.Source
        }
    }

    throw "mysql client was not found. Install MySQL/MariaDB client or update Resolve-MySqlCli."
}

$mysqlCli = Resolve-MySqlCli
if ($DatabaseName -notmatch '^[A-Za-z0-9_]+$') {
    throw "DatabaseName '$DatabaseName' contains unsupported characters."
}

$createDatabaseSql = "CREATE DATABASE IF NOT EXISTS $DatabaseName CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
$mysqlArgs = @(
    "-h", $Server,
    "-P", $Port,
    "-u", $User
)

if ($Password.Length -gt 0) {
    $mysqlArgs += "--password=$Password"
}

& $mysqlCli @mysqlArgs -e $createDatabaseSql
if ($LASTEXITCODE -ne 0) {
    throw "Failed to create database '$DatabaseName' on native MySQL."
}

Push-Location $projectRoot
try {
    dotnet tool restore
    dotnet dotnet-ef database update `
        --project "src/RentalApp.Infrastructure/RentalApp.Infrastructure.csproj" `
        --startup-project "src/RentalApp.Web/RentalApp.Web.csproj"
}
finally {
    Pop-Location
}
