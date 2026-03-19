import { expect, test } from '@playwright/test';
import { seedUsers } from '../helpers/test-data';
import { AccessDeniedPage } from '../page-objects/AccessDeniedPage';
import { AppShell } from '../page-objects/AppShell';
import { AdminDashboardPage } from '../page-objects/AdminDashboardPage';
import { LoginPage } from '../page-objects/LoginPage';
import { ProfilePage } from '../page-objects/ProfilePage';
import { RegisterPage } from '../page-objects/RegisterPage';

test.describe('Regression: auth negative and authorization flows', () => {
  test('TC-AUTH-04 duplicate register shows existing-email error', async ({ page }) => {
    const registerPage = new RegisterPage(page);

    await registerPage.goto();
    await registerPage.register({
      email: seedUsers.customer.email,
      password: seedUsers.customer.password,
      fullName: seedUsers.customer.fullName ?? 'Customer Local'
    });

    await registerPage.expectLoaded();
    await registerPage.expectDuplicateEmailError();
  });

  test('TC-AUTH-05 invalid login shows error and stays on login page', async ({ page }) => {
    const loginPage = new LoginPage(page);

    await loginPage.goto();
    await loginPage.login({
      email: 'notfound@local.test',
      password: 'WrongPassword123!'
    });

    await loginPage.expectLoaded();
    await loginPage.expectInvalidCredentialsError();
  });

  test('TC-AUTH-06 external returnUrl is ignored after login', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);

    await loginPage.goto('?returnUrl=https://evil.test/phish');
    await loginPage.login(seedUsers.customer);

    await appShell.expectAuthenticated();
    await expect(page).toHaveURL(/\/$/);
  });

  test('TC-AUTH-07 customer does not see admin link in navbar', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();
    await appShell.expectAdminLinkHidden();
  });

  test('TC-AUTH-08 admin sees admin link in navbar', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.admin);
    await appShell.expectAuthenticated();
    await appShell.expectAdminLinkVisible();
  });

  test('TC-AUTH-09 customer cannot access admin dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const adminDashboardPage = new AdminDashboardPage(page);
    const accessDeniedPage = new AccessDeniedPage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await adminDashboardPage.goto();
    await accessDeniedPage.expectLoaded();
  });

  test('TC-PRO-01 profile email field is readonly', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const profilePage = new ProfilePage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await profilePage.goto();
    await profilePage.expectLoaded();
    await profilePage.expectEmailReadonly();
  });

  test('TC-PRO-02 profile rejects empty full name', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const profilePage = new ProfilePage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await profilePage.goto();
    await profilePage.expectLoaded();
    await profilePage.clearFullName();
    await profilePage.submit();

    await expect(page).toHaveURL(/\/Account\/Profile/);
    await profilePage.expectValidationError();
  });

  test('TC-PRO-03 profile rejects overlong full name', async ({ page }) => {
    const loginPage = new LoginPage(page);
    const appShell = new AppShell(page);
    const profilePage = new ProfilePage(page);

    await loginPage.goto();
    await loginPage.login(seedUsers.customer);
    await appShell.expectAuthenticated();

    await profilePage.goto();
    await profilePage.expectLoaded();
    await profilePage.fillOverlongFullName();
    await profilePage.submit();

    await expect(page).toHaveURL(/\/Account\/Profile/);
    await profilePage.expectValidationError();
  });
});
