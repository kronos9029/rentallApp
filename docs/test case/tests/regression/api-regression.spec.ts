import { expect, test } from '@playwright/test';
import { apiUrl, createAuthenticatedApiContext } from '../helpers/api';
import { bookingDateFor, seedUsers } from '../helpers/test-data';
import { LoginPage } from '../page-objects/LoginPage';

test.describe('Regression: API contract and guardrail checks', () => {
  test('TC-API-01 public availability API returns success payload', async ({ request }, testInfo) => {
    const bookingDate = bookingDateFor(testInfo, 11);

    const response = await request.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Shared&slotQty=2`), {
      failOnStatusCode: false
    });

    expect(response.status()).toBe(200);

    const payload = await response.json();
    expect(payload.success).toBe(true);
    expect(Array.isArray(payload.errors)).toBe(true);
    expect(payload.data.bookingDate).toBe(bookingDate);
    expect(Array.isArray(payload.data.courts)).toBe(true);
    expect(payload.data.courts.length).toBeGreaterThan(0);
  });

  test('TC-API-02 public availability API rejects private slot quantity > 1', async ({ request }, testInfo) => {
    const bookingDate = bookingDateFor(testInfo, 12);

    const response = await request.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Private&slotQty=2`), {
      failOnStatusCode: false
    });

    expect(response.status()).toBe(400);

    const payload = await response.json();
    expect(payload.success).toBe(false);
    expect(payload.errors[0]).toContain('Private booking currently supports exactly one court slot per bucket.');
  });

  test('TC-API-03 api holds rejects missing Idempotency-Key for authenticated user', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(seedUsers.customer);

    const authRequest = createAuthenticatedApiContext(page);
    const bookingDate = bookingDateFor(testInfo, 13);
    const availabilityResponse = await authRequest.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Private&slotQty=1`));
    const availabilityPayload = await availabilityResponse.json();
    const firstCourt = availabilityPayload.data.courts[0];
    const firstSlot = firstCourt.slots.find((slot: { isAvailable: boolean }) => slot.isAvailable);

    const response = await authRequest.post(apiUrl('/api/holds'), {
      failOnStatusCode: false,
      data: {
        courtId: firstCourt.courtId,
        bucketIds: [firstSlot.bucketId],
        bookingMode: 1,
        slotQuantity: 1
      }
    });

    expect(response.status()).toBe(400);
    const payload = await response.json();
    expect(payload.success).toBe(false);
    expect(payload.errors[0]).toContain('Idempotency-Key header is required.');
  });

  test('TC-API-04 api checkout rejects missing Idempotency-Key for authenticated user', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(seedUsers.customer);

    const authRequest = createAuthenticatedApiContext(page);
    const response = await authRequest.post(apiUrl('/api/checkout'), {
      failOnStatusCode: false,
      data: {
        holdId: 'not-a-real-hold-id'
      }
    });

    expect(response.status()).toBe(400);
    const payload = await response.json();
    expect(payload.success).toBe(false);
    expect(payload.errors[0]).toContain('Idempotency-Key header is required.');
  });
});
