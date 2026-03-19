import { expect, type Page } from '@playwright/test';

export class AppShell {
  constructor(private readonly page: Page) {}

  async expectSignedInAs(displayName: string): Promise<void> {
    await expect(this.page.getByText(`Xin chao ${displayName}`)).toBeVisible();
  }

  async expectAuthenticated(): Promise<void> {
    await expect(this.page.getByText(/^Xin chao /)).toBeVisible();
    await expect(this.page.getByRole('button', { name: 'Logout' })).toBeVisible();
  }

  async expectAdminLinkHidden(): Promise<void> {
    await expect(this.page.getByRole('banner').getByRole('link', { name: 'Admin', exact: true })).toHaveCount(0);
  }

  async expectAdminLinkVisible(): Promise<void> {
    await expect(this.page.getByRole('banner').getByRole('link', { name: 'Admin', exact: true })).toBeVisible();
  }

  async logout(): Promise<void> {
    await this.page.getByRole('button', { name: 'Logout' }).click();
    await expect(this.page.getByRole('link', { name: 'Login' })).toBeVisible();
  }
}
