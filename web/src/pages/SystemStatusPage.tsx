import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Card, CardBody, Col, Row, Spinner, Table } from 'reactstrap';
import { commercialService } from '../services/commercialService';

const statusColor = (status: string): string => {
  const normalized = status.toLowerCase();
  if (normalized.includes('operational') || normalized.includes('healthy')) return 'success';
  if (normalized.includes('degraded') || normalized.includes('partial')) return 'warning';
  if (normalized.includes('outage') || normalized.includes('down')) return 'danger';
  return 'secondary';
};

export const SystemStatusPage = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ['system-status'],
    queryFn: commercialService.getSystemStatus,
    refetchInterval: 30000,
  });

  return (
    <div className="marketing-page">
      <h1 className="page-title mb-1">System status</h1>
      <p className="text-muted mb-4">Live platform health from GET /api/v1/status.</p>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load system status'}
        </Alert>
      )}

      {data && (
        <>
          <Row className="g-3 mb-4">
            <Col md={4}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">Overall</div>
                  <Badge color={statusColor(data.status)} className="fs-6">
                    {data.status}
                  </Badge>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">Version</div>
                  <h4 className="mb-0">{data.version || '—'}</h4>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">Updates</div>
                  <h5 className="mb-0">
                    {data.updateAvailable ? 'Update available' : 'Up to date'}
                  </h5>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Card>
            <CardBody>
              <h5 className="mb-3">Components</h5>
              {data.components.length === 0 ? (
                <p className="text-muted mb-0">No component details reported.</p>
              ) : (
                <Table responsive hover className="mb-0">
                  <thead>
                    <tr>
                      <th>Component</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.components.map((component) => (
                      <tr key={component.name}>
                        <td>{component.name}</td>
                        <td>
                          <Badge color={statusColor(component.status)}>{component.status}</Badge>
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
    </div>
  );
};
