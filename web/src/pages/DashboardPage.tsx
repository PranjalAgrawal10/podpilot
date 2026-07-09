import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardTitle, Col, Row, Badge, Table } from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import { useOrganization } from '../contexts/OrganizationContext';
import { healthService } from '../services/authService';
import { podService } from '../services/podService';
import { orchestratorService } from '../services/orchestratorService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { StatusBadge } from '../components/pods/StatusBadge';
import { GpuBadge } from '../components/pods/GpuBadge';
import { CostBadge } from '../components/pods/CostBadge';
import { RegionBadge } from '../components/pods/RegionBadge';
import { usePodStatusHub } from '../hooks/usePodStatusHub';
import { useOrchestratorHub } from '../hooks/useOrchestratorHub';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import { PERMISSIONS } from '../types';

export const DashboardPage = () => {
  const { user } = useAuth();
  const { currentOrganization, hasPermission } = useOrganization();
  const canReadPods = hasPermission(PERMISSIONS.PodRead);
  const canReadOrchestrator = hasPermission(PERMISSIONS.OrchestratorRead);
  const canReadObservability = hasPermission(PERMISSIONS.ObservabilityRead);

  usePodStatusHub(currentOrganization?.id);
  useOrchestratorHub(currentOrganization?.id);
  useObservabilityHub(currentOrganization?.id);

  const { data: health, isLoading: healthLoading } = useQuery({
    queryKey: ['health'],
    queryFn: healthService.getHealth,
    refetchInterval: 30000,
  });

  const { data: pods = [] } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id && canReadPods,
    refetchInterval: 30000,
  });

  const { data: orchestratorStatus } = useQuery({
    queryKey: ['orchestrator-status', currentOrganization?.id],
    queryFn: orchestratorService.getOrchestratorStatus,
    enabled: !!currentOrganization?.id && canReadOrchestrator,
    refetchInterval: 15000,
  });

  const { data: capacity } = useQuery({
    queryKey: ['capacity', currentOrganization?.id, 'all'],
    queryFn: () => orchestratorService.getCapacity(),
    enabled: !!currentOrganization?.id && canReadOrchestrator,
    refetchInterval: 15000,
  });

  const { data: scalingEvents = [] } = useQuery({
    queryKey: ['scaling-events', currentOrganization?.id],
    queryFn: () => orchestratorService.listScalingEvents(undefined, 5),
    enabled: !!currentOrganization?.id && canReadOrchestrator,
    refetchInterval: 15000,
  });

  const { data: liveMetrics } = useQuery({
    queryKey: ['live-metrics', currentOrganization?.id],
    queryFn: observabilityService.getLiveMetrics,
    enabled: !!currentOrganization?.id && canReadObservability,
    refetchInterval: 12000,
  });

  const { data: observabilityCost } = useQuery({
    queryKey: ['cost', currentOrganization?.id, 'Daily'],
    queryFn: () => observabilityService.getCost({ period: 'Daily' }),
    enabled: !!currentOrganization?.id && canReadObservability,
    refetchInterval: 15000,
  });

  const { data: latestMetrics = [] } = useQuery({
    queryKey: ['metrics', currentOrganization?.id, 'latest'],
    queryFn: () => observabilityService.getMetrics({ limit: 1 }),
    enabled: !!currentOrganization?.id && canReadObservability,
    refetchInterval: 15000,
  });

  const latestSnapshot = latestMetrics[0];

  const runningPods = pods.filter((p) => p.status === 'Running');
  const stoppedPods = pods.filter((p) => p.status === 'Stopped');

  return (
    <div>
      <h1 className="page-title">Dashboard</h1>
      <p className="text-muted mb-4">
        Welcome back, {user?.firstName}! Monitor and manage your GPU infrastructure.
      </p>

      <Row className="g-4">
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">Running Pods</CardTitle>
              <p className="stat-value">{runningPods.length}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">Stopped Pods</CardTitle>
              <p className="stat-value">{stoppedPods.length}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">Organizations</CardTitle>
              <p className="stat-value">{user?.organizations.length ?? 0}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">System Status</CardTitle>
              {healthLoading ? (
                <LoadingSpinner />
              ) : (
                <Badge color={health?.status === 'Healthy' ? 'success' : 'danger'}>
                  {health?.status ?? 'Unknown'}
                </Badge>
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>

      {canReadOrchestrator && (
        <>
          <Row className="g-4 mt-2">
            <Col md={3}>
              <Card className="stat-card">
                <CardBody>
                  <CardTitle tag="h5">Healthy Pods</CardTitle>
                  <p className="stat-value">{orchestratorStatus?.healthyPods ?? 0}</p>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="stat-card">
                <CardBody>
                  <CardTitle tag="h5">Pool Capacity</CardTitle>
                  <p className="stat-value">
                    {capacity ? `${Math.round(capacity.currentCapacity * 100)}%` : '—'}
                  </p>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="stat-card">
                <CardBody>
                  <CardTitle tag="h5">Queue</CardTitle>
                  <p className="stat-value">{orchestratorStatus?.queueLength ?? 0}</p>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card className="stat-card">
                <CardBody>
                  <CardTitle tag="h5">Pod Pools</CardTitle>
                  <p className="stat-value">{orchestratorStatus?.poolCount ?? 0}</p>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Card className="mt-4">
            <CardBody>
              <div className="d-flex justify-content-between align-items-center mb-3">
                <CardTitle tag="h5" className="mb-0">
                  Recent Scaling Events
                </CardTitle>
                <Link to="/orchestration/scaling" className="btn btn-sm btn-outline-primary">
                  View Auto Scaling
                </Link>
              </div>
              {scalingEvents.length === 0 ? (
                <p className="text-muted mb-0">
                  No scaling events yet.{' '}
                  <Link to="/orchestration/pools">Configure pod pools</Link> to get started.
                </p>
              ) : (
                <Table responsive hover className="mb-0">
                  <thead>
                    <tr>
                      <th>Time</th>
                      <th>Direction</th>
                      <th>Reason</th>
                      <th>Pods</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {scalingEvents.map((event) => (
                      <tr key={event.id}>
                        <td className="small">{new Date(event.occurredAt).toLocaleString()}</td>
                        <td>{event.direction}</td>
                        <td>{event.reason}</td>
                        <td>
                          {event.podCountBefore} → {event.podCountAfter}
                        </td>
                        <td>
                          <Badge color={event.success ? 'success' : 'danger'}>
                            {event.success ? 'Success' : 'Failed'}
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </CardBody>
          </Card>
        </>
      )}

      {canReadObservability && (
        <Row className="g-4 mt-2">
          <Col md={12}>
            <div className="d-flex justify-content-between align-items-center mb-2">
              <h5 className="mb-0">Live Observability</h5>
              <Link to="/observability" className="btn btn-sm btn-outline-primary">
                View Observability
              </Link>
            </div>
          </Col>
          <Col md={2} sm={4}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6" className="text-muted">GPU Util</CardTitle>
                <p className="stat-value mb-0">
                  {(liveMetrics?.gpuUtilizationPercent ?? 0).toFixed(1)}%
                </p>
              </CardBody>
            </Card>
          </Col>
          <Col md={2} sm={4}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6" className="text-muted">VRAM</CardTitle>
                <p className="stat-value mb-0 small">
                  {latestSnapshot?.gpuMemoryUsedBytes != null && latestSnapshot.gpuMemoryTotalBytes != null
                    ? `${Math.round((latestSnapshot.gpuMemoryUsedBytes / latestSnapshot.gpuMemoryTotalBytes) * 100)}%`
                    : '—'}
                </p>
              </CardBody>
            </Card>
          </Col>
          <Col md={2} sm={4}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6" className="text-muted">Today's Cost</CardTitle>
                <p className="stat-value mb-0 small">
                  ${(observabilityCost?.dailyCost ?? 0).toFixed(2)}
                </p>
              </CardBody>
            </Card>
          </Col>
          <Col md={2} sm={4}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6" className="text-muted">Latency</CardTitle>
                <p className="stat-value mb-0">
                  {(liveMetrics?.averageLatencyMs ?? 0).toFixed(0)} ms
                </p>
              </CardBody>
            </Card>
          </Col>
          <Col md={2} sm={4}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6" className="text-muted">Active Requests</CardTitle>
                <p className="stat-value mb-0">{liveMetrics?.activeStreams ?? 0}</p>
              </CardBody>
            </Card>
          </Col>
        </Row>
      )}

      {canReadPods && (
        <Card className="mt-4">
          <CardBody>
            <div className="d-flex justify-content-between align-items-center mb-3">
              <CardTitle tag="h5" className="mb-0">GPU Pods</CardTitle>
              <Link to="/pods" className="btn btn-sm btn-outline-primary">View All</Link>
            </div>
            {pods.length === 0 ? (
              <p className="text-muted mb-0">No pods yet. <Link to="/pods/create">Create a pod</Link> to get started.</p>
            ) : (
              <Table responsive hover className="mb-0">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Status</th>
                    <th>GPU</th>
                    <th>Region</th>
                    <th>Provider</th>
                    <th>Cost</th>
                    <th>Last Updated</th>
                  </tr>
                </thead>
                <tbody>
                  {pods.slice(0, 10).map((pod) => (
                    <tr key={pod.id}>
                      <td><Link to={`/pods/${pod.id}`}>{pod.name}</Link></td>
                      <td><StatusBadge status={pod.status} /></td>
                      <td><GpuBadge gpuType={pod.gpuType} /></td>
                      <td><RegionBadge region={pod.region} /></td>
                      <td>{pod.providerName}</td>
                      <td><CostBadge hourlyCost={pod.hourlyCost} /></td>
                      <td className="small text-muted">
                        {pod.lastSyncedAt ? new Date(pod.lastSyncedAt).toLocaleString() : '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            )}
          </CardBody>
        </Card>
      )}
    </div>
  );
};
