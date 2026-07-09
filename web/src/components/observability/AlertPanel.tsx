import { Link } from 'react-router-dom';
import { Alert, Badge, Card, CardBody, CardTitle, ListGroup, ListGroupItem } from 'reactstrap';
import type { ObservabilityAlert } from '../../types';

const severityColor = (severity: string): string => {
  const normalized = severity.toLowerCase();
  if (normalized === 'critical' || normalized === 'error') return 'danger';
  if (normalized === 'warning') return 'warning';
  if (normalized === 'info') return 'info';
  return 'secondary';
};

interface AlertPanelProps {
  alerts: ObservabilityAlert[];
  title?: string;
  maxItems?: number;
  showViewAll?: boolean;
}

export const AlertPanel = ({
  alerts,
  title = 'Recent Alerts',
  maxItems = 5,
  showViewAll = true,
}: AlertPanelProps) => {
  const visibleAlerts = alerts.slice(0, maxItems);

  return (
    <Card className="h-100">
      <CardBody>
        <div className="d-flex justify-content-between align-items-center mb-3">
          <CardTitle tag="h5" className="mb-0">
            {title}
          </CardTitle>
          {showViewAll && (
            <Link to="/observability/alerts" className="btn btn-sm btn-outline-primary">
              View All
            </Link>
          )}
        </div>
        {visibleAlerts.length === 0 ? (
          <Alert color="success" className="mb-0">
            No active alerts.
          </Alert>
        ) : (
          <ListGroup flush>
            {visibleAlerts.map((alert) => (
              <ListGroupItem key={alert.id} className="px-0">
                <div className="d-flex justify-content-between align-items-start gap-2">
                  <div>
                    <strong>{alert.title}</strong>
                    <p className="text-muted small mb-1">{alert.message}</p>
                    <small className="text-muted">
                      {new Date(alert.raisedAt).toLocaleString()}
                      {alert.alertType && ` · ${alert.alertType}`}
                    </small>
                  </div>
                  <Badge color={severityColor(alert.severity)}>{alert.severity}</Badge>
                </div>
              </ListGroupItem>
            ))}
          </ListGroup>
        )}
      </CardBody>
    </Card>
  );
};
