import { Badge } from 'reactstrap';
import { useOrganization } from '../../contexts/OrganizationContext';
import type { Permission } from '../../types';

interface PermissionBadgeProps {
  permission: Permission;
  label?: string;
}

export const PermissionBadge = ({ permission, label }: PermissionBadgeProps) => {
  const { hasPermission: canAccess } = useOrganization();
  const granted = canAccess(permission);

  return (
    <Badge color={granted ? 'success' : 'light'} className="permission-badge">
      {label ?? permission} {granted ? '✓' : '✗'}
    </Badge>
  );
};
