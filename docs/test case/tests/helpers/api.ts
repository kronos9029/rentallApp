import type { APIRequestContext, Page } from '@playwright/test';

export function createAuthenticatedApiContext(page: Page): APIRequestContext {
  return page.context().request;
}

export function apiUrl(path: string): string {
  return new URL(path, process.env.BASE_URL ?? 'https://localhost:7048').toString();
}
