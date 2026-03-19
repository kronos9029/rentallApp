import { expect, type Page } from '@playwright/test';

export type BookingModeLabel = 'Private' | 'Shared';

export class AvailabilityPage {
  constructor(private readonly page: Page) {}

  async goto(): Promise<void> {
    await this.page.goto('/Booking/Availability');
    await this.expectLoaded();
  }

  async expectLoaded(): Promise<void> {
    await expect(this.page.getByRole('heading', { name: 'Tim availability va tao hold' })).toBeVisible();
  }

  async search(options: { bookingDate: string; bookingMode: BookingModeLabel; slotQuantity?: number; expectResults?: boolean }): Promise<void> {
    await this.expectLoaded();

    await this.page.getByLabel('Ngay booking').fill(options.bookingDate);
    await this.page.getByLabel('Loai booking').selectOption({ label: options.bookingMode });

    if (options.slotQuantity !== undefined) {
      await this.page.getByLabel('So slot shared').fill(String(options.slotQuantity));
    }

    await this.page.getByRole('button', { name: 'Tai availability' }).click();
    await expect(this.page).toHaveURL(new RegExp(`/Booking/Availability\\?.*Filter\\.BookingDate=${options.bookingDate}`));
    if (options.expectResults ?? true) {
      await this.expectResultsLoaded();
    }
  }

  async expectResultsLoaded(): Promise<void> {
    await expect(this.page.locator('input[name="HoldInput.SelectedCourtId"]').first()).toBeVisible();
  }

  async gotoWithQuery(query: string): Promise<void> {
    await this.page.goto(`/Booking/Availability${query}`);
  }

  async expectBookingDate(bookingDate: string): Promise<void> {
    await expect(this.page.getByLabel('Ngay booking')).toHaveValue(bookingDate);
  }

  async expectSharedRemainingSlots(): Promise<void> {
    await expect(this.page.getByText(/slot con lai/).first()).toBeVisible();
  }

  async selectFirstAvailableCourtAndBucket(): Promise<void> {
    const courtRadio = this.page.locator('input[name="HoldInput.SelectedCourtId"]').first();
    const bucketCheckbox = this.page.locator('input[name="HoldInput.SelectedBucketIds"]:not([disabled])').first();

    await expect(courtRadio).toBeVisible();
    await expect(bucketCheckbox).toBeVisible();

    await courtRadio.check();
    await bucketCheckbox.check();
  }

  async submitHold(): Promise<void> {
    await this.page.getByRole('button', { name: 'Tao hold' }).click();
  }

  async expectSelectCourtError(): Promise<void> {
    await expect(this.page.getByText('Ban can chon mot san.')).toBeVisible();
  }

  async expectSelectBucketError(): Promise<void> {
    await expect(this.page.getByText('Ban can chon it nhat mot khung gio.')).toBeVisible();
  }

  async selectOnlyFirstCourt(): Promise<void> {
    const courtRadio = this.page.locator('input[name="HoldInput.SelectedCourtId"]').first();
    await expect(courtRadio).toBeVisible();
    await courtRadio.check();
  }

  async expectEmptyState(): Promise<void> {
    await expect(this.page.getByText('Chua co availability cho ngay va bo loc da chon.')).toBeVisible();
  }
}
