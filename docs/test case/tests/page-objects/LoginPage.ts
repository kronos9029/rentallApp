import { expect, type Page } from '@playwright/test';
import type { UserCredentials } from '../helpers/test-data';

export class LoginPage {
  constructor(private readonly page: Page) {}

  async goto(search = ''): Promise<void> {
    await this.page.goto(`/Auth/Login${search}`);
    await this.expectLoaded();
  }

  async expectLoaded(): Promise<void> {
    await expect(this.page.getByRole('heading', { name: 'Dang nhap' })).toBeVisible();
  }

  async login(credentials: UserCredentials): Promise<void> {
    await this.expectLoaded();
    await this.page.getByLabel('Email').fill(credentials.email);
    await this.page.getByLabel('Password').fill(credentials.password);
    await this.page.getByRole('button', { name: 'Dang nhap' }).click();
  }

  async expectRedirectedFromProtectedRoute(): Promise<void> {
    await expect(this.page).toHaveURL(/\/Auth\/Login\?(?:ReturnUrl|returnUrl)=/);
  }

  async expectRegistrationSuccess(): Promise<void> {
    await expect(this.page.getByText('Tao tai khoan thanh cong. Ban co the dang nhap ngay bay gio.')).toBeVisible();
  }

  async expectInvalidCredentialsError(): Promise<void> {
    await expect(this.page.getByText('Thong tin dang nhap khong hop le.')).toBeVisible();
  }
}
