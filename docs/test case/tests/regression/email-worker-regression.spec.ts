import { expect, test } from '@playwright/test';
import { canConnectToDb, execute, queryRows } from '../helpers/db';
import { apiUrl } from '../helpers/api';
import { buildNewCustomer, bookingDateFor } from '../helpers/test-data';
import { LoginPage } from '../page-objects/LoginPage';
import { RegisterPage } from '../page-objects/RegisterPage';
import { AvailabilityPage } from '../page-objects/AvailabilityPage';
import { AppShell } from '../page-objects/AppShell';
import { waitFor, runIfEnabled, startProcess, stopProcess, repoRoot, testOutputFile } from '../helpers/runtime';

type ResetTokenRow = {
  token_hash: string;
  user_id: string;
};

test.describe('Regression: email reset and worker expiry runtime', () => {
  test('TC-EMAIL-01 forgot password shows generic success message', async ({ page }) => {
    await page.goto('/Auth/ForgotPassword');
    await page.getByLabel('Email').fill('customer@local.test');
    await page.getByRole('button', { name: 'Gui yeu cau reset' }).click();

    await expect(
      page
        .getByText('Neu email ton tai, he thong da gui huong dan reset qua email.')
        .or(page.getByText('Email reset tam thoi chua duoc cau hinh day du.')),
    ).toBeVisible();
  });

  test('TC-EMAIL-02 reset password rejects invalid or expired link', async ({ page }) => {
    await page.goto('/Auth/ResetPassword?email=customer@local.test&token=invalid-token');
    await expect(page.getByText('Reset link khong hop le hoac da het han.')).toBeVisible();
  });

  test('TC-EMAIL-03 forgot/reset password full flow via MailHog API', async ({ page, request }) => {
    test.skip(!runIfEnabled('ENABLE_MAILHOG_TESTS'), 'Set ENABLE_MAILHOG_TESTS=true and configure web SMTP to MailHog.');
    test.setTimeout(90_000);

    const customer = buildNewCustomer();
    const registerPage = new RegisterPage(page);
    const loginPage = new LoginPage(page);
    await registerPage.goto();
    await registerPage.register(customer);

    await page.goto('/Auth/ForgotPassword');
    await page.getByLabel('Email').fill(customer.email);
    await page.getByRole('button', { name: 'Gui yeu cau reset' }).click();
    await expect(page.getByText('Neu email ton tai, he thong da gui huong dan reset qua email.')).toBeVisible();

    const mailhogApi = process.env.MAILHOG_API_URL ?? 'http://127.0.0.1:8025';
    const resetUrl = await waitFor(async () => {
      const response = await request.get(`${mailhogApi}/api/v2/messages`);
      const payload = await response.json();
      const message = payload.items?.find((item: { Raw?: { To?: string[]; Data?: string } }) =>
        item.Raw?.To?.some((to) => to.includes(customer.email)));
      if (!message) {
        return false;
      }

      const plainTextPart = message.MIME?.Parts?.find((part: { Headers?: Record<string, string[]>; Body?: string }) =>
        part.Headers?.['Content-Transfer-Encoding']?.includes('base64') &&
        part.Headers?.['Content-Type']?.some((value) => value.includes('text/plain')));

      const decodedBody = plainTextPart?.Body
        ? Buffer.from(plainTextPart.Body.replace(/\s+/g, ''), 'base64').toString('utf8')
        : message.Raw?.Data;
      if (!decodedBody) {
        return false;
      }

      const match = decodedBody.match(/https?:\/\/[^\s"<]+\/Auth\/ResetPassword[^\s"<]+/i);
      if (!match) {
        return false;
      }

      process.env.__PW_RESET_URL = match[0]
        .replace(/&amp;/gi, '&')
        .replace(/[<>]+$/g, '');
      return true;
    }, 20_000);

    expect(resetUrl).toBe(true);
    const url = process.env.__PW_RESET_URL;
    expect(url).toBeTruthy();

    await page.goto(url!);
    await page.locator('#Input_Password').fill('Customer456!');
    await page.locator('#Input_ConfirmPassword').fill('Customer456!');
    await page.getByRole('button', { name: 'Cap nhat mat khau' }).click();

    await expect(page).toHaveURL(/\/Auth\/Login/);
    await loginPage.login({
      email: customer.email,
      password: 'Customer456!',
    });
    await new AppShell(page).expectSignedInAs(customer.fullName);
  });

  test('TC-WORKER-01 expired hold is released by worker runtime', async ({ page }, testInfo) => {
    test.skip(!runIfEnabled('ENABLE_WORKER_RUNTIME_TESTS'), 'Set ENABLE_WORKER_RUNTIME_TESTS=true with reachable DB and worker prerequisites.');
    test.skip(!(await canConnectToDb()), 'DB runtime is not reachable.');

    const workerStdout = testOutputFile(testInfo, 'worker-stdout.log');
    const workerStderr = testOutputFile(testInfo, 'worker-stderr.log');
    const worker = startProcess(
      'dotnet',
      ['run', '--project', 'src/RentalApp.Worker/RentalApp.Worker.csproj', '--no-launch-profile'],
      {
        cwd: repoRoot(),
        stdoutFile: workerStdout,
        stderrFile: workerStderr,
        env: {
          ...process.env,
          DOTNET_ENVIRONMENT: 'Testing',
          Database__MySql__Host: process.env.TEST_DB_HOST ?? '127.0.0.1',
          Database__MySql__Port: process.env.TEST_DB_PORT ?? '3306',
          Database__MySql__Database: process.env.TEST_DB_NAME ?? 'rental_app',
          Database__MySql__User: process.env.TEST_DB_USER ?? 'root',
          Database__MySql__Password: process.env.TEST_DB_PASSWORD ?? '',
          Database__MySql__TreatTinyAsBoolean: 'true',
          Database__MySql__AllowPublicKeyRetrieval: 'true',
          Database__MySql__SslMode: 'None',
        },
      },
    );

    try {
      const loginPage = new LoginPage(page);
      await loginPage.goto();
      await loginPage.login({
        email: 'customer@local.test',
        password: 'Customer123!',
      });

      const availabilityPage = new AvailabilityPage(page);
      await availabilityPage.goto();
      await availabilityPage.search({
        bookingDate: bookingDateFor(testInfo, 18),
        bookingMode: 'Shared',
        slotQuantity: 2,
      });
      await availabilityPage.selectFirstAvailableCourtAndBucket();
      await availabilityPage.submitHold();

      const holdUrl = page.url();
      const holdId = holdUrl.split('/Booking/Hold/')[1];
      expect(holdId).toBeTruthy();

      await execute('UPDATE holds SET expires_at = UTC_TIMESTAMP() - INTERVAL 1 MINUTE WHERE hold_id = ?', [holdId]);

      const expired = await waitFor(async () => {
        const rows = await queryRows<{ status: string }>('SELECT status FROM holds WHERE hold_id = ?', [holdId]);
        return rows[0]?.status === 'Expired';
      }, 45_000, 2_000);

      expect(expired).toBe(true);
    } finally {
      await stopProcess(worker);
    }
  });
});
