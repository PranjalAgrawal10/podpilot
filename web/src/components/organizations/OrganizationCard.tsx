import { Link } from 'react-router-dom';
import { Card, CardBody, Button } from 'reactstrap';
import { RoleBadge } from '../common/RoleBadge';
import type { Organization } from '../../types';

interface OrganizationCardProps {
  organization: Organization;
  isCurrent?: boolean;
}

export const OrganizationCard = ({ organization, isCurrent = false }: OrganizationCardProps) => (
  <Card className={`org-card ${isCurrent ? 'org-card-current' : ''}`}>
    <CardBody>
      <div className="d-flex justify-content-between align-items-start mb-2">
        <div>
          <h5 className="mb-1">{organization.name}</h5>
          <small className="text-muted">{organization.slug}</small>
        </div>
        {organization.currentUserRole && <RoleBadge role={organization.currentUserRole} />}
      </div>
      {organization.description && (
        <p className="text-muted small mb-3">{organization.description}</p>
      )}
      <div className="d-flex gap-2">
        <Button
          tag={Link}
          to={`/organizations/${organization.id}/settings`}
          color="outline-primary"
          size="sm"
        >
          Settings
        </Button>
        {isCurrent && <span className="badge bg-success align-self-center">Current</span>}
      </div>
    </CardBody>
  </Card>
);
