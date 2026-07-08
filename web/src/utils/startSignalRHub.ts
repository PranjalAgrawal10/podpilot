import type { HubConnection } from '@microsoft/signalr';
import { waitForApi } from './waitForApi';

export async function startSignalRHub(
  connection: HubConnection,
  signal: AbortSignal,
): Promise<void> {
  const apiReady = await waitForApi(signal);
  if (!apiReady || signal.aborted) {
    return;
  }

  try {
    await connection.start();
  } catch {
    // SignalR is best-effort; pages still poll via React Query.
  }
}
