import { expect, type Page } from '@playwright/test';

export class AccessDeniedPage {
  constructor(private readonly page: Page) {}

  async expectLoaded(): Promise<void> {
    await expect(this.page).toHaveURL(/\/Auth\/AccessDenied/);
    await expect(this.page.getByRole('heading', { name: 'Khong co quyen truy cap' })).toBeVisible();
  }
}
