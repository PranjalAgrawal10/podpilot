import { Badge } from 'reactstrap';

export const HealthBadge = ({ status }: { status: string }) => {
  const color =
    status === 'Healthy'
      ? 'success'
      : status === 'Unavailable' || status === 'ModelMissing'
        ? 'secondary'
        : 'danger';

  return <Badge color={color}>{status}</Badge>;
};
