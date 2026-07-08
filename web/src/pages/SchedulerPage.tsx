import { Link } from 'react-router-dom';
import { Card, CardBody, CardTitle, Col, Row, Table, Badge, Button } from 'reactstrap';
import { useSchedulerHub } from '../hooks/useSchedulerHub';
import { schedulerService } from '../services/schedulerService';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useOrganization } from '../contexts/OrganizationContext';
import { toast } from 'react-toastify';

const statusColor = (status: string) => {
  switch (status) {
    case 'Completed':
      return 'success';
    case 'Failed':
    case 'TimedOut':
      return 'danger';
    case 'Queued':
      return 'warning';
    case 'Streaming':
    case 'Forwarding':
      return 'info';
    default:
      return 'secondary';
  }
};

export const SchedulerPage = () => {
  const { queueQuery, statusQuery, requestsQuery } = useSchedulerHub();
  const { currentOrganization } = useOrganization();
  const queryClient = useQueryClient();

  const cancelMutation = useMutation({
    mutationFn: schedulerService.cancelRequest,
    onSuccess: () => {
      toast.success('Request cancelled');
      queryClient.invalidateQueries({ queryKey: ['scheduler-requests', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['scheduler-queue', currentOrganization?.id] });
    },
    onError: () => toast.error('Failed to cancel request'),
  });

  const queue = queueQuery.data;
  const status = statusQuery.data;
  const requests = requestsQuery.data ?? [];

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Request Scheduler</h2>
        <div>
          <Link to="/scheduler/queue" className="btn btn-outline-primary me-2">Queue</Link>
          <Link to="/scheduler/requests" className="btn btn-outline-primary">Requests</Link>
        </div>
      </div>

      <Row className="mb-4">
        <Col md={3}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Queue Length</CardTitle>
              <h3>{queue?.queueLength ?? 0}</h3>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Running</CardTitle>
              <h3>{queue?.runningRequests ?? 0}</h3>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Avg Wait (ms)</CardTitle>
              <h3>{Math.round(queue?.averageWaitTimeMs ?? 0)}</h3>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card>
            <CardBody>
              <CardTitle tag="h6">Pod Utilization</CardTitle>
              <h3>{status?.podUtilizationPercent ?? 0}%</h3>
            </CardBody>
          </Card>
        </Col>
      </Row>

      <Card>
        <CardBody>
          <CardTitle tag="h5">Recent Requests</CardTitle>
          <Table responsive hover>
            <thead>
              <tr>
                <th>Model</th>
                <th>Path</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Wait</th>
                <th>Exec</th>
                <th>Retries</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {requests.slice(0, 20).map((request) => (
                <tr key={request.id}>
                  <td>{request.model ?? '-'}</td>
                  <td><Link to={`/scheduler/requests/${request.id}`}>{request.path}</Link></td>
                  <td><Badge color={statusColor(request.status)}>{request.status}</Badge></td>
                  <td>{request.priority}</td>
                  <td>{request.queueTimeMs ?? '-'}</td>
                  <td>{request.executionTimeMs ?? '-'}</td>
                  <td>{request.retryCount}</td>
                  <td>
                    {request.status === 'Queued' && (
                      <Button size="sm" color="danger" outline onClick={() => cancelMutation.mutate(request.id)}>
                        Cancel
                      </Button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>
    </div>
  );
};
