import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Col, Row, Spinner } from 'reactstrap';
import { aiProviderService } from '../services/aiProviderService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useAiProviderHub } from '../hooks/useAiProviderHub';
import { PERMISSIONS } from '../types';

export const AiProvidersPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.AiProviderRead);
  const canCreate = hasPermission(PERMISSIONS.AiProviderCreate);
  useAiProviderHub(currentOrganization?.id);

  const { data: providers = [], isLoading, error } = useQuery({
    queryKey: ['ai-providers', currentOrganization?.id],
    queryFn: aiProviderService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: dashboard } = useQuery({
    queryKey: ['ai-dashboard', currentOrganization?.id],
    queryFn: aiProviderService.getDashboard,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage AI providers.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view AI providers.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">AI Providers</h1>
          <p className="text-muted mb-0">Universal AI provider engine for {currentOrganization.name}.</p>
        </div>
        {canCreate && (
          <Button tag={Link} to="/ai/providers/add" color="primary">
            Add AI Provider
          </Button>
        )}
      </div>

      {dashboard && (
        <Row className="mb-4">
          <Col md={3}><Card><CardBody><div className="text-muted">Connected</div><h3>{dashboard.connectedProviders}/{dashboard.totalProviders}</h3></CardBody></Card></Col>
          <Col md={3}><Card><CardBody><div className="text-muted">Models</div><h3>{dashboard.availableModels}</h3></CardBody></Card></Col>
          <Col md={3}><Card><CardBody><div className="text-muted">Unhealthy</div><h3>{dashboard.unhealthyProviders}</h3></CardBody></Card></Col>
          <Col md={3}><Card><CardBody><div className="text-muted">Avg Latency</div><h3>{Math.round(dashboard.averageLatencyMs)} ms</h3></CardBody></Card></Col>
        </Row>
      )}

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load providers'}</Alert>}
      {!isLoading && !error && providers.length === 0 && (
        <Alert color="info">
          No AI providers yet. {canCreate && <Link to="/ai/providers/add">Add one</Link>}
        </Alert>
      )}

      <Row>
        {providers.map((provider) => (
          <Col key={provider.id} md={6} lg={4} className="mb-4">
            <Card className="h-100">
              <CardBody>
                <div className="d-flex justify-content-between mb-2">
                  <h5 className="mb-0">{provider.displayName}</h5>
                  <Badge color={provider.isValidated ? 'success' : 'secondary'}>{provider.providerKind}</Badge>
                </div>
                <p className="text-muted small mb-3">{provider.name}</p>
                <Button tag={Link} to={`/ai/providers/${provider.id}`} color="link" className="p-0">
                  View details
                </Button>
              </CardBody>
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  );
};
