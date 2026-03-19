import { test } from '@playwright/test';
import { bookingDateFor, seedUsers } from '../helpers/test-data';
import { AppShell } from '../page-objects/AppShell';
import { AvailabilityPage } from '../page-objects/AvailabilityPage';
import { CheckoutPage } from '../page-objects/CheckoutPage';
import { HoldPage } from '../page-objects/HoldPage';
import { LoginPage } from '../page-objects/LoginPage';

test.describe('Smoke: booking critical path', () => {
  test('TC-BOOK-01 customer can create private hold and checkout order pending', async ({ page }, testInfo) => {
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
      bookingDate: bookingDateFor(testInfo, 2),
      bookingMode: 'Private',
      slotQuantity: 1
    });
    await availabilityPage.selectFirstAvailableCourtAndBucket();
    await availabilityPage.submitHold();

    await holdPage.expectLoaded();
    await holdPage.expectActiveStatus();
    await holdPage.expectBookingMode('Private');
    await holdPage.continueToCheckout();

    await checkoutPage.expectLoaded();
    await checkoutPage.createCheckout();
    await checkoutPage.expectCheckoutCreated();
    await checkoutPage.expectCheckoutLockedAfterCreation();
  });
});
