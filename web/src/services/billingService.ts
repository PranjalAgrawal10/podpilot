import apiClient from './api';
import type {
  ApiResponse,
  CancelSubscriptionRequest,
  CheckoutSession,
  Invoice,
  Plan,
  StartCheckoutRequest,
  Subscription,
  UsageDashboard,
} from '../types';

export const billingService = {
  listPlans: async (): Promise<Plan[]> => {
    const response = await apiClient.get<ApiResponse<Plan[]>>('/billing/plans');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch plans');
    return response.data.data;
  },

  getSubscription: async (): Promise<Subscription> => {
    const response = await apiClient.get<ApiResponse<Subscription>>('/billing/subscription');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch subscription');
    return response.data.data;
  },

  ensureSubscription: async (): Promise<Subscription> => {
    const response = await apiClient.post<ApiResponse<Subscription>>('/billing/subscription');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to ensure subscription');
    return response.data.data;
  },

  startCheckout: async (data: StartCheckoutRequest): Promise<CheckoutSession> => {
    const response = await apiClient.post<ApiResponse<CheckoutSession>>('/billing/checkout', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to start checkout');
    return response.data.data;
  },

  cancelSubscription: async (data?: CancelSubscriptionRequest): Promise<void> => {
    await apiClient.post('/billing/cancel', data ?? { atPeriodEnd: true });
  },

  getUsage: async (): Promise<UsageDashboard> => {
    const response = await apiClient.get<ApiResponse<UsageDashboard>>('/billing/usage');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch usage');
    return response.data.data;
  },

  listInvoices: async (): Promise<Invoice[]> => {
    const response = await apiClient.get<ApiResponse<Invoice[]>>('/billing/invoices');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch invoices');
    return response.data.data;
  },

  generateInvoice: async (): Promise<Invoice> => {
    const response = await apiClient.post<ApiResponse<Invoice>>('/billing/invoices');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to generate invoice');
    return response.data.data;
  },
};
