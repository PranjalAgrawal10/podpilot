import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardTitle, Col, Row } from 'reactstrap';
import { schedulerService } from '../services/schedulerService';
import { useOrganization } from '../contexts/OrganizationContext';

export const QueuePage = () => {
  const { currentOrganization } = useOrganization();
  const { data: queue, isLoading } = useQuery({
    queryKey: ['scheduler-queue', currentOrganization?.id],
    queryFn: schedulerService.getQueue,
    enabled: !!currentOrganization?.id,
    refetchInterval: 3000,
  });

  if (isLoading) {
    return <div>Loading queue...</div>;
  }

  return (
    <div>
      <h2 className="mb-4">Request Queue</h2>
      <Row>
        <Col md={4}><Card><CardBody><CardTitle tag="h6">Queue Length</CardTitle><h2>{queue?.queueLength ?? 0}</h2></CardBody></Card></Col>
        <Col md={4}><Card><CardBody><CardTitle tag="h6">Running Requests</CardTitle><h2>{queue?.runningRequests ?? 0}</h2></CardBody></Card></Col>
        <Col md={4}><Card><CardBody><CardTitle tag="h6">Streaming</CardTitle><h2>{queue?.streamingRequests ?? 0}</h2></CardBody></Card></Col>
        <Col md={4}><Card><CardBody><CardTitle tag="h6">Failed (1h)</CardTitle><h2>{queue?.failedRequestsLastHour ?? 0}</h2></CardBody></Card></Col>
        <Col md={4}><Card><CardBody><CardTitle tag="h6">Avg Wait (ms)</CardTitle><h2>{Math.round(queue?.averageWaitTimeMs ?? 0)}</h2></CardBody></Card></Col>
        <Col md={4}><Card><CardBody><CardTitle tag="h6">Avg Execution (ms)</CardTitle><h2>{Math.round(queue?.averageExecutionTimeMs ?? 0)}</h2></CardBody></Card></Col>
      </Row>
    </div>
  );
};
