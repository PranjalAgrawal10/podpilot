import { Card, CardBody, CardTitle, Col, Row } from 'reactstrap';
import type { CostSummary } from '../../types';

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

interface CostSummaryCardProps {
  cost: CostSummary;
}

export const CostSummaryCard = ({ cost }: CostSummaryCardProps) => (
  <Card>
    <CardBody>
      <CardTitle tag="h5" className="mb-3">
        Cost Summary ({cost.period})
      </CardTitle>
      <Row className="g-3">
        <Col md={4} sm={6}>
          <div>
            <small className="text-muted">Hourly</small>
            <p className="stat-value mb-0">{formatCurrency(cost.hourlyCost)}</p>
          </div>
        </Col>
        <Col md={4} sm={6}>
          <div>
            <small className="text-muted">Daily</small>
            <p className="stat-value mb-0">{formatCurrency(cost.dailyCost)}</p>
          </div>
        </Col>
        <Col md={4} sm={6}>
          <div>
            <small className="text-muted">Weekly</small>
            <p className="stat-value mb-0">{formatCurrency(cost.weeklyCost)}</p>
          </div>
        </Col>
        <Col md={4} sm={6}>
          <div>
            <small className="text-muted">Monthly</small>
            <p className="stat-value mb-0">{formatCurrency(cost.monthlyCost)}</p>
          </div>
        </Col>
        <Col md={4} sm={6}>
          <div>
            <small className="text-muted">Projected Monthly</small>
            <p className="stat-value mb-0">{formatCurrency(cost.projectedMonthlyCost)}</p>
          </div>
        </Col>
        <Col md={4} sm={6}>
          <div>
            <small className="text-muted">Auto-Shutdown Savings</small>
            <p className="stat-value mb-0 text-success">{formatCurrency(cost.autoShutdownSavings)}</p>
          </div>
        </Col>
      </Row>
      <small className="text-muted d-block mt-3">
        Calculated at {new Date(cost.calculatedAt).toLocaleString()}
      </small>
    </CardBody>
  </Card>
);
