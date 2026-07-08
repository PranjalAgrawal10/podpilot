import { useState } from 'react';
import {
  Dropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
  Spinner,
} from 'reactstrap';
import { useAuth } from '../../contexts/AuthContext';
import { useOrganization } from '../../contexts/OrganizationContext';
import { toast } from 'react-toastify';

export const OrganizationSwitcher = () => {
  const { user } = useAuth();
  const { currentOrganization, isSwitching, switchOrganization } = useOrganization();
  const [isOpen, setIsOpen] = useState(false);

  if (!user || user.organizations.length === 0) {
    return null;
  }

  const handleSwitch = async (organizationId: string) => {
    if (organizationId === currentOrganization?.id) {
      setIsOpen(false);
      return;
    }

    try {
      await switchOrganization(organizationId);
      toast.success('Organization switched');
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to switch organization';
      toast.error(message);
    } finally {
      setIsOpen(false);
    }
  };

  return (
    <Dropdown isOpen={isOpen} toggle={() => setIsOpen(!isOpen)} className="org-switcher">
      <DropdownToggle caret color="light" className="org-switcher-toggle" disabled={isSwitching}>
        {isSwitching ? (
          <Spinner size="sm" />
        ) : (
          currentOrganization?.name ?? 'Select organization'
        )}
      </DropdownToggle>
      <DropdownMenu end>
        {user.organizations.map((org) => (
          <DropdownItem
            key={org.id}
            active={org.id === currentOrganization?.id}
            onClick={() => void handleSwitch(org.id)}
          >
            <div className="org-switcher-item">
              <strong>{org.name}</strong>
              <small className="text-muted d-block">{org.role}</small>
            </div>
          </DropdownItem>
        ))}
      </DropdownMenu>
    </Dropdown>
  );
};
