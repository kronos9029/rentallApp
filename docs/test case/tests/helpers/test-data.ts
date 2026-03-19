import { randomUUID } from 'node:crypto';
import type { TestInfo } from '@playwright/test';

export type UserCredentials = {
  email: string;
  password: string;
  fullName?: string;
};

export const seedUsers = {
  customer: {
    email: 'customer@local.test',
    password: 'Customer123!',
    fullName: 'Customer Local'
  },
  admin: {
    email: 'admin@local.test',
    password: 'Admin123!',
    fullName: 'Admin Local'
  }
} satisfies Record<string, UserCredentials>;

const runSeed = Number(process.env.PW_RUN_SEED ?? Date.now());

export function buildNewCustomer(): Required<UserCredentials> {
  const suffix = `${runSeed}-${randomUUID().slice(0, 8)}`.toLowerCase();

  return {
    email: `pw.customer.${suffix}@local.test`,
    password: 'Customer123!',
    fullName: `PW Customer ${suffix}`
  };
}

export function bookingDateFor(testInfo: TestInfo, dayOffset = 0): string {
  const date = new Date();
  const baseOffset = 30 + (runSeed % 180) + dayOffset + testInfo.retry + testInfo.parallelIndex;

  date.setUTCDate(date.getUTCDate() + baseOffset);
  return date.toISOString().slice(0, 10);
}
