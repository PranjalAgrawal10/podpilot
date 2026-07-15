import { Link } from 'react-router-dom';
import { Card, CardBody, Col, ListGroup, ListGroupItem, Row } from 'reactstrap';

const DOCS = [
  { title: 'Installation', href: '/docs/installation.md' },
  { title: 'Architecture', href: '/docs/architecture.md' },
  { title: 'API reference', href: '/docs/api-reference.md' },
  { title: 'CLI', href: '/docs/cli.md' },
  { title: 'SDKs', href: '/docs/sdk.md' },
  { title: 'Billing', href: '/docs/billing.md' },
  { title: 'Security', href: '/docs/security.md' },
  { title: 'Deployment', href: '/docs/deployment.md' },
];

export const DocsMarketingPage = () => (
  <div className="marketing-page">
    <h1 className="marketing-page-title">Documentation</h1>
    <p className="marketing-page-lead">
      Guides for operators, developers, and self-hosted deployments.
    </p>
    <Row className="g-4 mt-2">
      <Col md={5}>
        <Card>
          <CardBody>
            <h5 className="mb-3">Browse</h5>
            <ListGroup flush>
              {DOCS.map((doc) => (
                <ListGroupItem key={doc.href} tag="a" href={doc.href} action>
                  {doc.title}
                </ListGroupItem>
              ))}
            </ListGroup>
          </CardBody>
        </Card>
      </Col>
      <Col md={7}>
        <div className="marketing-feature">
          <h3>Prefer the app?</h3>
          <p>
            Signed-in users get a structured TOC under <code>/docs</code> in the app sidebar, or jump
            straight to the product.
          </p>
          <Link to="/register" className="btn btn-primary btn-sm">
            Create an account
          </Link>
        </div>
      </Col>
    </Row>
  </div>
);
