import { test, expect } from '@playwright/test';
import { buildNewCustomer, seedUsers } from '../helpers/test-data';
import { AppShell } from '../page-objects/AppShell';
import { AdminDashboardPage } from '../page-objects/AdminDashboardPage';
import { LoginPage } from '../page-objects/LoginPage';
import { ProfilePage } from '../page-objects/ProfilePage';
import { RegisterPage } from '../page-objects/RegisterPage';

test.describe('Smoke: auth critical path', () => {
  test('TC-AUTH-01 guest is redirected to login when opening profile', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const profilePage = new ProfilePage(page);

    await profilePage.goto();
    await loginPage.expectRedirectedFromProtectedRoute();
    await loginPage.expectLoaded();
  });

  test('TC-AUTH-02 customer can register, login, update profile, and logout', async ({ page }) => {
    const customer = buildNewCustomer();
    const updatedFullName = `${customer.fullName} Updated`;
    const registerPage = new RegisterPage(page);
    const loginPage = new LoginPage(page);
    const profilePage = new ProfilePage(page);
    const appShell = new AppShell(page);

    await test.step('Register a new customer account', async () => {
      await registerPage.goto();
      await registerPage.register(customer);
      await expect(page).toHaveURL(/\/Auth\/Login$/);
      await loginPage.expectRegistrationSuccess();
    });

    await test.step('Login with the new account', async () => {
      await loginPage.login(customer);
      await appShell.expectSignedInAs(customer.fullName);
      await expect(page).toHaveURL(/\/$/);
    });

    await test.step('Update profile successfully', async () => {
      await profilePage.goto();
      await profilePage.expectLoaded();
      await profilePage.updateFullName(updatedFullName);
      await profilePage.expectUpdateSuccess();
      await profilePage.expectFullName(updatedFullName);
    });

    await test.step('Logout to anonymous state', async () => {
      await appShell.logout();
      await expect(page).toHaveURL(/\/$/);
    });
  });

  test('TC-AUTH-03 admin can access admin dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const adminDashboardPage = new AdminDashboardPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.admin);
    await appShell.expectSignedInAs(seedUsers.admin.fullName!);

    await adminDashboardPage.goto();
    await adminDashboardPage.expectLoaded();
  });
});
