import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardTitle, Col, Row, Badge, Table } from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import { useOrganization } from '../contexts/OrganizationContext';
import { healthService } from '../services/authService';
import { podService } from '../services/podService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { StatusBadge } from '../components/pods/StatusBadge';
import { GpuBadge } from '../components/pods/GpuBadge';
import { CostBadge } from '../components/pods/CostBadge';
import { RegionBadge } from '../components/pods/RegionBadge';
import { usePodStatusHub } from '../hooks/usePodStatusHub';
import { PERMISSIONS } from '../types';

export const DashboardPage = () => {
  const { user } = useAuth();
  const { currentOrganization, hasPermission } = useOrganization();
  const canReadPods = hasPermission(PERMISSIONS.PodRead);

  usePodStatusHub(currentOrganization?.id);

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
