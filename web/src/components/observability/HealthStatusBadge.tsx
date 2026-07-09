import { Badge } from 'reactstrap';

const statusColor = (status: string): string => {
  const normalized = status.toLowerCase();
  if (normalized === 'healthy' || normalized === 'ok' || normalized === 'up') {
    return 'success';
  }
  if (normalized === 'degraded' || normalized === 'warning') {
    return 'warning';
  }
  if (normalized === 'unhealthy' || normalized === 'critical' || normalized === 'down' || normalized === 'failed') {
    return 'danger';
  }
  return 'secondary';
};

interface HealthStatusBadgeProps {
  status: string;
  className?: string;
}

export const HealthStatusBadge = ({ status, className }: HealthStatusBadgeProps) => (
  <Badge color={statusColor(status)} className={className}>
    {status}
  </Badge>
);
