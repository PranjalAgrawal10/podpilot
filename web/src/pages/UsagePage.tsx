import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Card, CardBody, Col, Progress, Row, Spinner } from 'reactstrap';
import { billingService } from '../services/billingService';
import { useCommercialHub } from '../hooks/useCommercialHub';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

const quotaPercent = (used: number, max: number): number => {
  if (max <= 0) return 0;
  return Math.min(100, Math.round((used / max) * 100));
};

export const UsagePage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead =
    hasPermission(PERMISSIONS.BillingRead) || hasPermission(PERMISSIONS.BillingView);
  useCommercialHub(currentOrganization?.id);

  const { data: usage, isLoading, error } = useQuery({
    queryKey: ['billing-usage', currentOrganization?.id],
    queryFn: billingService.getUsage,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 30000,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view usage.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view usage.</Alert>;
  }

  const bars = usage
    ? [
        {
          label: 'API requests',
          value: usage.requests,
          max: usage.quotas.maxApiRequestsPerMonth,
          percent: usage.requestsQuotaPercent || quotaPercent(usage.requests, usage.quotas.maxApiRequestsPerMonth),
        },
        {
          label: 'Storage (GB)',
          value: usage.storageGb,
          max: usage.quotas.maxStorageGb,
          percent: quotaPercent(usage.storageGb, usage.quotas.maxStorageGb),
        },
        {
          label: 'Models',
          value: usage.models,
          max: usage.quotas.maxModels,
          percent: quotaPercent(usage.models, usage.quotas.maxModels),
        },
        {
          label: 'Providers',
          value: usage.providers,
          max: usage.quotas.maxProviders,
          percent: quotaPercent(usage.providers, usage.quotas.maxProviders),
        },
        {
          label: 'Organizations',
          value: usage.organizations,
          max: usage.quotas.maxOrganizations,
          percent: quotaPercent(usage.organizations, usage.quotas.maxOrganizations),
        },
      ]
    : [];

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Usage</h1>
          <p className="text-muted mb-0">Quota and metered usage for the current billing period.</p>
        </div>
        <Link to="/billing" className="btn btn-sm btn-outline-primary">
          Billing
        </Link>
      </div>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load usage'}</Alert>
      )}

      {usage && (
        <>
          <Row className="g-3 mb-4">
            <Col md={3} sm={6}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">GPU hours</div>
                  <h3>{usage.gpuHours.toFixed(1)}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3} sm={6}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">Tokens</div>
                  <h3>{usage.tokens.toLocaleString()}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3} sm={6}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">Bandwidth</div>
                  <h3>{usage.bandwidthGb.toFixed(2)} GB</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3} sm={6}>
              <Card className="stat-card">
                <CardBody>
                  <div className="text-muted small">Est. monthly cost</div>
                  <h3>${usage.estimatedMonthlyCostUsd.toFixed(2)}</h3>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <p className="text-muted small mb-3">
            Period {new Date(usage.periodStart).toLocaleDateString()} –{' '}
            {new Date(usage.periodEnd).toLocaleDateString()}
          </p>

          <Card>
            <CardBody>
              {bars.map((bar) => (
                <div key={bar.label} className="mb-3">
                  <div className="d-flex justify-content-between mb-1">
                    <span>{bar.label}</span>
                    <span className="text-muted small">
                      {typeof bar.value === 'number' && !Number.isInteger(bar.value)
                        ? bar.value.toFixed(1)
                        : bar.value.toLocaleString()}{' '}
                      / {bar.max.toLocaleString()}
                      <Badge color={bar.percent >= 90 ? 'danger' : 'secondary'} className="ms-2">
                        {Math.round(bar.percent)}%
                      </Badge>
                    </span>
                  </div>
                  <Progress
                    value={bar.percent}
                    color={bar.percent >= 90 ? 'danger' : bar.percent >= 70 ? 'warning' : 'primary'}
                  />
                </div>
              ))}
            </CardBody>
          </Card>
        </>
      )}
    </div>
  );
};
