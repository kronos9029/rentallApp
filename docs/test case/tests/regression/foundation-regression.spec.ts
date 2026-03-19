import fs from 'node:fs';
import path from 'node:path';
import { expect, test } from '@playwright/test';
import { repoRoot, runIfEnabled } from '../helpers/runtime';
import { execFileAsync } from '../helpers/shell';

function dotnetCommandEnv(): NodeJS.ProcessEnv {
  const env = { ...process.env };
  delete env.BASE_URL;
  delete env.ASPNETCORE_URLS;
  return env;
}

test.describe('Regression: foundation, DevOps, and encrypted-config baseline', () => {
  test('TC-FOUND-01 repository contains expected sprint-1 solution structure', async () => {
    const root = repoRoot();

    for (const relativePath of [
      'src/RentalApp.Web',
      'src/RentalApp.Application',
      'src/RentalApp.Domain',
      'src/RentalApp.Infrastructure',
      'src/RentalApp.Worker',
      'tests/RentalApp.UnitTests',
      'tests/RentalApp.IntegrationTests',
      'build/scripts',
      '.github/workflows',
    ]) {
      expect(fs.existsSync(path.join(root, relativePath))).toBeTruthy();
    }
  });

  test('TC-FOUND-02 CI workflow contains restore, build, test, and docker steps', async () => {
    const workflow = fs.readFileSync(path.join(repoRoot(), '.github/workflows/ci.yml'), 'utf8');

    expect(workflow).toContain('dotnet restore RentalApp.slnx');
    expect(workflow).toContain('dotnet build RentalApp.slnx --no-restore --configuration Release');
    expect(workflow).toContain('dotnet test RentalApp.slnx --no-build --configuration Release');
    expect(workflow).toContain('docker build -f build/docker/web.Dockerfile');
    expect(workflow).toContain('docker build -f build/docker/worker.Dockerfile');
  });

  test('TC-FOUND-03 docker artifacts for web, worker, and compose exist', async () => {
    const root = repoRoot();

    expect(fs.existsSync(path.join(root, 'docker-compose.yml'))).toBeTruthy();
    expect(fs.existsSync(path.join(root, 'build/docker/web.Dockerfile'))).toBeTruthy();
    expect(fs.existsSync(path.join(root, 'build/docker/worker.Dockerfile'))).toBeTruthy();
  });

  test('TC-FOUND-04 docker compose config resolves successfully', async () => {
    test.skip(!runIfEnabled('ENABLE_INFRA_COMMAND_TESTS'), 'Set ENABLE_INFRA_COMMAND_TESTS=true to run shell-backed infrastructure checks.');

    const result = await execFileAsync('docker', ['compose', 'config'], {
      cwd: repoRoot(),
      timeoutMs: 120_000,
    });

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain('services:');
  });

  test('TC-FOUND-05 encrypted secret helper emits enc:: payload', async () => {
    const result = await execFileAsync(
      'cmd.exe',
      ['/c', 'build\\scripts\\Protect-Secrets.cmd', '--value', 'playwright-secret', '--key-base64', 'AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA='],
      {
        cwd: repoRoot(),
        timeoutMs: 120_000,
      },
    );

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain('enc::');
  });

  test('TC-FOUND-06 dotnet tool restore, build, and test can be invoked from repo root', async () => {
    test.skip(!runIfEnabled('ENABLE_DOTNET_COMMAND_TESTS'), 'Set ENABLE_DOTNET_COMMAND_TESTS=true to run build/test command checks.');

    const toolRestore = await execFileAsync('dotnet', ['tool', 'restore'], {
      cwd: repoRoot(),
      env: dotnetCommandEnv(),
      timeoutMs: 180_000,
    });
    expect(toolRestore.exitCode).toBe(0);

    const build = await execFileAsync('dotnet', ['build', 'RentalApp.slnx'], {
      cwd: repoRoot(),
      env: dotnetCommandEnv(),
      timeoutMs: 300_000,
    });
    expect(build.exitCode).toBe(0);

    const testResult = await execFileAsync('dotnet', ['test', 'RentalApp.slnx'], {
      cwd: repoRoot(),
      env: dotnetCommandEnv(),
      timeoutMs: 300_000,
    });
    expect(testResult.exitCode).toBe(0);
  });
});
