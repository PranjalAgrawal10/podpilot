import { PERMISSIONS, type OrganizationRole, type Permission } from '../types';

const ALL_PERMISSIONS = Object.values(PERMISSIONS);

const ROLE_PERMISSIONS: Record<OrganizationRole, readonly Permission[]> = {
  Owner: ALL_PERMISSIONS,
  Admin: [
    PERMISSIONS.OrganizationRead,
    PERMISSIONS.OrganizationUpdate,
    PERMISSIONS.PodCreate,
    PERMISSIONS.PodDelete,
    PERMISSIONS.ProviderCreate,
    PERMISSIONS.ProviderDelete,
    PERMISSIONS.ModelPull,
    PERMISSIONS.ModelDelete,
    PERMISSIONS.DashboardView,
    PERMISSIONS.BillingView,
    PERMISSIONS.MemberRead,
    PERMISSIONS.MemberManage,
    PERMISSIONS.MemberRoleUpdate,
    PERMISSIONS.InvitationCreate,
  ],
  Developer: [
    PERMISSIONS.OrganizationRead,
    PERMISSIONS.PodCreate,
    PERMISSIONS.PodDelete,
    PERMISSIONS.ProviderCreate,
    PERMISSIONS.ProviderDelete,
    PERMISSIONS.ModelPull,
    PERMISSIONS.ModelDelete,
    PERMISSIONS.DashboardView,
    PERMISSIONS.MemberRead,
  ],
  Viewer: [
    PERMISSIONS.OrganizationRead,
    PERMISSIONS.DashboardView,
    PERMISSIONS.BillingView,
    PERMISSIONS.MemberRead,
  ],
};

export const parseOrganizationRole = (role: string | null | undefined): OrganizationRole | null => {
  if (!role) return null;
  const normalized = role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
  if (normalized in ROLE_PERMISSIONS) {
    return normalized as OrganizationRole;
  }
  return null;
};

export const hasPermission = (role: OrganizationRole | null, permission: Permission): boolean => {
  if (!role) return false;
  return ROLE_PERMISSIONS[role].includes(permission);
};

export const getPermissionsForRole = (role: OrganizationRole): readonly Permission[] =>
  ROLE_PERMISSIONS[role];
