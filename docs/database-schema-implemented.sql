-- =========================================================
-- RentalApp - Implemented Database Schema
-- Scope: Sprint 2 Part 2 Foundation
-- Source of truth:
--   src/RentalApp.Infrastructure/Persistence/Migrations/20260318043353_Sprint01Foundation.cs
--   src/RentalApp.Infrastructure/Persistence/Migrations/20260318063342_Sprint02Part1AvailabilityHold.cs
--   src/RentalApp.Infrastructure/Persistence/Migrations/20260318081607_Sprint02Part2ExpiryCheckout.cs
-- =========================================================

CREATE DATABASE IF NOT EXISTS rental_app
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE rental_app;

SET NAMES utf8mb4;
SET time_zone = '+00:00';

-- =========================================================
-- Identity and Access
-- =========================================================

CREATE TABLE IF NOT EXISTS users (
  user_id              VARCHAR(36)   NOT NULL,
  email                VARCHAR(191)  NOT NULL,
  username             VARCHAR(100)  NOT NULL,
  password_hash        VARCHAR(512)  NOT NULL,
  phone_number         VARCHAR(20)   NULL,
  status               VARCHAR(20)   NOT NULL,
  last_login_at        DATETIME(6)   NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (user_id),
  UNIQUE KEY uq_users_email (email),
  UNIQUE KEY uq_users_username (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS roles (
  role_id              VARCHAR(36)   NOT NULL,
  role_name            VARCHAR(50)   NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (role_id),
  UNIQUE KEY uq_roles_name (role_name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS user_roles (
  user_id              VARCHAR(36)   NOT NULL,
  role_id              VARCHAR(36)   NOT NULL,
  assigned_at          DATETIME(6)   NOT NULL,
  PRIMARY KEY (user_id, role_id),
  KEY ix_user_roles_role_id (role_id),
  CONSTRAINT fk_user_roles_user
    FOREIGN KEY (user_id) REFERENCES users(user_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_user_roles_role
    FOREIGN KEY (role_id) REFERENCES roles(role_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS user_profiles (
  user_id              VARCHAR(36)   NOT NULL,
  full_name            VARCHAR(150)  NULL,
  avatar_url           VARCHAR(500)  NULL,
  date_of_birth        DATE          NULL,
  emergency_contact    VARCHAR(50)   NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (user_id),
  CONSTRAINT fk_user_profiles_user
    FOREIGN KEY (user_id) REFERENCES users(user_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS password_reset_tokens (
  token_id             VARCHAR(36)   NOT NULL,
  user_id              VARCHAR(36)   NOT NULL,
  token_hash           VARCHAR(64)   NOT NULL,
  expires_at           DATETIME(6)   NOT NULL,
  used_at              DATETIME(6)   NULL,
  created_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (token_id),
  UNIQUE KEY uq_password_reset_token_hash (token_hash),
  KEY ix_password_reset_user (user_id),
  KEY ix_password_reset_expires (expires_at),
  CONSTRAINT fk_password_reset_user
    FOREIGN KEY (user_id) REFERENCES users(user_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =========================================================
-- Booking Foundation
-- =========================================================

CREATE TABLE IF NOT EXISTS courts (
  court_id             VARCHAR(36)   NOT NULL,
  court_code           VARCHAR(20)   NOT NULL,
  court_name           VARCHAR(100)  NOT NULL,
  sort_order           TINYINT UNSIGNED NOT NULL,
  is_active            BOOLEAN       NOT NULL,
  maintenance_reason   VARCHAR(255)  NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (court_id),
  UNIQUE KEY uq_courts_code (court_code),
  KEY ix_courts_is_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS time_buckets (
  bucket_id            VARCHAR(36)   NOT NULL,
  start_at             DATETIME(6)   NOT NULL,
  end_at               DATETIME(6)   NOT NULL,
  duration_min         SMALLINT      NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (bucket_id),
  UNIQUE KEY uq_time_buckets_window (start_at, end_at),
  KEY ix_time_buckets_start_at (start_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS court_buckets (
  court_id             VARCHAR(36)   NOT NULL,
  bucket_id            VARCHAR(36)   NOT NULL,
  mode                 VARCHAR(20)   NOT NULL DEFAULT 'None',
  shared_capacity      TINYINT UNSIGNED NOT NULL DEFAULT 8,
  shared_reserved      TINYINT UNSIGNED NOT NULL DEFAULT 0,
  private_hold_id      VARCHAR(36)   NULL,
  lock_version         INT UNSIGNED  NOT NULL DEFAULT 0,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (court_id, bucket_id),
  KEY ix_court_buckets_bucket_id (bucket_id),
  KEY ix_court_buckets_mode (mode),
  CONSTRAINT fk_court_buckets_court
    FOREIGN KEY (court_id) REFERENCES courts(court_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_court_buckets_bucket
    FOREIGN KEY (bucket_id) REFERENCES time_buckets(bucket_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =========================================================
-- Pricing and Cancellation
-- =========================================================

CREATE TABLE IF NOT EXISTS pricing_rules (
  pricing_rule_id      VARCHAR(36)   NOT NULL,
  rule_name            VARCHAR(120)  NOT NULL,
  applies_to           VARCHAR(20)   NOT NULL,
  day_type             VARCHAR(20)   NOT NULL,
  start_time           TIME(6)       NULL,
  end_time             TIME(6)       NULL,
  private_rate         DECIMAL(12,2) NOT NULL,
  shared_rate          DECIMAL(12,2) NOT NULL,
  weekend_markup_pct   DECIMAL(5,2)  NOT NULL,
  is_active            BOOLEAN       NOT NULL,
  effective_from       DATETIME(6)   NOT NULL,
  effective_to         DATETIME(6)   NULL,
  created_by_user_id   VARCHAR(36)   NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (pricing_rule_id),
  KEY ix_pricing_rules_created_by_user (created_by_user_id),
  KEY ix_pricing_rules_active_effective (is_active, effective_from),
  CONSTRAINT fk_pricing_rules_created_by_user
    FOREIGN KEY (created_by_user_id) REFERENCES users(user_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS cancellation_policies (
  policy_id            VARCHAR(36)   NOT NULL,
  policy_name          VARCHAR(100)  NOT NULL,
  min_hours_before_start INT         NOT NULL,
  refund_strategy      VARCHAR(20)   NOT NULL,
  refund_percent       DECIMAL(5,2)  NULL,
  cancel_fee_percent   DECIMAL(5,2)  NOT NULL,
  is_active            BOOLEAN       NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (policy_id),
  KEY ix_cancellation_policies_is_active (is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =========================================================
-- Hold, Booking, Checkout, and Idempotency
-- =========================================================

CREATE TABLE IF NOT EXISTS holds (
  hold_id              VARCHAR(36)   NOT NULL,
  user_id              VARCHAR(36)   NOT NULL,
  status               VARCHAR(20)   NOT NULL,
  expires_at           DATETIME(6)   NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (hold_id),
  KEY ix_holds_status_expires (status, expires_at),
  KEY ix_holds_user_created (user_id, created_at),
  CONSTRAINT fk_holds_user
    FOREIGN KEY (user_id) REFERENCES users(user_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS hold_items (
  hold_item_id         VARCHAR(36)   NOT NULL,
  hold_id              VARCHAR(36)   NOT NULL,
  court_id             VARCHAR(36)   NOT NULL,
  bucket_id            VARCHAR(36)   NOT NULL,
  booking_mode         VARCHAR(20)   NOT NULL,
  slot_qty             TINYINT UNSIGNED NOT NULL DEFAULT 1,
  unit_price           DECIMAL(12,2) NOT NULL,
  line_total           DECIMAL(12,2) NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (hold_item_id),
  UNIQUE KEY uq_hold_items_scope (hold_id, court_id, bucket_id, booking_mode),
  KEY ix_hold_items_court_bucket (court_id, bucket_id),
  CONSTRAINT fk_hold_items_hold
    FOREIGN KEY (hold_id) REFERENCES holds(hold_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_hold_items_court
    FOREIGN KEY (court_id) REFERENCES courts(court_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_hold_items_bucket
    FOREIGN KEY (bucket_id) REFERENCES time_buckets(bucket_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS bookings (
  booking_id           VARCHAR(36)   NOT NULL,
  user_id              VARCHAR(36)   NOT NULL,
  hold_id              VARCHAR(36)   NULL,
  status               VARCHAR(20)   NOT NULL,
  total_amount         DECIMAL(12,2) NOT NULL,
  booking_date         DATE          NOT NULL,
  start_at             DATETIME(6)   NOT NULL,
  end_at               DATETIME(6)   NOT NULL,
  cancellation_policy_id VARCHAR(36) NULL,
  cancelled_at         DATETIME(6)   NULL,
  confirmed_at         DATETIME(6)   NULL,
  completed_at         DATETIME(6)   NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (booking_id),
  UNIQUE KEY uq_bookings_hold (hold_id),
  KEY ix_bookings_user_created (user_id, created_at),
  KEY ix_bookings_status_date (status, booking_date),
  CONSTRAINT fk_bookings_user
    FOREIGN KEY (user_id) REFERENCES users(user_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_bookings_hold
    FOREIGN KEY (hold_id) REFERENCES holds(hold_id),
  CONSTRAINT fk_bookings_cancellation_policy
    FOREIGN KEY (cancellation_policy_id) REFERENCES cancellation_policies(policy_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS booking_items (
  booking_item_id      VARCHAR(36)   NOT NULL,
  booking_id           VARCHAR(36)   NOT NULL,
  court_id             VARCHAR(36)   NOT NULL,
  bucket_id            VARCHAR(36)   NOT NULL,
  booking_mode         VARCHAR(20)   NOT NULL,
  slot_qty             TINYINT UNSIGNED NOT NULL DEFAULT 1,
  unit_price           DECIMAL(12,2) NOT NULL,
  line_total           DECIMAL(12,2) NOT NULL,
  status               VARCHAR(20)   NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (booking_item_id),
  KEY ix_booking_items_booking (booking_id),
  KEY ix_booking_items_court_bucket (court_id, bucket_id),
  CONSTRAINT fk_booking_items_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(booking_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_booking_items_court
    FOREIGN KEY (court_id) REFERENCES courts(court_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_booking_items_bucket
    FOREIGN KEY (bucket_id) REFERENCES time_buckets(bucket_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS checkout_orders (
  order_id             VARCHAR(36)   NOT NULL,
  user_id              VARCHAR(36)   NOT NULL,
  status               VARCHAR(20)   NOT NULL,
  subtotal_amount      DECIMAL(12,2) NOT NULL,
  discount_amount      DECIMAL(12,2) NOT NULL,
  total_amount         DECIMAL(12,2) NOT NULL,
  currency             VARCHAR(3)    NOT NULL,
  expires_at           DATETIME(6)   NULL,
  created_at           DATETIME(6)   NOT NULL,
  updated_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (order_id),
  KEY ix_checkout_orders_user_created (user_id, created_at),
  KEY ix_checkout_orders_status_created (status, created_at),
  CONSTRAINT fk_checkout_orders_user
    FOREIGN KEY (user_id) REFERENCES users(user_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS checkout_order_items (
  order_item_id        VARCHAR(36)   NOT NULL,
  order_id             VARCHAR(36)   NOT NULL,
  booking_id           VARCHAR(36)   NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (order_item_id),
  UNIQUE KEY uq_checkout_order_booking (order_id, booking_id),
  KEY ix_checkout_order_items_booking (booking_id),
  CONSTRAINT fk_checkout_order_items_order
    FOREIGN KEY (order_id) REFERENCES checkout_orders(order_id)
    ON DELETE CASCADE,
  CONSTRAINT fk_checkout_order_items_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(booking_id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS idempotency_keys (
  idempotency_id       VARCHAR(36)   NOT NULL,
  scope                VARCHAR(36)   NOT NULL,
  endpoint             VARCHAR(80)   NOT NULL,
  idempotency_key      VARCHAR(64)   NOT NULL,
  request_hash         VARCHAR(64)   NOT NULL,
  response_status      INT           NOT NULL,
  response_snapshot    LONGTEXT      NOT NULL,
  expires_at           DATETIME(6)   NOT NULL,
  created_at           DATETIME(6)   NOT NULL,
  PRIMARY KEY (idempotency_id),
  UNIQUE KEY uq_idempotency_scope_endpoint_key (scope, endpoint, idempotency_key),
  KEY ix_idempotency_expires (expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- =========================================================
-- Seed Data Included In Migration
-- =========================================================

INSERT INTO roles (role_id, role_name, created_at)
VALUES
  ('00000000-0000-0000-0000-000000000001', 'Admin', '2026-03-18 00:00:00.000000'),
  ('00000000-0000-0000-0000-000000000002', 'Customer', '2026-03-18 00:00:00.000000');

INSERT INTO users (
  user_id,
  email,
  username,
  password_hash,
  phone_number,
  status,
  last_login_at,
  created_at,
  updated_at
)
VALUES (
  '10000000-0000-0000-0000-000000000001',
  'admin@local.test',
  'admin.local',
  'AQAAAAIAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v+9B/m68fN7pVomQZIk2QhdaT58yv9z9zo2k0tHeC3mUA==',
  NULL,
  'Active',
  NULL,
  '2026-03-18 00:00:00.000000',
  '2026-03-18 00:00:00.000000'
);

INSERT INTO users (
  user_id,
  email,
  username,
  password_hash,
  phone_number,
  status,
  last_login_at,
  created_at,
  updated_at
)
VALUES (
  '10000000-0000-0000-0000-000000000002',
  'customer@local.test',
  'customer.local',
  'AQAAAAIAAYagAAAAEBAhMkNUZXaHmKm6y9zt/g/dvnXaKV7KW02eQ7Sj/tJyjyBsxoyog7jpqOJeHhupEg==',
  NULL,
  'Active',
  NULL,
  '2026-03-18 00:00:00.000000',
  '2026-03-18 00:00:00.000000'
);

INSERT INTO user_profiles (
  user_id,
  full_name,
  avatar_url,
  date_of_birth,
  emergency_contact,
  created_at,
  updated_at
)
VALUES (
  '10000000-0000-0000-0000-000000000001',
  'Admin Local',
  NULL,
  NULL,
  NULL,
  '2026-03-18 00:00:00.000000',
  '2026-03-18 00:00:00.000000'
);

INSERT INTO user_profiles (
  user_id,
  full_name,
  avatar_url,
  date_of_birth,
  emergency_contact,
  created_at,
  updated_at
)
VALUES (
  '10000000-0000-0000-0000-000000000002',
  'Customer Local',
  NULL,
  NULL,
  NULL,
  '2026-03-18 00:00:00.000000',
  '2026-03-18 00:00:00.000000'
);

INSERT INTO user_roles (user_id, role_id, assigned_at)
VALUES (
  '10000000-0000-0000-0000-000000000001',
  '00000000-0000-0000-0000-000000000001',
  '2026-03-18 00:00:00.000000'
);

INSERT INTO user_roles (user_id, role_id, assigned_at)
VALUES (
  '10000000-0000-0000-0000-000000000002',
  '00000000-0000-0000-0000-000000000002',
  '2026-03-18 00:00:00.000000'
);

INSERT INTO courts (
  court_id,
  court_code,
  court_name,
  sort_order,
  is_active,
  maintenance_reason,
  created_at,
  updated_at
)
VALUES
  ('20000000-0000-0000-0000-000000000001', 'C01', 'Court 01', 1, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000002', 'C02', 'Court 02', 2, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000003', 'C03', 'Court 03', 3, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000004', 'C04', 'Court 04', 4, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000005', 'C05', 'Court 05', 5, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000006', 'C06', 'Court 06', 6, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000007', 'C07', 'Court 07', 7, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000008', 'C08', 'Court 08', 8, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000'),
  ('20000000-0000-0000-0000-000000000009', 'C09', 'Court 09', 9, TRUE, NULL, '2026-03-18 00:00:00.000000', '2026-03-18 00:00:00.000000');

INSERT INTO pricing_rules (
  pricing_rule_id,
  rule_name,
  applies_to,
  day_type,
  start_time,
  end_time,
  private_rate,
  shared_rate,
  weekend_markup_pct,
  is_active,
  effective_from,
  effective_to,
  created_by_user_id,
  created_at,
  updated_at
)
VALUES
  (
    '40000000-0000-0000-0000-000000000001',
    'Weekday Baseline',
    'Any',
    'Weekday',
    '06:00:00.000000',
    '22:00:00.000000',
    280000.00,
    45000.00,
    0.00,
    TRUE,
    '2026-01-01 00:00:00.000000',
    NULL,
    '10000000-0000-0000-0000-000000000001',
    '2026-03-18 00:00:00.000000',
    '2026-03-18 00:00:00.000000'
  ),
  (
    '40000000-0000-0000-0000-000000000002',
    'Weekend Baseline',
    'Any',
    'Weekend',
    '06:00:00.000000',
    '22:00:00.000000',
    280000.00,
    45000.00,
    20.00,
    TRUE,
    '2026-01-01 00:00:00.000000',
    NULL,
    '10000000-0000-0000-0000-000000000001',
    '2026-03-18 00:00:00.000000',
    '2026-03-18 00:00:00.000000'
  );

INSERT INTO cancellation_policies (
  policy_id,
  policy_name,
  min_hours_before_start,
  refund_strategy,
  refund_percent,
  cancel_fee_percent,
  is_active,
  created_at,
  updated_at
)
VALUES (
  '30000000-0000-0000-0000-000000000001',
  'Standard 2 Hour Refund',
  2,
  'Full',
  100.00,
  0.00,
  TRUE,
  '2026-03-18 00:00:00.000000',
  '2026-03-18 00:00:00.000000'
);
