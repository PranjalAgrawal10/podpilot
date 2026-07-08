import { Badge } from 'reactstrap';
import type { ProviderConnectionStatus } from '../../types';

interface HealthBadgeProps {
  status: ProviderConnectionStatus;
  className?: string;
}

const statusColors: Record<ProviderConnectionStatus, string> = {
  Connected: 'success',
  Disconnected: 'danger',
  Degraded: 'warning',
  Unknown: 'secondary',
};

export const HealthBadge = ({ status, className = '' }: HealthBadgeProps) => (
  <Badge color={statusColors[status] ?? 'secondary'} className={`health-badge ${className}`.trim()}>
    {status}
  </Badge>
);
