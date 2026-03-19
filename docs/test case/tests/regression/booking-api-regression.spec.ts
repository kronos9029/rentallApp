import { expect, test } from '@playwright/test';
import { apiUrl, createAuthenticatedApiContext } from '../helpers/api';
import { bookingDateFor, seedUsers } from '../helpers/test-data';
import { LoginPage } from '../page-objects/LoginPage';

test.describe('Regression: booking API idempotency and concurrency baseline', () => {
  test('TC-BOOK-API-01 hold API replays same idempotency key without duplicating hold', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(seedUsers.customer);

    const request = createAuthenticatedApiContext(page);
    const bookingDate = bookingDateFor(testInfo, 14);
    const availabilityResponse = await request.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Private&slotQty=1`));
    const availabilityPayload = await availabilityResponse.json();
    const firstCourt = availabilityPayload.data.courts[0];
    const firstSlot = firstCourt.slots.find((slot: { isAvailable: boolean }) => slot.isAvailable);
    const idempotencyKey = `pw-hold-replay-${Date.now()}`;

    const firstResponse = await request.post(apiUrl('/api/holds'), {
      headers: { 'Idempotency-Key': idempotencyKey },
      data: {
        courtId: firstCourt.courtId,
        bucketIds: [firstSlot.bucketId],
        bookingMode: 1,
        slotQuantity: 1,
      },
    });
    const secondResponse = await request.post(apiUrl('/api/holds'), {
      headers: { 'Idempotency-Key': idempotencyKey },
      data: {
        courtId: firstCourt.courtId,
        bucketIds: [firstSlot.bucketId],
        bookingMode: 1,
        slotQuantity: 1,
      },
    });

    expect(firstResponse.status()).toBe(201);
    expect(secondResponse.status()).toBe(201);

    const firstPayload = await firstResponse.json();
    const secondPayload = await secondResponse.json();
    expect(firstPayload.data.holdId).toBe(secondPayload.data.holdId);
    expect(secondPayload.replayed).toBe(true);
  });

  test('TC-BOOK-API-02 checkout API replays same idempotency key without duplicate order', async ({ page }, testInfo) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(seedUsers.customer);

    const request = createAuthenticatedApiContext(page);
    const bookingDate = bookingDateFor(testInfo, 15);
    const availabilityResponse = await request.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Private&slotQty=1`));
    const availabilityPayload = await availabilityResponse.json();
    const firstCourt = availabilityPayload.data.courts[0];
    const firstSlot = firstCourt.slots.find((slot: { isAvailable: boolean }) => slot.isAvailable);
    const holdResponse = await request.post(apiUrl('/api/holds'), {
      headers: { 'Idempotency-Key': `pw-checkout-hold-${Date.now()}` },
      data: {
        courtId: firstCourt.courtId,
        bucketIds: [firstSlot.bucketId],
        bookingMode: 1,
        slotQuantity: 1,
      },
    });
    const holdPayload = await holdResponse.json();
    const checkoutIdempotencyKey = `pw-checkout-replay-${Date.now()}`;

    const firstResponse = await request.post(apiUrl('/api/checkout'), {
      headers: { 'Idempotency-Key': checkoutIdempotencyKey },
      data: {
        holdId: holdPayload.data.holdId,
      },
    });
    const secondResponse = await request.post(apiUrl('/api/checkout'), {
      headers: { 'Idempotency-Key': checkoutIdempotencyKey },
      data: {
        holdId: holdPayload.data.holdId,
      },
    });

    expect(firstResponse.status()).toBe(201);
    expect(secondResponse.status()).toBe(201);

    const firstPayload = await firstResponse.json();
    const secondPayload = await secondResponse.json();
    expect(firstPayload.data.orderId).toBe(secondPayload.data.orderId);
    expect(secondPayload.replayed).toBe(true);
  });

  test('TC-BOOK-API-03 concurrent private hold requests do not both succeed', async ({ browser, page }, testInfo) => {
    const bookingDate = bookingDateFor(testInfo, 16);
    const contextOne = await browser.newContext({ ignoreHTTPSErrors: true });
    const contextTwo = await browser.newContext({ ignoreHTTPSErrors: true });
    const pageOne = await contextOne.newPage();
    const pageTwo = await contextTwo.newPage();

    const loginPageOne = new LoginPage(pageOne);
    await loginPageOne.goto();
    await loginPageOne.login(seedUsers.customer);

    const loginPageTwo = new LoginPage(pageTwo);
    await loginPageTwo.goto();
    await loginPageTwo.login(seedUsers.admin);

    const requestOne = createAuthenticatedApiContext(pageOne);
    const requestTwo = createAuthenticatedApiContext(pageTwo);
    const availabilityResponse = await requestOne.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Private&slotQty=1`));
    const availabilityPayload = await availabilityResponse.json();
    const firstCourt = availabilityPayload.data.courts[0];
    const firstSlot = firstCourt.slots.find((slot: { isAvailable: boolean }) => slot.isAvailable);

    const [first, second] = await Promise.all([
      requestOne.post(apiUrl('/api/holds'), {
        failOnStatusCode: false,
        headers: { 'Idempotency-Key': `pw-concurrent-customer-${Date.now()}` },
        data: {
          courtId: firstCourt.courtId,
          bucketIds: [firstSlot.bucketId],
          bookingMode: 1,
          slotQuantity: 1,
        },
      }),
      requestTwo.post(apiUrl('/api/holds'), {
        failOnStatusCode: false,
        headers: { 'Idempotency-Key': `pw-concurrent-admin-${Date.now()}` },
        data: {
          courtId: firstCourt.courtId,
          bucketIds: [firstSlot.bucketId],
          bookingMode: 1,
          slotQuantity: 1,
        },
      }),
    ]);

    const statuses = [first.status(), second.status()].sort();
    expect(statuses).toEqual([201, 409]);

    await contextOne.close();
    await contextTwo.close();
  });

  test('TC-BOOK-API-04 shared capacity over 8 is rejected under concurrent load', async ({ browser, page }, testInfo) => {
    const bookingDate = bookingDateFor(testInfo, 17);
    const contextOne = await browser.newContext({ ignoreHTTPSErrors: true });
    const contextTwo = await browser.newContext({ ignoreHTTPSErrors: true });
    const pageOne = await contextOne.newPage();
    const pageTwo = await contextTwo.newPage();

    const loginPageOne = new LoginPage(pageOne);
    await loginPageOne.goto();
    await loginPageOne.login(seedUsers.customer);

    const loginPageTwo = new LoginPage(pageTwo);
    await loginPageTwo.goto();
    await loginPageTwo.login(seedUsers.admin);

    const requestOne = createAuthenticatedApiContext(pageOne);
    const requestTwo = createAuthenticatedApiContext(pageTwo);
    const availabilityResponse = await requestOne.get(apiUrl(`/api/availability?date=${bookingDate}&bookingMode=Shared&slotQty=8`));
    const availabilityPayload = await availabilityResponse.json();
    const firstCourt = availabilityPayload.data.courts[0];
    const firstSlot = firstCourt.slots.find((slot: { isAvailable: boolean }) => slot.isAvailable);

    const first = await requestOne.post(apiUrl('/api/holds'), {
      headers: { 'Idempotency-Key': `pw-shared-eight-${Date.now()}` },
      data: {
        courtId: firstCourt.courtId,
        bucketIds: [firstSlot.bucketId],
        bookingMode: 2,
        slotQuantity: 8,
      },
    });

    const second = await requestTwo.post(apiUrl('/api/holds'), {
      failOnStatusCode: false,
      headers: { 'Idempotency-Key': `pw-shared-over-${Date.now()}` },
      data: {
        courtId: firstCourt.courtId,
        bucketIds: [firstSlot.bucketId],
        bookingMode: 2,
        slotQuantity: 1,
      },
    });

    expect(first.status()).toBe(201);
    expect(second.status()).toBe(409);

    await contextOne.close();
    await contextTwo.close();
  });
});
