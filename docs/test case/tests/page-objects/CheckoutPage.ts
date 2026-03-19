import { expect, type Page } from '@playwright/test';

export class CheckoutPage {
  constructor(private readonly page: Page) {}

  async expectLoaded(): Promise<void> {
    await expect(this.page).toHaveURL(/\/Checkout/);
    await expect(this.page.getByRole('heading', { name: 'Checkout' })).toBeVisible();
  }

  async createCheckout(): Promise<void> {
    await expect(this.page.getByRole('button', { name: 'Tao checkout' })).toBeEnabled();
    await this.page.getByRole('button', { name: 'Tao checkout' }).click();
  }

  async expectCheckoutCreated(): Promise<void> {
    await expect(this.page.getByText('Da tao checkout thanh cong.')).toBeVisible();
  }

  async expectCheckoutLockedAfterCreation(): Promise<void> {
    await expect(this.page.getByRole('button', { name: 'Tao checkout' })).toBeDisabled();
    await expect(this.page.getByText('Checkout nay da duoc tao truoc do.')).toBeVisible();
  }
}
