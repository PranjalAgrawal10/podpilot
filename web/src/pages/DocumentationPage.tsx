import { Link } from 'react-router-dom';
import { Card, CardBody, Col, ListGroup, ListGroupItem, Row } from 'reactstrap';

const DOC_SECTIONS = [
  { id: 'installation', title: 'Installation', path: '/docs/installation.md', summary: 'Install API, web, and CLI.' },
  { id: 'architecture', title: 'Architecture', path: '/docs/architecture.md', summary: 'Clean architecture and layers.' },
  { id: 'api', title: 'API reference', path: '/docs/api-reference.md', summary: 'REST endpoints and contracts.' },
  { id: 'cli', title: 'CLI', path: '/docs/cli.md', summary: 'Command-line workflows.' },
  { id: 'sdk', title: 'SDKs', path: '/docs/sdk.md', summary: 'TypeScript, Python, .NET, Go, Java.' },
  { id: 'providers', title: 'Providers', path: '/docs/providers.md', summary: 'GPU provider integrations.' },
  { id: 'routing', title: 'Routing', path: '/docs/routing.md', summary: 'Smart model routing.' },
  { id: 'billing', title: 'Billing', path: '/docs/billing.md', summary: 'Plans, quotas, and invoices.' },
  { id: 'security', title: 'Security', path: '/docs/security.md', summary: 'Auth, MFA, and compliance.' },
  { id: 'deployment', title: 'Deployment', path: '/docs/deployment.md', summary: 'Docker, Kubernetes, cloud.' },
  { id: 'mcp', title: 'MCP', path: '/docs/mcp.md', summary: 'Model Context Protocol servers.' },
  { id: 'plugins', title: 'Plugins', path: '/docs/plugins.md', summary: 'Extensibility marketplace.' },
];

export const DocumentationPage = () => (
  <div>
    <h1 className="page-title mb-1">Documentation</h1>
    <p className="text-muted mb-4">
      In-app guide to public PodPilot docs.{' '}
      <Link to="/documentation">Public documentation site</Link>
    </p>

    <Row className="g-3">
      <Col md={4}>
        <Card>
          <CardBody>
            <h5 className="mb-3">Table of contents</h5>
            <ListGroup flush>
              {DOC_SECTIONS.map((section) => (
                <ListGroupItem key={section.id} tag="a" href={`#${section.id}`} action>
                  {section.title}
                </ListGroupItem>
              ))}
            </ListGroup>
          </CardBody>
        </Card>
      </Col>
      <Col md={8}>
        {DOC_SECTIONS.map((section) => (
          <Card key={section.id} id={section.id} className="mb-3">
            <CardBody>
              <h5>{section.title}</h5>
              <p className="text-muted mb-2">{section.summary}</p>
              <a href={section.path} className="btn btn-sm btn-outline-primary">
                Open {section.path}
              </a>
            </CardBody>
          </Card>
        ))}
      </Col>
    </Row>
  </div>
);
