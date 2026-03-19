import { test, expect } from '@playwright/test';
import { bookingDateFor, seedUsers } from '../helpers/test-data';
import { canConnectToDb, execute, queryRows } from '../helpers/db';
import { AppShell } from '../page-objects/AppShell';
import { AvailabilityPage } from '../page-objects/AvailabilityPage';
import { CheckoutPage } from '../page-objects/CheckoutPage';
import { HoldPage } from '../page-objects/HoldPage';
import { LoginPage } from '../page-objects/LoginPage';

type CourtStateRow = {
  court_id: string;
  is_active: number;
  maintenance_reason: string | null;
};

test.describe('Regression: booking secondary and negative flows', () => {
  test('TC-BOOK-02 public visitor can load shared availability', async ({ page }, testInfo) => {
    const availabilityPage = new AvailabilityPage(page);

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 3),
      bookingMode: 'Shared',
      slotQuantity: 2
    });

    await availabilityPage.expectSharedRemainingSlots();
  });

  test('TC-BOOK-03 availability empty state is not reachable in seeded environment', async ({ page }, testInfo) => {
    test.skip(!(await canConnectToDb()), 'DB runtime is not reachable.');
    const availabilityPage = new AvailabilityPage(page);
    const bookingDate = bookingDateFor(testInfo, 11);
    const originalCourts = await queryRows<CourtStateRow>(
      'SELECT court_id, is_active, maintenance_reason FROM courts ORDER BY sort_order, court_id');

    try {
      await execute(
        'UPDATE courts SET is_active = 0, maintenance_reason = ?',
        ['playwright-empty-state-temp'],
      );

      await availabilityPage.goto();
      await availabilityPage.search({
        bookingDate,
        bookingMode: 'Private',
        slotQuantity: 1,
        expectResults: false,
      });
      await availabilityPage.expectEmptyState();
    } finally {
      for (const court of originalCourts) {
        await execute(
          'UPDATE courts SET is_active = ?, maintenance_reason = ? WHERE court_id = ?',
          [court.is_active, court.maintenance_reason, court.court_id],
        );
      }
    }
  });

  test('TC-BOOK-04 guest creating hold is redirected to login and returns to availability', async ({ page }, testInfo) => {
    const bookingDate = bookingDateFor(testInfo, 4);
    const availabilityPage = new AvailabilityPage(page);
    const loginPage = new LoginPage(page);

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate,
      bookingMode: 'Private',
      slotQuantity: 1
    });
    await availabilityPage.selectFirstAvailableCourtAndBucket();
    await availabilityPage.submitHold();

    await loginPage.expectRedirectedFromProtectedRoute();
    await loginPage.login(seedUsers.customer);

    await expect(page).toHaveURL(/\/Booking\/Availability\?/);
    await availabilityPage.expectLoaded();
    await availabilityPage.expectBookingDate(bookingDate);
  });

  test('TC-BOOK-05 hold submission without selecting court shows validation error', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const availabilityPage = new AvailabilityPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 6),
      bookingMode: 'Private',
      slotQuantity: 1
    });
    await availabilityPage.submitHold();

    await availabilityPage.expectSelectCourtError();
    await availabilityPage.expectSelectBucketError();
  });

  test('TC-BOOK-06 hold submission without selecting bucket shows validation error', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const availabilityPage = new AvailabilityPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 7),
      bookingMode: 'Private',
      slotQuantity: 1
    });
    await availabilityPage.selectOnlyFirstCourt();
    await availabilityPage.submitHold();

    await availabilityPage.expectSelectBucketError();
  });

  test('TC-BOOK-07 direct access to fake hold id returns 404', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    const response = await page.goto('/Booking/Hold/not-a-real-hold-id');
    expect(response?.status()).toBe(404);
  });

  test('TC-BOOK-08 direct access to fake checkout hold id returns 404', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    const response = await page.goto('/Checkout?holdId=not-a-real-hold-id');
    expect(response?.status()).toBe(404);
  });

  test('TC-BOOK-09 customer can create shared hold with slot quantity 2', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const availabilityPage = new AvailabilityPage(page);
    const holdPage = new HoldPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 5),
      bookingMode: 'Shared',
      slotQuantity: 2
    });
    await availabilityPage.expectSharedRemainingSlots();
    await availabilityPage.selectFirstAvailableCourtAndBucket();
    await availabilityPage.submitHold();

    await holdPage.expectLoaded();
    await holdPage.expectBookingMode('Shared');
    await holdPage.expectQuantity(2);
  });

  test('TC-BOOK-10 checkout remains disabled after page reload once order exists', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const availabilityPage = new AvailabilityPage(page);
    const holdPage = new HoldPage(page);
    const checkoutPage = new CheckoutPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 8),
      bookingMode: 'Private',
      slotQuantity: 1
    });
    await availabilityPage.selectFirstAvailableCourtAndBucket();
    await availabilityPage.submitHold();

    await holdPage.expectLoaded();
    await holdPage.continueToCheckout();

    await checkoutPage.expectLoaded();
    await checkoutPage.createCheckout();
    await checkoutPage.expectCheckoutCreated();

    await page.reload();
    await checkoutPage.expectLoaded();
    await checkoutPage.expectCheckoutLockedAfterCreation();
  });

  test('TC-BOOK-11 shared booking supports boundary slot quantities 1 and 8', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const availabilityPage = new AvailabilityPage(page);
    const holdPage = new HoldPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 9),
      bookingMode: 'Shared',
      slotQuantity: 1
    });
    await availabilityPage.selectFirstAvailableCourtAndBucket();
    await availabilityPage.submitHold();
    await holdPage.expectLoaded();
    await holdPage.expectBookingMode('Shared');
    await holdPage.expectQuantity(1);

    await availabilityPage.goto();
    await availabilityPage.search({
      bookingDate: bookingDateFor(testInfo, 10),
      bookingMode: 'Shared',
      slotQuantity: 8
    });
    await availabilityPage.selectFirstAvailableCourtAndBucket();
    await availabilityPage.submitHold();
    await holdPage.expectLoaded();
    await holdPage.expectBookingMode('Shared');
    await holdPage.expectQuantity(8);
  });
});
