import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Row, Col, Spinner, Alert } from 'reactstrap';
import { organizationService } from '../services/organizationService';
import { OrganizationCard } from '../components/organizations/OrganizationCard';
import { useOrganization } from '../contexts/OrganizationContext';

export const OrganizationsPage = () => {
  const { currentOrganization } = useOrganization();

  const { data: organizations = [], isLoading, error } = useQuery({
    queryKey: ['organizations'],
    queryFn: organizationService.list,
  });

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Organizations</h1>
          <p className="text-muted mb-0">Manage your organizations and memberships.</p>
        </div>
        <Button tag={Link} to="/organizations/create" color="primary">
          Create Organization
        </Button>
      </div>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}

      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load organizations'}
        </Alert>
      )}

      {!isLoading && !error && organizations.length === 0 && (
        <Alert color="info">
          You don&apos;t belong to any organizations yet.{' '}
          <Link to="/organizations/create">Create one</Link> to get started.
        </Alert>
      )}

      {!isLoading && organizations.length > 0 && (
        <Row>
          {organizations.map((org) => (
            <Col key={org.id} md={6} lg={4} className="mb-4">
              <OrganizationCard
                organization={org}
                isCurrent={org.id === currentOrganization?.id}
              />
            </Col>
          ))}
        </Row>
      )}
    </div>
  );
};
