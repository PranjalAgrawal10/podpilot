import { Link, useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  CardHeader,
  Col,
  Progress,
  Row,
  Spinner,
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { deploymentService } from '../services/deploymentService';
import { useDeploymentHub } from '../hooks/useDeploymentHub';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS } from '../types';

const statusColor = (status: string): string => {
  switch (status) {
    case 'Ready':
    case 'Running':
      return 'success';
    case 'Failed':
    case 'Cancelled':
      return 'danger';
    case 'DownloadingModels':
    case 'Provisioning':
    case 'Starting':
    case 'InstallingRuntime':
    case 'Configuring':
    case 'HealthCheck':
      return 'info';
    case 'Deleting':
      return 'warning';
    default:
      return 'secondary';
  }
};

export const DeploymentDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.DeploymentRead);
  const canManage = hasPermission(PERMISSIONS.DeploymentManage);
  useDeploymentHub(currentOrganization?.id, id);

  const { data: deployment, isLoading, error } = useQuery({
    queryKey: ['deployment', id],
    queryFn: () => deploymentService.getById(id!),
    enabled: !!id && canRead,
    refetchInterval: 15000,
  });

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['deployment', id] });
    void queryClient.invalidateQueries({ queryKey: ['deployments', currentOrganization?.id] });
    void queryClient.invalidateQueries({ queryKey: ['deployment-dashboard', currentOrganization?.id] });
  };

  const restartMutation = useMutation({
    mutationFn: () => deploymentService.restart(id!),
    onSuccess: () => {
      toast.success('Restart requested');
      invalidate();
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Failed to restart')),
  });

  const healthMutation = useMutation({
    mutationFn: () => deploymentService.runHealthCheck(id!),
    onSuccess: () => {
      toast.success('Health check completed');
      invalidate();
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Health check failed')),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deploymentService.delete(id!),
    onSuccess: () => {
      toast.success('Deployment deletion requested');
      navigate('/deployments');
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Failed to delete')),
  });

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view deployments.</Alert>;
  }

  if (isLoading) {
    return <div className="text-center py-5"><Spinner /></div>;
  }

  if (error || !deployment) {
    return (
      <Alert color="danger">
        {getApiErrorMessage(error, 'Deployment not found')}
      </Alert>
    );
  }

  const health = deployment.health;

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">{deployment.name}</h1>
          <p className="text-muted mb-0">
            <Link to="/deployments">Deployments</Link> / {deployment.name}
          </p>
        </div>
        <div className="d-flex flex-wrap gap-2 align-items-center">
          <Badge color={statusColor(deployment.status)}>{deployment.status}</Badge>
          <Badge color="secondary">{deployment.healthState || 'Unknown'}</Badge>
          <Button tag={Link} to={`/deployments/${deployment.id}/logs`} color="secondary" outline size="sm">
            Logs
          </Button>
          {canManage && (
            <>
              <Button
                color="primary"
                outline
                size="sm"
                disabled={restartMutation.isPending}
                onClick={() => restartMutation.mutate()}
              >
                Restart
              </Button>
              <Button
                color="info"
                outline
                size="sm"
                disabled={healthMutation.isPending}
                onClick={() => healthMutation.mutate()}
              >
                Health check
              </Button>
              <Button
                color="danger"
                outline
                size="sm"
                disabled={deleteMutation.isPending}
                onClick={() => {
                  if (window.confirm(`Delete deployment "${deployment.name}"?`)) {
                    deleteMutation.mutate();
                  }
                }}
              >
                Delete
              </Button>
            </>
          )}
        </div>
      </div>

      {deployment.errorMessage && (
        <Alert color="danger">{deployment.errorMessage}</Alert>
      )}

      <Row className="g-4 mb-4">
        <Col md={8}>
          <Card className="mb-4">
            <CardHeader>Progress</CardHeader>
            <CardBody>
              <p className="mb-2">{deployment.statusMessage || deployment.status}</p>
              <Progress value={deployment.progressPercent} className="mb-2">
                {deployment.progressPercent}%
              </Progress>
            </CardBody>
          </Card>

          <Card className="mb-4">
            <CardHeader>Models</CardHeader>
            <CardBody className="p-0">
              <Table responsive hover className="mb-0">
                <thead>
                  <tr>
                    <th>Model</th>
                    <th>Status</th>
                    <th>Progress</th>
                    <th>Primary</th>
                  </tr>
                </thead>
                <tbody>
                  {deployment.models.length === 0 && (
                    <tr><td colSpan={4} className="text-muted">No models</td></tr>
                  )}
                  {deployment.models.map((m) => (
                    <tr key={m.id}>
                      <td>{m.modelReference}</td>
                      <td>
                        <Badge color={statusColor(m.downloadStatus)}>{m.downloadStatus}</Badge>
                        {m.errorMessage && <div className="text-danger small">{m.errorMessage}</div>}
                      </td>
                      <td style={{ minWidth: 120 }}>
                        <Progress value={m.progressPercent} style={{ height: 8 }} />
                        <span className="small text-muted">{m.progressPercent}%</span>
                      </td>
                      <td>{m.isPrimary ? 'Yes' : '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>Recent logs</CardHeader>
            <CardBody className="p-0">
              <Table responsive size="sm" className="mb-0">
                <thead>
                  <tr>
                    <th>Time</th>
                    <th>Level</th>
                    <th>Stage</th>
                    <th>Message</th>
                  </tr>
                </thead>
                <tbody>
                  {(deployment.logs ?? []).slice(0, 15).map((log) => (
                    <tr key={log.id}>
                      <td className="text-nowrap small">{new Date(log.timestampUtc).toLocaleString()}</td>
                      <td><Badge color={log.level === 'Error' ? 'danger' : 'secondary'}>{log.level}</Badge></td>
                      <td>{log.stage}</td>
                      <td>{log.message}</td>
                    </tr>
                  ))}
                  {(deployment.logs ?? []).length === 0 && (
                    <tr><td colSpan={4} className="text-muted">No logs yet</td></tr>
                  )}
                </tbody>
              </Table>
            </CardBody>
          </Card>
        </Col>

        <Col md={4}>
          <Card className="mb-4">
            <CardHeader>Overview</CardHeader>
            <CardBody>
              <dl className="mb-0">
                <dt>Runtime</dt>
                <dd>{deployment.runtime}</dd>
                <dt>GPU</dt>
                <dd>{deployment.gpuCode}</dd>
                <dt>Region</dt>
                <dd>{deployment.region}</dd>
                <dt>Cloud</dt>
                <dd>{deployment.cloudProvider}</dd>
                <dt>CUDA</dt>
                <dd>{deployment.cudaVersion}</dd>
                <dt>Image</dt>
                <dd className="small">{deployment.imageName || '—'}</dd>
                <dt>Cost / hr</dt>
                <dd>${deployment.estimatedHourlyCostUsd.toFixed(2)}</dd>
                <dt>Ready at</dt>
                <dd>{deployment.readyAt ? new Date(deployment.readyAt).toLocaleString() : '—'}</dd>
              </dl>
              <div className="d-flex flex-column gap-2 mt-3">
                {deployment.gpuPodId && (
                  <Button tag={Link} to={`/pods/${deployment.gpuPodId}`} color="link" className="p-0 text-start">
                    View GPU pod
                  </Button>
                )}
                {deployment.gatewayRouteId && (
                  <Button tag={Link} to="/gateway" color="link" className="p-0 text-start">
                    Open AI Gateway
                  </Button>
                )}
              </div>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>Health</CardHeader>
            <CardBody>
              {!health && <p className="text-muted mb-0">No health snapshot yet.</p>}
              {health && (
                <ul className="list-unstyled mb-0">
                  <li>State: <Badge color={health.state === 'Healthy' ? 'success' : 'warning'}>{health.state}</Badge></li>
                  <li>GPU: {health.gpuAvailable ? '✓' : '✗'}</li>
                  <li>CUDA: {health.cudaAvailable ? '✓' : '✗'}</li>
                  <li>Runtime: {health.runtimeRunning ? '✓' : '✗'}</li>
                  <li>Model: {health.modelAvailable ? '✓' : '✗'}</li>
                  <li>Gateway: {health.gatewayReachable ? '✓' : '✗'}</li>
                  <li>Streaming: {health.streamingWorks ? '✓' : '✗'}</li>
                  <li className="text-muted small mt-2">
                    Last check:{' '}
                    {health.lastCheckedAt ? new Date(health.lastCheckedAt).toLocaleString() : '—'}
                  </li>
                </ul>
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
