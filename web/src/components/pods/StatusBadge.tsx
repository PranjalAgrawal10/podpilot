import type { PodStatus } from '../../types';

const STATUS_COLORS: Record<PodStatus, string> = {
  Creating: 'info',
  Starting: 'info',
  Running: 'success',
  Stopping: 'warning',
  Stopped: 'secondary',
  Restarting: 'info',
  Deleting: 'warning',
  Deleted: 'dark',
  Failed: 'danger',
  Unknown: 'light',
};

export const StatusBadge = ({ status }: { status: PodStatus }) => (
  <span className={`badge bg-${STATUS_COLORS[status] || 'secondary'}`}>{status}</span>
);
