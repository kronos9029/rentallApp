import { expect, type Page } from '@playwright/test';

export class ProfilePage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/Account/Profile');
  }

  async expectLoaded(): Promise<void> {
    await expect(this.page.getByRole('heading', { name: 'Profile' })).toBeVisible();
  }

  async updateFullName(fullName: string): Promise<void> {
    await this.page.getByLabel('Ho va ten').fill(fullName);
    await this.page.getByRole('button', { name: 'Luu thay doi' }).click();
  }

  async expectUpdateSuccess(): Promise<void> {
    await expect(this.page.getByText('Cap nhat profile thanh cong.')).toBeVisible();
  }

  async expectFullName(fullName: string): Promise<void> {
    await expect(this.page.getByLabel('Ho va ten')).toHaveValue(fullName);
  }

  async expectEmailReadonly(): Promise<void> {
    await expect(this.page.getByLabel('Email')).toHaveAttribute('readonly', '');
  }

  async clearFullName(): Promise<void> {
    await this.page.getByLabel('Ho va ten').fill('');
  }

  async fillOverlongFullName(length = 101): Promise<string> {
    const value = 'A'.repeat(length);
    await this.page.getByLabel('Ho va ten').evaluate((element, nextValue) => {
      const input = element as HTMLInputElement;
      input.value = nextValue;
      input.dispatchEvent(new Event('input', { bubbles: true }));
      input.dispatchEvent(new Event('change', { bubbles: true }));
    }, value);
    return value;
  }

  async submit(): Promise<void> {
    await this.page.getByRole('button', { name: 'Luu thay doi' }).click();
  }

  async expectValidationError(): Promise<void> {
    await expect(this.page.locator('.text-danger').filter({ hasText: /required|must be a string|string|Ho va ten/i }).first()).toBeVisible();
  }
}
