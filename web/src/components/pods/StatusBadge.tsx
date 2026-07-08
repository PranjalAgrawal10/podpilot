import type { PodStatus } from '../../types';

const STATUS_COLORS: Record<PodStatus, string> = {
  Creating: 'info',
  BuildingPending: 'info',
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

const STATUS_LABELS: Partial<Record<PodStatus, string>> = {
  Creating: 'Building Pending',
  BuildingPending: 'Building Pending',
};

export const StatusBadge = ({ status }: { status: PodStatus }) => (
  <span className={`badge bg-${STATUS_COLORS[status] || 'secondary'}`}>
    {STATUS_LABELS[status] ?? status}
  </span>
);
