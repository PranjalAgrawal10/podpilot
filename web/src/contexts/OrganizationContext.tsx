import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { useAuth } from './AuthContext';
import { organizationService } from '../services/organizationService';
import { hasPermission, parseOrganizationRole } from '../utils/permissions';
import { organizationStorage } from '../utils/organizationStorage';
import type { OrganizationRole, OrganizationSummary, Permission } from '../types';

interface OrganizationContextValue {
  currentOrganization: OrganizationSummary | null;
  currentRole: OrganizationRole | null;
  isSwitching: boolean;
  switchOrganization: (organizationId: string) => Promise<void>;
  hasPermission: (permission: Permission) => boolean;
}

const OrganizationContext = createContext<OrganizationContextValue | undefined>(undefined);

const resolveCurrentOrganization = (
  organizations: OrganizationSummary[],
): OrganizationSummary | null => {
  if (organizations.length === 0) {
    return null;
  }

  const storedId = organizationStorage.getCurrentOrganizationId();
  if (storedId) {
    const storedOrg = organizations.find((org) => org.id === storedId);
    if (storedOrg) {
      return storedOrg;
    }
  }

  return organizations[0];
};

export const OrganizationProvider = ({ children }: { children: ReactNode }) => {
  const { user, applyAuthResponse } = useAuth();
  const [currentOrganization, setCurrentOrganization] = useState<OrganizationSummary | null>(null);
  const [currentRole, setCurrentRole] = useState<OrganizationRole | null>(null);
  const [isSwitching, setIsSwitching] = useState(false);

  const syncFromUser = useCallback((organizations: OrganizationSummary[]) => {
    const org = resolveCurrentOrganization(organizations);
    setCurrentOrganization(org);

    if (org) {
      organizationStorage.setCurrentOrganizationId(org.id);
      organizationStorage.setCurrentOrganizationRole(org.role);
      setCurrentRole(parseOrganizationRole(org.role));
    } else {
      organizationStorage.clear();
      setCurrentRole(null);
    }
  }, []);

  useEffect(() => {
    if (user) {
      syncFromUser(user.organizations);
    } else {
      setCurrentOrganization(null);
      setCurrentRole(null);
    }
  }, [user, syncFromUser]);

  const switchOrganization = useCallback(
    async (organizationId: string) => {
      setIsSwitching(true);
      try {
        const auth = await organizationService.switch({ organizationId });
        await applyAuthResponse(auth);
      } finally {
        setIsSwitching(false);
      }
    },
    [applyAuthResponse],
  );

  const checkPermission = useCallback(
    (permission: Permission) => hasPermission(currentRole, permission),
    [currentRole],
  );

  const value = useMemo(
    () => ({
      currentOrganization,
      currentRole,
      isSwitching,
      switchOrganization,
      hasPermission: checkPermission,
    }),
    [currentOrganization, currentRole, isSwitching, switchOrganization, checkPermission],
  );

  return <OrganizationContext.Provider value={value}>{children}</OrganizationContext.Provider>;
};

export const useOrganization = (): OrganizationContextValue => {
  const context = useContext(OrganizationContext);
  if (!context) {
    throw new Error('useOrganization must be used within an OrganizationProvider');
  }
  return context;
};
