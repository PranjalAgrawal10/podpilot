import apiClient from './api';
import type {
  AcceptInvitationRequest,
  ApiResponse,
  Invitation,
  InviteMemberRequest,
  Member,
} from '../types';

export const invitationService = {
  invite: async (organizationId: string, data: InviteMemberRequest): Promise<Invitation> => {
    const response = await apiClient.post<ApiResponse<Invitation>>(
      `/organizations/${organizationId}/invite`,
      data,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to send invitation');
    }
    return response.data.data;
  },

  accept: async (data: AcceptInvitationRequest): Promise<Member> => {
    const response = await apiClient.post<ApiResponse<Member>>('/organizations/accept', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to accept invitation');
    }
    return response.data.data;
  },
};
