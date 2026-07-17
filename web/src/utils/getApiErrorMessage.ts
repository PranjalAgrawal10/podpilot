import axios from 'axios';
import type { ApiResponse } from '../types';

/** Extract a user-facing message from API / axios errors (including 403). */
export const getApiErrorMessage = (err: unknown, fallback: string): string => {
  if (axios.isAxiosError(err)) {
    const status = err.response?.status;
    const payload = err.response?.data as ApiResponse<unknown> | undefined;
    if (status === 403) {
      return payload?.message || 'You do not have permission to perform this action.';
    }
    if (payload?.message) {
      return payload.message;
    }
  }
  if (err instanceof Error && err.message) {
    return err.message;
  }
  return fallback;
};
