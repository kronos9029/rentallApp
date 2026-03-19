import { execFile } from 'node:child_process';

export function execFileAsync(
  file: string,
  args: string[],
  options: {
    cwd?: string;
    env?: NodeJS.ProcessEnv;
    timeoutMs?: number;
  } = {},
): Promise<{ stdout: string; stderr: string; exitCode: number }> {
  return new Promise((resolve, reject) => {
    execFile(
      file,
      args,
      {
        cwd: options.cwd,
        env: options.env,
        timeout: options.timeoutMs ?? 120_000,
        windowsHide: true,
        shell: false,
      },
      (error, stdout, stderr) => {
        if (error && typeof (error as NodeJS.ErrnoException).code !== 'number') {
          reject(error);
          return;
        }

        resolve({
          stdout,
          stderr,
          exitCode: typeof (error as NodeJS.ErrnoException | null)?.code === 'number'
            ? Number((error as NodeJS.ErrnoException).code)
            : 0,
        });
      },
    );
  });
}
