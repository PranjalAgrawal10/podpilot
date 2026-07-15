import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Badge, Card, CardBody, Col, Progress, Row, Spinner } from 'reactstrap';
import { commercialService } from '../../services/commercialService';
import { useCommercialHub } from '../../hooks/useCommercialHub';
import { useOrganization } from '../../contexts/OrganizationContext';
import { PERMISSIONS } from '../../types';

export const CommercialDashboardWidgets = ({ showTitle = true }: { showTitle?: boolean }) => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead =
    hasPermission(PERMISSIONS.BillingRead) || hasPermission(PERMISSIONS.BillingView);
  useCommercialHub(currentOrganization?.id);

  const { data, isLoading } = useQuery({
    queryKey: ['commercial-dashboard', currentOrganization?.id],
    queryFn: commercialService.getDashboard,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!canRead || !currentOrganization) {
    return null;
  }

  if (isLoading) {
    return (
      <div className="text-center py-3">
        <Spinner size="sm" />
      </div>
    );
  }

  if (!data) {
    return null;
  }

  const quotaUsed = Math.max(0, Math.min(100, 100 - data.remainingRequestQuotaPercent));

  return (
    <div className="mb-4">
      {showTitle && (
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="mb-0">Commercial</h5>
          <Link to="/billing" className="btn btn-sm btn-outline-primary">
            Manage billing
          </Link>
        </div>
      )}
      <Row className="g-3">
        <Col md={3} sm={6}>
          <Card className="stat-card h-100">
            <CardBody>
              <div className="text-muted small">Subscription</div>
              <h5 className="mb-1">{data.subscription.planName}</h5>
              <Badge color={data.subscription.status === 'Active' ? 'success' : 'secondary'}>
                {data.subscription.status}
              </Badge>
            </CardBody>
          </Card>
        </Col>
        <Col md={3} sm={6}>
          <Card className="stat-card h-100">
            <CardBody>
              <div className="text-muted small">Est. monthly cost</div>
              <h4 className="mb-1">${data.estimatedMonthlyCostUsd.toFixed(2)}</h4>
              <div className="small text-muted">
                {data.usage.requests.toLocaleString()} requests this period
              </div>
              <Progress value={quotaUsed} className="mt-2" style={{ height: '0.4rem' }} />
            </CardBody>
          </Card>
        </Col>
        <Col md={3} sm={6}>
          <Card className="stat-card h-100">
            <CardBody>
              <div className="text-muted small">License</div>
              <h5 className="mb-1">{data.license.edition}</h5>
              <Badge color={data.license.isValid ? 'success' : 'warning'}>
                {data.license.isActivated ? 'Activated' : 'Not activated'}
              </Badge>
            </CardBody>
          </Card>
        </Col>
        <Col md={3} sm={6}>
          <Card className="stat-card h-100">
            <CardBody>
              <div className="text-muted small">Version</div>
              <h5 className="mb-1">{data.release.currentVersion || '—'}</h5>
              {data.release.updateAvailable ? (
                <Badge color="info">Update available</Badge>
              ) : (
                <span className="small text-muted">{data.release.channel}</span>
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
