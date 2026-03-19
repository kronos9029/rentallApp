const mysql = require('mysql2/promise') as {
  createConnection: (config: DbConfig) => Promise<{
    query: (sql: string, values?: unknown[]) => Promise<[unknown, unknown]>;
    execute: (sql: string, values?: unknown[]) => Promise<[unknown, unknown]>;
    end: () => Promise<void>;
  }>;
};

export type DbConfig = {
  host: string;
  port: number;
  user: string;
  password: string;
  database: string;
};

export function getDbConfig(): DbConfig {
  return {
    host: process.env.TEST_DB_HOST ?? '127.0.0.1',
    port: Number(process.env.TEST_DB_PORT ?? '3306'),
    user: process.env.TEST_DB_USER ?? 'root',
    password: process.env.TEST_DB_PASSWORD ?? '',
    database: process.env.TEST_DB_NAME ?? 'rental_app',
  };
}

export async function canConnectToDb(): Promise<boolean> {
  try {
    const connection = await mysql.createConnection(getDbConfig());
    await connection.end();
    return true;
  } catch {
    return false;
  }
}

export async function queryRows<T>(sql: string, values: unknown[] = []): Promise<T[]> {
  const connection = await mysql.createConnection(getDbConfig());
  try {
    const [rows] = await connection.query(sql, values);
    return rows as T[];
  } finally {
    await connection.end();
  }
}

export async function execute(sql: string, values: unknown[] = []): Promise<void> {
  const connection = await mysql.createConnection(getDbConfig());
  try {
    await connection.execute(sql, values);
  } finally {
    await connection.end();
  }
}
