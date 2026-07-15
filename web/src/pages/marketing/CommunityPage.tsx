import { Button, Col, Row } from 'reactstrap';

export const CommunityPage = () => (
  <div className="marketing-page">
    <h1 className="marketing-page-title">Community</h1>
    <p className="marketing-page-lead">
      Build with other operators who run GPU fleets for AI coding agents.
    </p>
    <Row className="g-4 mt-2">
      <Col md={4}>
        <div className="marketing-feature">
          <h3>GitHub</h3>
          <p>Issues, discussions, and release notes for the open control plane.</p>
          <Button color="secondary" outline size="sm" href="https://github.com/podpilot" tag="a">
            Open GitHub
          </Button>
        </div>
      </Col>
      <Col md={4}>
        <div className="marketing-feature">
          <h3>Discord</h3>
          <p>Ask for wake policy tips and share gateway client configs.</p>
          <Button color="secondary" outline size="sm" href="https://discord.gg/podpilot" tag="a">
            Join Discord
          </Button>
        </div>
      </Col>
      <Col md={4}>
        <div className="marketing-feature">
          <h3>Status</h3>
          <p>Public system status for the hosted control plane.</p>
          <Button color="secondary" outline size="sm" href="/status" tag="a">
            View status
          </Button>
        </div>
      </Col>
    </Row>
  </div>
);
