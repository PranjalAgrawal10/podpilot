const CURRENT_ORG_ID_KEY = 'podpilot_current_organization_id';
const CURRENT_ORG_ROLE_KEY = 'podpilot_current_organization_role';

export const organizationStorage = {
  getCurrentOrganizationId: (): string | null => localStorage.getItem(CURRENT_ORG_ID_KEY),
  setCurrentOrganizationId: (id: string): void => localStorage.setItem(CURRENT_ORG_ID_KEY, id),
  getCurrentOrganizationRole: (): string | null => localStorage.getItem(CURRENT_ORG_ROLE_KEY),
  setCurrentOrganizationRole: (role: string): void => localStorage.setItem(CURRENT_ORG_ROLE_KEY, role),
  clear: (): void => {
    localStorage.removeItem(CURRENT_ORG_ID_KEY);
    localStorage.removeItem(CURRENT_ORG_ROLE_KEY);
  },
};
