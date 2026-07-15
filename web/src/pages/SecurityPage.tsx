import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Button, Card, CardBody, Col, Row, Spinner, Table } from 'reactstrap';
import { securityService } from '../services/securityService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useSecurityHub } from '../hooks/useSecurityHub';
import { PERMISSIONS } from '../types';

export const SecurityPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.SecurityRead);
  useSecurityHub(currentOrganization?.id);

  const { data: dashboard, isLoading, error } = useQuery({
    queryKey: ['security-dashboard', currentOrganization?.id],
    queryFn: securityService.getDashboard,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view security.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view security.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Security</h1>
          <p className="text-muted mb-0">Enterprise security overview for {currentOrganization.name}.</p>
        </div>
      </div>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load security dashboard'}</Alert>
      )}

      {dashboard && (
        <>
          <Row className="mb-4">
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Security score</div>
                  <h3>{dashboard.securityScore}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Active sessions</div>
                  <h3>{dashboard.activeSessions}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Failed logins (24h)</div>
                  <h3>{dashboard.failedLogins24h}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">MFA coverage</div>
                  <h3>{dashboard.mfaCoveragePercent.toFixed(0)}%</h3>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Row className="mb-4">
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Compliance</div>
                  <h3 className="h5 mb-0">{dashboard.complianceStatus}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Secrets</div>
                  <h3>{dashboard.secretCount}</h3>
                  <div className="text-muted small">{dashboard.expiringSecrets} expiring</div>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Recent audits</div>
                  <h3>{dashboard.recentAuditEvents}</h3>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <div className="d-flex flex-wrap gap-2 mb-4">
            <Button tag={Link} to="/security/audit" color="secondary" outline size="sm">
              Audit logs
            </Button>
            <Button tag={Link} to="/security/secrets" color="secondary" outline size="sm">
              Secrets
            </Button>
            <Button tag={Link} to="/security/identity-providers" color="secondary" outline size="sm">
              Identity providers
            </Button>
            <Button tag={Link} to="/security/policies" color="secondary" outline size="sm">
              Policies
            </Button>
            <Button tag={Link} to="/security/compliance" color="secondary" outline size="sm">
              Compliance
            </Button>
            <Button tag={Link} to="/security/sessions" color="secondary" outline size="sm">
              Sessions
            </Button>
            <Button tag={Link} to="/security/devices" color="secondary" outline size="sm">
              Trusted devices
            </Button>
          </div>

          <h2 className="h5 mb-3">Recent audit events</h2>
          {dashboard.recentAudits.length === 0 ? (
            <Alert color="info">No recent audit events.</Alert>
          ) : (
            <Table responsive hover>
              <thead>
                <tr>
                  <th>When</th>
                  <th>Actor</th>
                  <th>Category</th>
                  <th>Event</th>
                  <th>Summary</th>
                </tr>
              </thead>
              <tbody>
                {dashboard.recentAudits.map((event) => (
                  <tr key={event.id}>
                    <td className="text-nowrap">{new Date(event.occurredAt).toLocaleString()}</td>
                    <td>{event.actorEmail || '—'}</td>
                    <td>{event.category}</td>
                    <td>{event.eventType}</td>
                    <td>{event.summary}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </>
      )}
    </div>
  );
};
