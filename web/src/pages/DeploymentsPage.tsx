import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  Col,
  Row,
  Spinner,
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { deploymentService } from '../services/deploymentService';
import { useDeploymentHub } from '../hooks/useDeploymentHub';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS, type Deployment } from '../types';

const statusColor = (status: string): string => {
  switch (status) {
    case 'Ready':
    case 'Running':
      return 'success';
    case 'Failed':
    case 'Deleted':
      return 'danger';
    case 'Downloading':
    case 'Provisioning':
    case 'Starting':
      return 'info';
    case 'Deleting':
      return 'warning';
    default:
      return 'secondary';
  }
};

const healthColor = (state: string): string => {
  switch (state) {
    case 'Healthy':
      return 'success';
    case 'Unhealthy':
    case 'Failed':
      return 'danger';
    case 'Degraded':
      return 'warning';
    default:
      return 'secondary';
  }
};

export const DeploymentsPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.DeploymentRead);
  const canManage = hasPermission(PERMISSIONS.DeploymentManage);
  useDeploymentHub(currentOrganization?.id);

  const { data: dashboard, isLoading: dashLoading, error: dashError } = useQuery({
    queryKey: ['deployment-dashboard', currentOrganization?.id],
    queryFn: deploymentService.getDashboard,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: deployments = [], isLoading, error } = useQuery({
    queryKey: ['deployments', currentOrganization?.id],
    queryFn: deploymentService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deploymentService.delete(id),
    onSuccess: () => {
      toast.success('Deployment deletion requested');
      void queryClient.invalidateQueries({ queryKey: ['deployments', currentOrganization?.id] });
      void queryClient.invalidateQueries({ queryKey: ['deployment-dashboard', currentOrganization?.id] });
    },
    onError: (err) => toast.error(getApiErrorMessage(err, 'Failed to delete deployment')),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view deployments.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view deployments.</Alert>;
  }

  const cards = [
    { label: 'Running', value: dashboard?.runningDeployments ?? 0, color: 'success' },
    { label: 'Downloading', value: dashboard?.downloadingModels ?? 0, color: 'info' },
    { label: 'Healthy', value: dashboard?.healthyDeployments ?? 0, color: 'primary' },
    {
      label: 'Est. monthly cost',
      value: `$${(dashboard?.estimatedMonthlyCostUsd ?? 0).toFixed(2)}`,
      color: 'secondary',
    },
  ];

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Deployments</h1>
          <p className="text-muted mb-0">
            One-click AI pods for {currentOrganization.name}.
          </p>
        </div>
        <div className="d-flex flex-wrap gap-2">
          <Button tag={Link} to="/deployments/templates" color="secondary" outline size="sm">
            Templates
          </Button>
          <Button tag={Link} to="/deployments/models" color="secondary" outline size="sm">
            Models
          </Button>
          <Button tag={Link} to="/deployments/gpus" color="secondary" outline size="sm">
            GPUs
          </Button>
          {canManage && (
            <Button tag={Link} to="/deployments/create" color="primary">
              Deploy AI Pod
            </Button>
          )}
        </div>
      </div>

      {(dashLoading || isLoading) && (
        <div className="text-center py-4">
          <Spinner />
        </div>
      )}

      {(dashError || error) && (
        <Alert color="danger">
          {getApiErrorMessage(dashError || error, 'Failed to load deployments')}
        </Alert>
      )}

      <Row className="mb-4">
        {cards.map((card) => (
          <Col key={card.label} md={6} lg={3} className="mb-3">
            <Card className="h-100">
              <CardBody>
                <p className="text-muted small mb-1">{card.label}</p>
                <h3 className="mb-0">{card.value}</h3>
              </CardBody>
            </Card>
          </Col>
        ))}
      </Row>

      {!isLoading && deployments.length === 0 && (
        <Alert color="info">
          No deployments yet.{' '}
          {canManage && <Link to="/deployments/create">Deploy your first AI pod</Link>}
        </Alert>
      )}

      {deployments.length > 0 && (
        <Card>
          <CardBody className="p-0">
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Status</th>
                  <th>Health</th>
                  <th>Runtime</th>
                  <th>GPU</th>
                  <th>Region</th>
                  <th>Cost/hr</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {deployments.map((d: Deployment) => (
                  <tr key={d.id}>
                    <td>
                      <Link to={`/deployments/${d.id}`}>{d.name}</Link>
                      {d.statusMessage && (
                        <div className="text-muted small">{d.statusMessage}</div>
                      )}
                    </td>
                    <td>
                      <Badge color={statusColor(d.status)}>{d.status}</Badge>
                      {d.progressPercent > 0 && d.progressPercent < 100 && (
                        <span className="text-muted small ms-2">{d.progressPercent}%</span>
                      )}
                    </td>
                    <td>
                      <Badge color={healthColor(d.healthState)}>{d.healthState || 'Unknown'}</Badge>
                    </td>
                    <td>{d.runtime}</td>
                    <td>{d.gpuCode}</td>
                    <td>{d.region}</td>
                    <td>${d.estimatedHourlyCostUsd.toFixed(2)}</td>
                    <td className="text-end">
                      <Button tag={Link} to={`/deployments/${d.id}`} color="link" size="sm">
                        Details
                      </Button>
                      {canManage && (
                        <Button
                          color="link"
                          size="sm"
                          className="text-danger"
                          disabled={deleteMutation.isPending}
                          onClick={() => {
                            if (window.confirm(`Delete deployment "${d.name}"?`)) {
                              deleteMutation.mutate(d.id);
                            }
                          }}
                        >
                          Delete
                        </Button>
                      )}
                    </td>
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
