import path from 'node:path';
import { defineConfig } from '@playwright/test';

const baseURL = process.env.BASE_URL ?? 'https://localhost:7048';
const autoStartRequested = process.env.PLAYWRIGHT_START_WEB_SERVER !== 'false';
const autoStartSupported = /^https:\/\/(localhost|127\.0\.0\.1)(:\d+)?$/i.test(baseURL);

export default defineConfig({
  testDir: './tests',
  fullyParallel: false,
  workers: 1,
  timeout: 45_000,
  expect: {
    timeout: 10_000
  },
  reporter: [
    ['list'],
    ['html', { open: 'never', outputFolder: 'playwright-report' }]
  ],
  use: {
    baseURL,
    headless: true,
    ignoreHTTPSErrors: true,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },
  webServer: autoStartRequested && autoStartSupported
    ? {
        command: 'dotnet run --project src/RentalApp.Web/RentalApp.Web.csproj --no-launch-profile',
        cwd: path.resolve(__dirname, '..', '..'),
        url: `${baseURL}/healthz`,
        ignoreHTTPSErrors: true,
        reuseExistingServer: true,
        timeout: 120_000,
        env: {
          ...process.env,
          ASPNETCORE_ENVIRONMENT: process.env.ASPNETCORE_ENVIRONMENT ?? 'Testing',
          ASPNETCORE_URLS: process.env.ASPNETCORE_URLS ?? baseURL
        }
      }
    : undefined
});
