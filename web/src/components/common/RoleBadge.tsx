import { Badge } from 'reactstrap';
import type { OrganizationRole } from '../../types';

interface RoleBadgeProps {
  role: string;
}

const roleColors: Record<OrganizationRole, string> = {
  Owner: 'danger',
  Admin: 'primary',
  Developer: 'success',
  Viewer: 'secondary',
};

export const RoleBadge = ({ role }: RoleBadgeProps) => {
  const normalized = role.charAt(0).toUpperCase() + role.slice(1).toLowerCase() as OrganizationRole;
  const color = roleColors[normalized] ?? 'secondary';

  return <Badge color={color} className="role-badge">{role}</Badge>;
};
