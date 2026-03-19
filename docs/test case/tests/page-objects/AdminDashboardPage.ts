import { expect, type Page } from '@playwright/test';

export class AdminDashboardPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/Admin');
  }

  async expectLoaded(): Promise<void> {
    await expect(this.page.getByRole('heading', { name: 'Admin Dashboard' })).toBeVisible();
    await expect(this.page.getByText('Admin only')).toBeVisible();
  }
}
