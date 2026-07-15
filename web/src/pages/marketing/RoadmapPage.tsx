import { Col, Row } from 'reactstrap';

const ITEMS = [
  { quarter: 'Q3 2026', title: 'Commercial billing GA', body: 'Stripe + Razorpay checkout, invoices, and usage quotas in production.' },
  { quarter: 'Q3 2026', title: 'Self-hosted license packs', body: 'Offline activation and air-gapped deployment modes.' },
  { quarter: 'Q4 2026', title: 'Additional GPU providers', body: 'Broader multi-cloud templates and region coverage.' },
  { quarter: 'Q4 2026', title: 'Agent marketplace polish', body: 'First-party plugins and MCP packs curated for ops teams.' },
];

export const RoadmapPage = () => (
  <div className="marketing-page">
    <h1 className="marketing-page-title">Roadmap</h1>
    <p className="marketing-page-lead">What we are shipping next — subject to customer feedback.</p>
    <Row className="g-4 mt-2">
      {ITEMS.map((item) => (
        <Col key={item.title} md={6}>
          <div className="marketing-feature">
            <p className="small text-muted mb-1">{item.quarter}</p>
            <h3>{item.title}</h3>
            <p>{item.body}</p>
          </div>
        </Col>
      ))}
    </Row>
  </div>
);
