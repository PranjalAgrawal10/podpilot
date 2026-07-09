import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Card,
  CardBody,
  CardTitle,
  Col,
  FormGroup,
  Input,
  Label,
  Row,
  Table,
} from 'reactstrap';
import { useOrganization } from '../contexts/OrganizationContext';
import { useOrchestratorHub } from '../hooks/useOrchestratorHub';
import { orchestratorService } from '../services/orchestratorService';
import { podService } from '../services/podService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS } from '../types';

const healthBadge = (healthy: boolean) => (
  <Badge color={healthy ? 'success' : 'danger'}>{healthy ? 'Healthy' : 'Unhealthy'}</Badge>
);

const formatBytes = (bytes?: number | null) => {
  if (bytes == null) return '—';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
};

export const HealthPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.OrchestratorRead);
  const [selectedPodId, setSelectedPodId] = useState('');

  useOrchestratorHub(currentOrganization?.id);

  const { data: pods = [] } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: metrics = [], isLoading, error } = useQuery({
    queryKey: ['pod-health-metrics', currentOrganization?.id, selectedPodId || 'all'],
    queryFn: () => orchestratorService.listPodHealthMetrics(selectedPodId || undefined, 100),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  const podNameById = Object.fromEntries(pods.map((pod) => [pod.id, pod.name]));

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view pod health.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view pod health metrics.</Alert>;
  }

  const healthyCount = metrics.filter(
    (m) => m.gpuHealthy && m.ollamaHealthy && m.modelsHealthy && m.networkHealthy,
  ).length;

  return (
    <div>
      <h1 className="page-title">Pod Health</h1>
      <p className="text-muted mb-4">
        Real-time health metrics for pods in your orchestration pools.
      </p>

      <Card className="mb-4">
        <CardBody>
          <FormGroup>
            <Label for="healthPod">Filter by pod</Label>
            <Input
              id="healthPod"
              type="select"
              value={selectedPodId}
              onChange={(e) => setSelectedPodId(e.target.value)}
            >
              <option value="">All pods</option>
              {pods.map((pod) => (
                <option key={pod.id} value={pod.id}>
                  {pod.name}
                </option>
              ))}
            </Input>
          </FormGroup>
        </CardBody>
      </Card>

      <Row className="g-4 mb-4">
        <Col md={4}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Metrics Recorded</CardTitle>
              <h3>{metrics.length}</h3>
            </CardBody>
          </Card>
        </Col>
        <Col md={4}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Fully Healthy</CardTitle>
              <h3>{healthyCount}</h3>
            </CardBody>
          </Card>
        </Col>
        <Col md={4}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Unhealthy</CardTitle>
              <h3>{metrics.length - healthyCount}</h3>
            </CardBody>
          </Card>
        </Col>
      </Row>

      {isLoading && <LoadingSpinner />}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load health metrics'}
        </Alert>
      )}

      {!isLoading && metrics.length === 0 && (
        <Alert color="info">No health metrics recorded yet.</Alert>
      )}

      {!isLoading && metrics.length > 0 && (
        <Card>
          <CardBody>
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Recorded</th>
                  <th>Pod</th>
                  <th>State</th>
                  <th>GPU</th>
                  <th>Ollama</th>
                  <th>Models</th>
                  <th>Network</th>
                  <th>Latency</th>
                  <th>GPU %</th>
                  <th>Memory</th>
                </tr>
              </thead>
              <tbody>
                {metrics.map((metric) => (
                  <tr key={metric.id}>
                    <td className="small">{new Date(metric.recordedAt).toLocaleString()}</td>
                    <td>
                      <Link to={`/pods/${metric.gpuPodId}`}>
                        {podNameById[metric.gpuPodId] ?? metric.gpuPodId.slice(0, 8)}
                      </Link>
                    </td>
                    <td>{metric.state}</td>
                    <td>{healthBadge(metric.gpuHealthy)}</td>
                    <td>{healthBadge(metric.ollamaHealthy)}</td>
                    <td>{healthBadge(metric.modelsHealthy)}</td>
                    <td>{healthBadge(metric.networkHealthy)}</td>
                    <td>{metric.latencyMs} ms</td>
                    <td>
                      {metric.gpuUtilizationPercent != null
                        ? `${metric.gpuUtilizationPercent.toFixed(1)}%`
                        : '—'}
                    </td>
                    <td>{formatBytes(metric.memoryUsedBytes)}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </CardBody>
        </Card>
      )}
    </div>
  );
};
