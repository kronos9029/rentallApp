import { expect, type Page } from '@playwright/test';

export class HoldPage {
  constructor(private readonly page: Page) {}

  async expectLoaded(): Promise<void> {
    await expect(this.page).toHaveURL(/\/Booking\/Hold\//);
    await expect(this.page.getByRole('heading', { name: 'Hold Summary' })).toBeVisible();
  }

  async expectActiveStatus(): Promise<void> {
    await expect(this.page.locator('dl')).toContainText('Active');
  }

  async expectBookingMode(mode: 'Private' | 'Shared'): Promise<void> {
    await expect(this.page.locator('table')).toContainText(mode);
  }

  async expectQuantity(quantity: number): Promise<void> {
    await expect(this.page.locator('table')).toContainText(String(quantity));
  }

  async continueToCheckout(): Promise<void> {
    await this.page.getByRole('link', { name: 'Tiep tuc checkout' }).click();
  }
}
