import { Link, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Spinner, Table } from 'reactstrap';
import { deploymentService } from '../services/deploymentService';
import { useDeploymentHub } from '../hooks/useDeploymentHub';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS } from '../types';

export const DeploymentLogsPage = () => {
  const { id } = useParams<{ id: string }>();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.DeploymentRead);
  useDeploymentHub(currentOrganization?.id, id);

  const { data: deployment, isLoading, error, refetch, isFetching } = useQuery({
    queryKey: ['deployment', id],
    queryFn: () => deploymentService.getById(id!),
    enabled: !!id && canRead,
    refetchInterval: 10000,
  });

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view deployments.</Alert>;
  }

  if (isLoading) {
    return <div className="text-center py-5"><Spinner /></div>;
  }

  if (error || !deployment) {
    return <Alert color="danger">{getApiErrorMessage(error, 'Deployment not found')}</Alert>;
  }

  const logs = [...(deployment.logs ?? [])].sort(
    (a, b) => new Date(b.timestampUtc).getTime() - new Date(a.timestampUtc).getTime(),
  );

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Deployment logs</h1>
          <p className="text-muted mb-0">
            <Link to="/deployments">Deployments</Link>
            {' / '}
            <Link to={`/deployments/${deployment.id}`}>{deployment.name}</Link>
            {' / Logs'}
          </p>
        </div>
        <Button color="secondary" outline size="sm" disabled={isFetching} onClick={() => void refetch()}>
          Refresh
        </Button>
      </div>

      <Card>
        <CardBody className="p-0">
          <Table responsive hover className="mb-0">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Level</th>
                <th>Stage</th>
                <th>Message</th>
              </tr>
            </thead>
            <tbody>
              {logs.length === 0 && (
                <tr>
                  <td colSpan={4} className="text-muted">No logs yet.</td>
                </tr>
              )}
              {logs.map((log) => (
                <tr key={log.id}>
                  <td className="text-nowrap small">{new Date(log.timestampUtc).toLocaleString()}</td>
                  <td>
                    <Badge
                      color={
                        log.level === 'Error' ? 'danger' : log.level === 'Warning' ? 'warning' : 'secondary'
                      }
                    >
                      {log.level}
                    </Badge>
                  </td>
                  <td>{log.stage}</td>
                  <td>{log.message}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>
    </div>
  );
};
