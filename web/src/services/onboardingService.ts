import apiClient from './api';
import type { ApiResponse, CompleteOnboardingStepRequest, OnboardingStatus } from '../types';

export const onboardingService = {
  get: async (): Promise<OnboardingStatus> => {
    const response = await apiClient.get<ApiResponse<OnboardingStatus>>('/onboarding');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch onboarding');
    return response.data.data;
  },

  completeStep: async (data: CompleteOnboardingStepRequest): Promise<OnboardingStatus> => {
    const response = await apiClient.post<ApiResponse<OnboardingStatus>>(
      '/onboarding/steps/complete',
      data,
    );
    if (!response.data.data) throw new Error(response.data.message || 'Failed to complete step');
    return response.data.data;
  },

  dismiss: async (): Promise<void> => {
    await apiClient.post('/onboarding/dismiss');
  },
};
