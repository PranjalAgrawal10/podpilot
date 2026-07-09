import { Card, CardBody, CardTitle } from 'reactstrap';

interface MetricCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: string;
  trend?: string;
}

export const MetricCard = ({ title, value, subtitle, icon, trend }: MetricCardProps) => (
  <Card className="stat-card h-100">
    <CardBody>
      <div className="d-flex justify-content-between align-items-start">
        <CardTitle tag="h6" className="text-muted mb-2">
          {title}
        </CardTitle>
        {icon && <span className="fs-5">{icon}</span>}
      </div>
      <p className="stat-value mb-1">{value}</p>
      {subtitle && <small className="text-muted">{subtitle}</small>}
      {trend && <small className="text-muted d-block mt-1">{trend}</small>}
    </CardBody>
  </Card>
);
