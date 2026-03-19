import { spawn, type ChildProcessWithoutNullStreams } from 'node:child_process';
import fs from 'node:fs';
import path from 'node:path';
import type { TestInfo } from '@playwright/test';

export function repoRoot(): string {
  return path.resolve(__dirname, '..', '..', '..', '..');
}

export function runIfEnabled(name: string): boolean {
  return process.env[name] === 'true';
}

export function testOutputFile(testInfo: TestInfo, fileName: string): string {
  const dir = testInfo.outputPath('');
  fs.mkdirSync(dir, { recursive: true });
  return path.join(dir, fileName);
}

export async function waitFor(condition: () => Promise<boolean>, timeoutMs: number, intervalMs = 1000): Promise<boolean> {
  const started = Date.now();
  while (Date.now() - started < timeoutMs) {
    if (await condition()) {
      return true;
    }
    await new Promise((resolve) => setTimeout(resolve, intervalMs));
  }
  return false;
}

export function startProcess(
  command: string,
  args: string[],
  options: {
    cwd?: string;
    env?: NodeJS.ProcessEnv;
    stdoutFile?: string;
    stderrFile?: string;
  } = {},
): ChildProcessWithoutNullStreams {
  const child = spawn(command, args, {
    cwd: options.cwd,
    env: options.env,
    stdio: 'pipe',
    shell: false,
  });

  if (options.stdoutFile) {
    child.stdout.pipe(fs.createWriteStream(options.stdoutFile, { flags: 'a' }));
  }

  if (options.stderrFile) {
    child.stderr.pipe(fs.createWriteStream(options.stderrFile, { flags: 'a' }));
  }

  return child;
}

export async function stopProcess(child: ChildProcessWithoutNullStreams): Promise<void> {
  if (child.killed) {
    return;
  }

  child.kill('SIGTERM');
  await new Promise((resolve) => setTimeout(resolve, 1000));
  if (!child.killed) {
    child.kill('SIGKILL');
  }
}
