import apiClient from './api';
import type {
  ApiResponse,
  AuthResponse,
  BeginSsoRequest,
  CompleteSsoRequest,
  CreateIdentityProviderRequest,
  IdentityProvider,
  MfaEnrollmentResponse,
  MfaRequest,
  SecurityDashboard,
  SessionInfo,
  SsoChallengeResponse,
  TrustedDevice,
} from '../types';

export const securityService = {
  getDashboard: async (): Promise<SecurityDashboard> => {
    const response = await apiClient.get<ApiResponse<SecurityDashboard>>('/security/dashboard');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch security dashboard');
    return response.data.data;
  },

  listSessions: async (): Promise<SessionInfo[]> => {
    const response = await apiClient.get<ApiResponse<SessionInfo[]>>('/security/sessions');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch sessions');
    return response.data.data;
  },

  listDevices: async (): Promise<TrustedDevice[]> => {
    const response = await apiClient.get<ApiResponse<TrustedDevice[]>>('/security/devices');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch trusted devices');
    return response.data.data;
  },

  revokeDevice: async (id: string): Promise<void> => {
    await apiClient.delete(`/security/devices/${id}`);
  },

  listIdentityProviders: async (): Promise<IdentityProvider[]> => {
    const response = await apiClient.get<ApiResponse<IdentityProvider[]>>('/security/identity-providers');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch identity providers');
    return response.data.data;
  },

  listPublicProviders: async (organizationId: string): Promise<IdentityProvider[]> => {
    const response = await apiClient.get<ApiResponse<IdentityProvider[]>>('/auth/providers', {
      params: { organizationId },
    });
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch SSO providers');
    return response.data.data;
  },

  createIdentityProvider: async (data: CreateIdentityProviderRequest): Promise<IdentityProvider> => {
    const response = await apiClient.post<ApiResponse<IdentityProvider>>('/security/identity-providers', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to create identity provider');
    return response.data.data;
  },

  deleteIdentityProvider: async (id: string): Promise<void> => {
    await apiClient.delete(`/security/identity-providers/${id}`);
  },

  beginSso: async (data: BeginSsoRequest): Promise<SsoChallengeResponse> => {
    const response = await apiClient.post<ApiResponse<SsoChallengeResponse>>('/auth/sso/begin', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to begin SSO');
    return response.data.data;
  },

  completeSso: async (data: CompleteSsoRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/sso/complete', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to complete SSO');
    return response.data.data;
  },

  enrollMfa: async (): Promise<MfaEnrollmentResponse> => {
    const response = await apiClient.post<ApiResponse<MfaEnrollmentResponse>>('/auth/mfa', {
      action: 'enroll',
    } satisfies MfaRequest);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to enroll MFA');
    return response.data.data;
  },

  confirmMfa: async (code: string): Promise<void> => {
    await apiClient.post('/auth/mfa', { action: 'confirm', code } satisfies MfaRequest);
  },

  verifyMfa: async (code: string, mfaToken: string): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/mfa', {
      action: 'verify',
      code,
      mfaToken,
    } satisfies MfaRequest);
    if (!response.data.data) throw new Error(response.data.message || 'MFA verification failed');
    return response.data.data;
  },
};
