import { PERMISSIONS, type OrganizationRole, type Permission } from '../types';

const ALL_PERMISSIONS = Object.values(PERMISSIONS);

const ROLE_PERMISSIONS: Record<OrganizationRole, readonly Permission[]> = {
  Owner: ALL_PERMISSIONS,
  Admin: [
    PERMISSIONS.OrganizationRead,
    PERMISSIONS.OrganizationUpdate,
    PERMISSIONS.PodRead,
    PERMISSIONS.PodCreate,
    PERMISSIONS.PodUpdate,
    PERMISSIONS.PodDelete,
    PERMISSIONS.ProviderRead,
    PERMISSIONS.ProviderCreate,
    PERMISSIONS.ProviderUpdate,
    PERMISSIONS.ProviderDelete,
    PERMISSIONS.ModelPull,
    PERMISSIONS.ModelDelete,
    PERMISSIONS.DashboardView,
    PERMISSIONS.BillingView,
    PERMISSIONS.MemberRead,
    PERMISSIONS.MemberManage,
    PERMISSIONS.MemberRoleUpdate,
    PERMISSIONS.InvitationCreate,
    PERMISSIONS.OrchestratorRead,
    PERMISSIONS.OrchestratorManage,
    PERMISSIONS.ObservabilityRead,
    PERMISSIONS.ObservabilityExport,
  ],
  Developer: [
    PERMISSIONS.OrganizationRead,
    PERMISSIONS.PodRead,
    PERMISSIONS.PodCreate,
    PERMISSIONS.PodUpdate,
    PERMISSIONS.PodDelete,
    PERMISSIONS.ProviderRead,
    PERMISSIONS.ProviderCreate,
    PERMISSIONS.ProviderUpdate,
    PERMISSIONS.ProviderDelete,
    PERMISSIONS.ModelPull,
    PERMISSIONS.ModelDelete,
    PERMISSIONS.DashboardView,
    PERMISSIONS.MemberRead,
    PERMISSIONS.OrchestratorRead,
    PERMISSIONS.OrchestratorManage,
    PERMISSIONS.ObservabilityRead,
  ],
  Viewer: [
    PERMISSIONS.OrganizationRead,
    PERMISSIONS.PodRead,
    PERMISSIONS.DashboardView,
    PERMISSIONS.BillingView,
    PERMISSIONS.MemberRead,
    PERMISSIONS.ProviderRead,
    PERMISSIONS.OrchestratorRead,
    PERMISSIONS.ObservabilityRead,
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
