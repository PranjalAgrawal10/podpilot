const DEFAULT_ATTEMPTS = 30;
const DEFAULT_INTERVAL_MS = 2000;

export async function waitForApi(
  signal: AbortSignal,
  maxAttempts = DEFAULT_ATTEMPTS,
  intervalMs = DEFAULT_INTERVAL_MS,
): Promise<boolean> {
  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    if (signal.aborted) {
      return false;
    }

    try {
      const response = await fetch('/api/v1/health', { signal });
      if (response.ok) {
        return true;
      }
    } catch {
      if (signal.aborted) {
        return false;
      }
    }

    await delay(intervalMs, signal);
  }

  return false;
}

async function delay(ms: number, signal: AbortSignal): Promise<void> {
  if (signal.aborted) {
    return;
  }

  await new Promise<void>((resolve) => {
    const timer = window.setTimeout(() => resolve(), ms);
    signal.addEventListener(
      'abort',
      () => {
        window.clearTimeout(timer);
        resolve();
      },
      { once: true },
    );
  });
}
