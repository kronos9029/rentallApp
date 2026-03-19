import { expect, type Page } from '@playwright/test';
import type { UserCredentials } from '../helpers/test-data';

export class RegisterPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/Auth/Register');
    await this.expectLoaded();
  }

  async expectLoaded(): Promise<void> {
    await expect(this.page.getByRole('heading', { name: 'Tao tai khoan customer' })).toBeVisible();
  }

  async register(customer: Required<UserCredentials>): Promise<void> {
    await this.expectLoaded();
    await this.page.getByLabel('Ho va ten').fill(customer.fullName);
    await this.page.getByLabel('Email').fill(customer.email);
    await this.page.getByLabel('Password', { exact: true }).fill(customer.password);
    await this.page.getByLabel('Xac nhan password').fill(customer.password);
    await this.page.getByRole('button', { name: 'Tao tai khoan' }).click();
  }

  async expectDuplicateEmailError(): Promise<void> {
    await expect(this.page.getByText('Email da ton tai.')).toBeVisible();
  }
}
