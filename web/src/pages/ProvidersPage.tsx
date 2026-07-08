import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Button, Row, Col, Spinner, Alert } from 'reactstrap';
import { providerService } from '../services/providerService';
import { ProviderCard } from '../components/providers/ProviderCard';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const ProvidersPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ProviderRead);
  const canCreate = hasPermission(PERMISSIONS.ProviderCreate);

  const { data: providers = [], isLoading, error } = useQuery({
    queryKey: ['providers', currentOrganization?.id],
    queryFn: providerService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return (
      <Alert color="info">
        Select an organization to view providers, or create one from the Organizations page.
      </Alert>
    );
  }

  if (!canRead) {
    return (
      <Alert color="warning">
        You don&apos;t have permission to view providers in this organization.
      </Alert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Providers</h1>
          <p className="text-muted mb-0">
            Manage GPU compute providers for {currentOrganization.name}.
          </p>
        </div>
        {canCreate && (
          <Button tag={Link} to="/providers/add" color="primary">
            Add Provider
          </Button>
        )}
      </div>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}

      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load providers'}
        </Alert>
      )}

      {!isLoading && !error && providers.length === 0 && (
        <Alert color="info">
          No providers configured yet.{' '}
          {canCreate && (
            <>
              <Link to="/providers/add">Add a provider</Link> to get started.
            </>
          )}
        </Alert>
      )}

      {!isLoading && providers.length > 0 && (
        <Row>
          {providers.map((provider) => (
            <Col key={provider.id} md={6} lg={4} className="mb-4">
              <ProviderCard provider={provider} />
            </Col>
          ))}
        </Row>
      )}
    </div>
  );
};
