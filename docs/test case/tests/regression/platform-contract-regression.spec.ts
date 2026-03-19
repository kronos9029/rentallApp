import fs from 'node:fs';
import path from 'node:path';
import { expect, test } from '@playwright/test';
import { repoRoot } from '../helpers/runtime';

function read(relativePath: string): string {
  return fs.readFileSync(path.join(repoRoot(), relativePath), 'utf8');
}

test.describe('Regression: explicit platform contracts for bootstrap, retry, and SMTP', () => {
  test('TC-BOOT-01 database bootstrap helper script creates DB and runs EF migration update', async () => {
    const script = read('build/scripts/Initialize-Database.ps1');

    expect(script).toContain('CREATE DATABASE IF NOT EXISTS');
    expect(script).toContain('dotnet tool restore');
    expect(script).toContain('dotnet dotnet-ef database update');
    expect(script).toContain('Resolve-MySqlCli');
  });

  test('TC-BOOT-02 design-time DbContext factory loads appsettings and decrypts env-based secrets', async () => {
    const factory = read('src/RentalApp.Infrastructure/Persistence/DesignTimeDbContextFactory.cs');

    expect(factory).toContain('AddJsonFile("src/RentalApp.Web/appsettings.json"');
    expect(factory).toContain('AddJsonFile($"src/RentalApp.Web/appsettings.{environmentName}.json"');
    expect(factory).toContain('configuration.DecryptMarkedValuesFromEnvironment()');
    expect(factory).toContain('ServerVersion.AutoDetect(connectionString)');
  });

  test('TC-MIG-01 sprint 1 and sprint 2 migration artifacts exist explicitly', async () => {
    const root = repoRoot();
    const expectedFiles = [
      'src/RentalApp.Infrastructure/Persistence/Migrations/20260318043353_Sprint01Foundation.cs',
      'src/RentalApp.Infrastructure/Persistence/Migrations/20260318055631_DbBackedAuthSeed.cs',
      'src/RentalApp.Infrastructure/Persistence/Migrations/20260318063342_Sprint02Part1AvailabilityHold.cs',
      'src/RentalApp.Infrastructure/Persistence/Migrations/20260318081607_Sprint02Part2ExpiryCheckout.cs',
    ];

    for (const relativePath of expectedFiles) {
      expect(fs.existsSync(path.join(root, relativePath))).toBeTruthy();
    }
  });

  test('TC-RETRY-01 deadlock retry executor limits attempts to three', async () => {
    const retryExecutor = read('src/RentalApp.Infrastructure/Booking/DeadlockRetryExecutor.cs');

    expect(retryExecutor).toContain('const int maxAttempts = 3;');
    expect(retryExecutor).toContain('attempt < maxAttempts');
  });

  test('TC-RETRY-02 deadlock retry executor targets MySQL 1205/1213 with jittered delay', async () => {
    const retryExecutor = read('src/RentalApp.Infrastructure/Booking/DeadlockRetryExecutor.cs');

    expect(retryExecutor).toContain('MySqlException { Number: 1205 or 1213 }');
    expect(retryExecutor).toContain('DbUpdateException { InnerException: MySqlException { Number: 1205 or 1213 } }');
    expect(retryExecutor).toContain('Random.Shared.Next(40, 120) * attempt');
  });

  test('TC-SMTP-01 base appsettings exposes full SMTP config contract', async () => {
    const appsettings = read('src/RentalApp.Web/appsettings.json');

    expect(appsettings).toContain('"Email"');
    expect(appsettings).toContain('"Smtp"');
    expect(appsettings).toContain('"Host"');
    expect(appsettings).toContain('"Port"');
    expect(appsettings).toContain('"EnableSsl"');
    expect(appsettings).toContain('"Username"');
    expect(appsettings).toContain('"Password"');
    expect(appsettings).toContain('"FromAddress"');
    expect(appsettings).toContain('"FromDisplayName"');
  });

  test('TC-SMTP-02 development SMTP secrets are encrypted in environment-specific config', async () => {
    const developmentConfig = read('src/RentalApp.Web/appsettings.Development.json');

    expect(developmentConfig).toContain('"Username": "enc::');
    expect(developmentConfig).toContain('"Password": "enc::');
    expect(developmentConfig).toContain('"FromAddress": "enc::');
  });

  test('TC-SMTP-03 password reset email template contains subject, expiry, CTA, and fallback URL', async () => {
    const template = read('src/RentalApp.Infrastructure/Communication/Email/PasswordResetEmailTemplate.cs');

    expect(template).toContain('RentalApp - Password reset');
    expect(template).toContain('Reset link co hieu luc den:');
    expect(template).toContain('Dat lai mat khau');
    expect(template).toContain('Neu nut khong mo duoc, hay copy link sau vao trinh duyet:');
  });

  test('TC-SMTP-04 local SMTP delivery and verification strategy is documented', async () => {
    const readme = read('README.md');
    const sprintTasks = read('docs/sprint/sprint-02-tasks.md');

    expect(readme).toContain('Email:Smtp:Username');
    expect(readme).toContain('Email:Smtp:Password');
    expect(readme).toContain('Email:Smtp:FromAddress');
    expect(sprintTasks).toContain('Local smoke cho forgot-password da gui mail thanh cong qua Gmail SMTP voi app password');
    expect(sprintTasks).toContain('SMTP contract + forgot-password email dispatch da duoc implement');
  });
});
