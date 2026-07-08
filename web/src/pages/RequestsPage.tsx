import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Badge, Card, CardBody, CardTitle, Table } from 'reactstrap';
import { schedulerService } from '../services/schedulerService';
import { useOrganization } from '../contexts/OrganizationContext';

export const RequestsPage = () => {
  const { currentOrganization } = useOrganization();
  const { data: requests = [], isLoading } = useQuery({
    queryKey: ['scheduler-requests', currentOrganization?.id],
    queryFn: () => schedulerService.listRequests(),
    enabled: !!currentOrganization?.id,
    refetchInterval: 5000,
  });

  if (isLoading) {
    return <div>Loading requests...</div>;
  }

  return (
    <div>
      <h2 className="mb-4">Scheduler Requests</h2>
      <Card>
        <CardBody>
          <CardTitle tag="h5">All Requests</CardTitle>
          <Table responsive hover>
            <thead>
              <tr>
                <th>ID</th>
                <th>Model</th>
                <th>Status</th>
                <th>Priority</th>
                <th>Created</th>
              </tr>
            </thead>
            <tbody>
              {requests.map((request) => (
                <tr key={request.id}>
                  <td><Link to={`/scheduler/requests/${request.id}`}>{request.id.slice(0, 8)}</Link></td>
                  <td>{request.model ?? '-'}</td>
                  <td><Badge color="info">{request.status}</Badge></td>
                  <td>{request.priority}</td>
                  <td>{new Date(request.createdAt).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>
    </div>
  );
};
