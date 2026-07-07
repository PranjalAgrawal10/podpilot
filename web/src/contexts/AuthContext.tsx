import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { authService, userService } from '../services/authService';
import { tokenStorage } from '../utils/tokenStorage';
import type { LoginRequest, RegisterRequest, UserResponse } from '../types';

interface AuthContextValue {
  user: UserResponse | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshUser = useCallback(async () => {
    const token = tokenStorage.getAccessToken();
    if (!token) {
      setUser(null);
      return;
    }

    const currentUser = await userService.getCurrentUser();
    setUser(currentUser);
  }, []);

  useEffect(() => {
    const init = async () => {
      try {
        if (tokenStorage.getAccessToken()) {
          await refreshUser();
        }
      } catch {
        tokenStorage.clear();
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    void init();
  }, [refreshUser]);

  const login = useCallback(async (data: LoginRequest) => {
    const auth = await authService.login(data);
    tokenStorage.setAccessToken(auth.accessToken);
    tokenStorage.setRefreshToken(auth.refreshToken);
    await refreshUser();
  }, [refreshUser]);

  const register = useCallback(async (data: RegisterRequest) => {
    const auth = await authService.register(data);
    tokenStorage.setAccessToken(auth.accessToken);
    tokenStorage.setRefreshToken(auth.refreshToken);
    await refreshUser();
  }, [refreshUser]);

  const logout = useCallback(async () => {
    const refreshToken = tokenStorage.getRefreshToken();
    if (refreshToken) {
      try {
        await authService.logout(refreshToken);
      } catch {
        // Continue with local logout even if API call fails
      }
    }
    tokenStorage.clear();
    setUser(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      isLoading,
      login,
      register,
      logout,
      refreshUser,
    }),
    [user, isLoading, login, register, logout, refreshUser],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
