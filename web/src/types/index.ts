export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: Record<string, string[]>;
  correlationId?: string;
}

export type OrganizationRole = 'Owner' | 'Admin' | 'Developer' | 'Viewer';

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  currentOrganizationId?: string | null;
  currentOrganizationRole?: string | null;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  user: UserSummary;
}

export interface OrganizationSummary {
  id: string;
  name: string;
  slug: string;
  role: string;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  organizations: OrganizationSummary[];
}

export interface Organization {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  logo?: string | null;
  ownerUserId: string;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  currentUserRole?: string | null;
}

export interface Member {
  id: string;
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  status: string;
  joinedAt: string;
}

export interface Invitation {
  id: string;
  organizationId: string;
  email: string;
  role: string;
  status: string;
  expiresAt: string;
  token: string;
}

export interface CreateOrganizationRequest {
  name: string;
  description?: string | null;
  logo?: string | null;
}

export interface UpdateOrganizationRequest {
  name?: string | null;
  description?: string | null;
  logo?: string | null;
}

export interface SwitchOrganizationRequest {
  organizationId: string;
}

export interface AddMemberRequest {
  email: string;
  role: string;
}

export interface UpdateMemberRoleRequest {
  role: string;
}

export interface InviteMemberRequest {
  email: string;
  role: string;
}

export interface AcceptInvitationRequest {
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationName: string;
}

export interface HealthResponse {
  status: string;
  checks: Record<string, { status: string; description?: string; duration: string }>;
  totalDuration: string;
}

export type ThemeMode = 'light' | 'dark';

export const PERMISSIONS = {
  OrganizationRead: 'Organization.Read',
  OrganizationUpdate: 'Organization.Update',
  OrganizationDelete: 'Organization.Delete',
  PodCreate: 'Pod.Create',
  PodDelete: 'Pod.Delete',
  ProviderCreate: 'Provider.Create',
  ProviderDelete: 'Provider.Delete',
  ModelPull: 'Model.Pull',
  ModelDelete: 'Model.Delete',
  DashboardView: 'Dashboard.View',
  BillingView: 'Billing.View',
  MemberRead: 'Member.Read',
  MemberManage: 'Member.Manage',
  MemberRoleUpdate: 'Member.RoleUpdate',
  InvitationCreate: 'Invitation.Create',
} as const;

export type Permission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
