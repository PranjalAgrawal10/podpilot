import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Card, CardBody, CardTitle, FormGroup, Input, Label, Table } from 'reactstrap';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { useOrganization } from '../contexts/OrganizationContext';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import { PERMISSIONS } from '../types';

const severityColor = (severity: string): string => {
  const normalized = severity.toLowerCase();
  if (normalized === 'critical' || normalized === 'error') return 'danger';
  if (normalized === 'warning') return 'warning';
  if (normalized === 'info') return 'info';
  return 'secondary';
};

export const AlertsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);
  const [activeOnly, setActiveOnly] = useState(true);

  useObservabilityHub(currentOrganization?.id);

  const { data: alerts = [], isLoading } = useQuery({
    queryKey: ['alerts', currentOrganization?.id, activeOnly],
    queryFn: () => observabilityService.listAlerts(activeOnly, 100),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view alerts.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view alerts.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title">Alerts</h1>
      <p className="text-muted mb-4">Monitor active and historical observability alerts.</p>

      <Card className="mb-4">
        <CardBody>
          <FormGroup check>
            <Input
              id="activeOnly"
              type="checkbox"
              checked={activeOnly}
              onChange={(e) => setActiveOnly(e.target.checked)}
            />
            <Label for="activeOnly" check>
              Show active alerts only
            </Label>
          </FormGroup>
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <CardTitle tag="h5" className="mb-3">
            {activeOnly ? 'Active Alerts' : 'All Alerts'} ({alerts.length})
          </CardTitle>
          {isLoading && alerts.length === 0 ? (
            <LoadingSpinner />
          ) : alerts.length === 0 ? (
            <Alert color="success" className="mb-0">
              {activeOnly ? 'No active alerts.' : 'No alerts found.'}
            </Alert>
          ) : (
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Raised</th>
                  <th>Severity</th>
                  <th>Type</th>
                  <th>Title</th>
                  <th>Message</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {alerts.map((alert) => (
                  <tr key={alert.id}>
                    <td className="small">{new Date(alert.raisedAt).toLocaleString()}</td>
                    <td>
                      <Badge color={severityColor(alert.severity)}>{alert.severity}</Badge>
                    </td>
                    <td>{alert.alertType}</td>
                    <td>{alert.title}</td>
                    <td className="small">{alert.message}</td>
                    <td>
                      <Badge color={alert.isActive ? 'warning' : 'success'}>
                        {alert.isActive ? 'Active' : 'Resolved'}
                      </Badge>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
