import { Link } from 'react-router-dom';
import { Button, Col, Row } from 'reactstrap';

const FEATURES = [
  {
    title: 'Idle-aware pods',
    body: 'Auto-shutdown idle GPUs and wake them on the next gateway request.',
  },
  {
    title: 'Multi-provider control',
    body: 'RunPod and beyond — unify credentials, regions, and templates in one org.',
  },
  {
    title: 'AI gateway',
    body: 'OpenAI-compatible routes with wake-on-demand and API key isolation.',
  },
  {
    title: 'Smart routing',
    body: 'Cost, latency, and reliability ranking across local and cloud models.',
  },
  {
    title: 'Observability',
    body: 'GPU metrics, costs, and alerts wired into the same control plane.',
  },
  {
    title: 'Enterprise security',
    body: 'SSO, MFA, secrets, audit logs, and compliance exports.',
  },
];

export const FeaturesPage = () => (
  <div className="marketing-page">
    <h1 className="marketing-page-title">Built for GPU fleets, not chat demos</h1>
    <p className="marketing-page-lead">
      PodPilot is the autopilot layer between your team and expensive GPUs.
    </p>
    <Row className="g-4 mt-2">
      {FEATURES.map((feature) => (
        <Col key={feature.title} md={4}>
          <div className="marketing-feature">
            <h3>{feature.title}</h3>
            <p>{feature.body}</p>
          </div>
        </Col>
      ))}
    </Row>
    <div className="mt-5">
      <Button tag={Link} to="/pricing" color="primary">
        View pricing
      </Button>
    </div>
  </div>
);
