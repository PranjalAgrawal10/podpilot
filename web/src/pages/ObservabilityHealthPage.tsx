import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Card, CardBody, CardTitle, Col, Row, Table } from 'reactstrap';
import { HealthStatusBadge } from '../components/observability/HealthStatusBadge';
import { MetricCard } from '../components/observability/MetricCard';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { useOrganization } from '../contexts/OrganizationContext';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import { PERMISSIONS } from '../types';

const componentBadge = (healthy: boolean) => (
  <Badge color={healthy ? 'success' : 'danger'}>{healthy ? 'OK' : 'Issue'}</Badge>
);

export const ObservabilityHealthPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);

  useObservabilityHub(currentOrganization?.id);

  const { data: systemHealth, isLoading: systemLoading } = useQuery({
    queryKey: ['system-health', currentOrganization?.id],
    queryFn: observabilityService.getSystemHealth,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const { data: podHealth, isLoading: podLoading } = useQuery({
    queryKey: ['pod-health-overview', currentOrganization?.id],
    queryFn: observabilityService.getPodHealth,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const { data: providerHealth, isLoading: providerLoading } = useQuery({
    queryKey: ['provider-health-overview', currentOrganization?.id],
    queryFn: observabilityService.getProviderHealth,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view health.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view health status.</Alert>;
  }

  if ((systemLoading || podLoading || providerLoading) && !systemHealth && !podHealth && !providerHealth) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <h1 className="page-title">Health</h1>
      <p className="text-muted mb-4">
        System, pod, and provider health for your organization.
      </p>

      <Row className="g-4 mb-4">
        <Col md={3} sm={6}>
          <MetricCard
            title="System Status"
            value={systemHealth?.overallStatus ?? 'Unknown'}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Healthy Pods"
            value={`${podHealth?.healthyPods ?? 0} / ${podHealth?.totalPods ?? 0}`}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Degraded Pods" value={podHealth?.degradedPods ?? 0} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Healthy Providers"
            value={`${providerHealth?.healthyProviders ?? 0} / ${providerHealth?.totalProviders ?? 0}`}
          />
        </Col>
      </Row>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">System Components</CardTitle>
          {(systemHealth?.components ?? []).length === 0 ? (
            <Alert color="info" className="mb-0">No component health data available.</Alert>
          ) : (
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Component</th>
                  <th>Status</th>
                  <th>Message</th>
                </tr>
              </thead>
              <tbody>
                {(systemHealth?.components ?? []).map((component) => (
                  <tr key={component.component}>
                    <td>{component.component}</td>
                    <td>
                      <HealthStatusBadge status={component.status} />
                    </td>
                    <td>{component.message}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>

      <Row className="g-4">
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Pod Health</CardTitle>
              {(podHealth?.pods ?? []).length === 0 ? (
                <Alert color="info" className="mb-0">No pod health data.</Alert>
              ) : (
                <Table responsive hover className="mb-0">
                  <thead>
                    <tr>
                      <th>Pod</th>
                      <th>Status</th>
                      <th>GPU</th>
                      <th>Ollama</th>
                      <th>Models</th>
                      <th>Latency</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(podHealth?.pods ?? []).map((pod) => (
                      <tr key={pod.podId}>
                        <td>{pod.podName}</td>
                        <td>
                          <HealthStatusBadge status={pod.status} />
                        </td>
                        <td>{componentBadge(pod.gpuHealthy)}</td>
                        <td>{componentBadge(pod.ollamaHealthy)}</td>
                        <td>{componentBadge(pod.modelsHealthy)}</td>
                        <td>{pod.latencyMs} ms</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Provider Health</CardTitle>
              {(providerHealth?.providers ?? []).length === 0 ? (
                <Alert color="info" className="mb-0">No provider health data.</Alert>
              ) : (
                <Table responsive hover className="mb-0">
                  <thead>
                    <tr>
                      <th>Provider</th>
                      <th>Status</th>
                      <th>Response Time</th>
                      <th>Last Checked</th>
                    </tr>
                  </thead>
                  <tbody>
                    {(providerHealth?.providers ?? []).map((provider) => (
                      <tr key={provider.providerId}>
                        <td>{provider.providerName}</td>
                        <td>
                          <HealthStatusBadge status={provider.status} />
                        </td>
                        <td>{provider.responseTimeMs != null ? `${provider.responseTimeMs} ms` : '—'}</td>
                        <td className="small text-muted">
                          {provider.lastCheckedAt
                            ? new Date(provider.lastCheckedAt).toLocaleString()
                            : '—'}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
