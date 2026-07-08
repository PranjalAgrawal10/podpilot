import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Badge, Card, CardBody, CardTitle, Row, Col } from 'reactstrap';
import { schedulerService } from '../services/schedulerService';

export const RequestDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const { data: request, isLoading, error } = useQuery({
    queryKey: ['scheduler-request', id],
    queryFn: () => schedulerService.getRequest(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return <div>Loading request...</div>;
  }

  if (error || !request) {
    return <div>Request not found. <Link to="/scheduler/requests">Back</Link></div>;
  }

  return (
    <div>
      <h2 className="mb-4">Request Details</h2>
      <Card>
        <CardBody>
          <CardTitle tag="h5">{request.path}</CardTitle>
          <Row className="mb-2"><Col sm={4} className="text-muted">Status</Col><Col sm={8}><Badge color="info">{request.status}</Badge></Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Model</Col><Col sm={8}>{request.model ?? '-'}</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Priority</Col><Col sm={8}>{request.priority}</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Pod</Col><Col sm={8}>{request.podId}</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Queue Time</Col><Col sm={8}>{request.queueTimeMs ?? '-'} ms</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Execution Time</Col><Col sm={8}>{request.executionTimeMs ?? '-'} ms</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Retries</Col><Col sm={8}>{request.retryCount}</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Streaming</Col><Col sm={8}>{request.isStreaming ? 'Yes' : 'No'}</Col></Row>
          <Row className="mb-2"><Col sm={4} className="text-muted">Created</Col><Col sm={8}>{new Date(request.createdAt).toLocaleString()}</Col></Row>
          {request.completedAt && (
            <Row className="mb-2"><Col sm={4} className="text-muted">Completed</Col><Col sm={8}>{new Date(request.completedAt).toLocaleString()}</Col></Row>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
