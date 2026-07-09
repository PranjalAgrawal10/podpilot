import { Card, CardBody, CardTitle, Col, Row } from 'reactstrap';

interface QueueWidgetProps {
  queueSize: number;
  activeStreams: number;
  requestsPerSecond?: number;
  title?: string;
}

export const QueueWidget = ({
  queueSize,
  activeStreams,
  requestsPerSecond,
  title = 'Queue & Requests',
}: QueueWidgetProps) => (
  <Card className="h-100">
    <CardBody>
      <CardTitle tag="h6" className="text-muted mb-3">
        {title}
      </CardTitle>
      <Row className="g-3">
        <Col xs={6}>
          <small className="text-muted d-block">Queue Size</small>
          <p className="stat-value mb-0">{queueSize}</p>
        </Col>
        <Col xs={6}>
          <small className="text-muted d-block">Active Requests</small>
          <p className="stat-value mb-0">{activeStreams}</p>
        </Col>
        {requestsPerSecond != null && (
          <Col xs={12}>
            <small className="text-muted d-block">Requests / sec</small>
            <p className="stat-value mb-0">{requestsPerSecond.toFixed(2)}</p>
          </Col>
        )}
      </Row>
    </CardBody>
  </Card>
);
