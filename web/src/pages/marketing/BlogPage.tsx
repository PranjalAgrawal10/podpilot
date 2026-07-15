import { Link } from 'react-router-dom';
import { Card, CardBody, Col, Row } from 'reactstrap';

const POSTS = [
  {
    slug: 'idle-gpu-shutdown',
    title: 'How PodPilot cuts idle GPU spend without killing wake latency',
    date: '2026-06-12',
    excerpt:
      'A walkthrough of grace periods, warm standby pools, and gateway-triggered wake so teams stop paying for silent RTX nodes overnight.',
  },
  {
    slug: 'openai-compatible-gateway',
    title: 'Shipping an OpenAI-compatible gateway on your own pods',
    date: '2026-05-28',
    excerpt:
      'Point Claude Code, Cursor, or custom agents at PodPilot — API keys, model routes, and stream-safe timeouts that map to real GPU capacity.',
  },
];

export const BlogPage = () => (
  <div className="marketing-page">
    <h1 className="marketing-page-title">Blog</h1>
    <p className="marketing-page-lead">Notes from building GPU autopilot for real fleets.</p>
    <Row className="g-4 mt-2">
      {POSTS.map((post) => (
        <Col key={post.slug} md={6}>
          <Card className="h-100 marketing-price-card">
            <CardBody>
              <p className="small text-muted mb-2">{post.date}</p>
              <h3 className="h5">{post.title}</h3>
              <p className="text-muted">{post.excerpt}</p>
              <Link to="/contact" className="stretched-link text-decoration-none">
                Read more →
              </Link>
            </CardBody>
          </Card>
        </Col>
      ))}
    </Row>
  </div>
);
