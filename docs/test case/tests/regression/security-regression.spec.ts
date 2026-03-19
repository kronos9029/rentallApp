import { devices, expect, test } from '@playwright/test';
import { seedUsers } from '../helpers/test-data';
import { LoginPage } from '../page-objects/LoginPage';
import { AvailabilityPage } from '../page-objects/AvailabilityPage';

test.describe('Regression: security baseline and mobile behavior', () => {
  test('TC-SEC-01 home page returns security headers', async ({ request }) => {
    const response = await request.get(new URL('/', process.env.BASE_URL ?? 'https://localhost:7048').toString(), {
      failOnStatusCode: false,
    });

    expect(response.status()).toBe(200);
    expect(response.headers()['x-content-type-options']).toBeTruthy();
    expect(response.headers()['x-frame-options']).toBeTruthy();
    expect(response.headers()['content-security-policy']).toBeTruthy();
  });

  test('TC-SEC-02 auth cookie uses secure and httpOnly flags after login', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(seedUsers.customer);

    const cookies = await page.context().cookies();
    const authCookie = cookies.find((cookie) => cookie.name === '__Host-rentalapp-auth');

    expect(authCookie).toBeTruthy();
    expect(authCookie?.httpOnly).toBeTruthy();
    expect(authCookie?.secure).toBeTruthy();
    expect(authCookie?.sameSite).toBe('Lax');
  });

  test('TC-SEC-03 posting to profile without antiforgery token is rejected', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(seedUsers.customer);

    const response = await page.context().request.post(new URL('/Account/Profile', process.env.BASE_URL ?? 'https://localhost:7048').toString(), {
      failOnStatusCode: false,
      form: {
        'Input.FullName': 'Forged Request',
        'Input.Email': seedUsers.customer.email,
      },
    });

    expect(response.status()).toBe(400);
  });

  test('TC-MOB-01 availability page works on mobile viewport', async ({ browser }, testInfo) => {
    const context = await browser.newContext({
      ...devices['iPhone 13'],
      ignoreHTTPSErrors: true,
    });
    const page = await context.newPage();
    const availabilityPage = new AvailabilityPage(page);
    const bookingDate = new Date();
    bookingDate.setUTCDate(bookingDate.getUTCDate() + 20 + testInfo.retry);
    const bookingDateValue = bookingDate.toISOString().slice(0, 10);

    try {
      await availabilityPage.goto();
      await availabilityPage.search({
        bookingDate: bookingDateValue,
        bookingMode: 'Shared',
        slotQuantity: 2,
      });

      await availabilityPage.expectSharedRemainingSlots();
      await expect(page.getByRole('button', { name: 'Tai availability' })).toBeVisible();
    } finally {
      await context.close();
    }
  });
});
