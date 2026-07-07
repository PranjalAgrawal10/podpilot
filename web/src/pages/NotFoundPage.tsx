import { Link } from 'react-router-dom';
import { Button, Card, CardBody } from 'reactstrap';

export const NotFoundPage = () => (
  <div className="not-found-page d-flex align-items-center justify-content-center">
    <Card className="text-center auth-card">
      <CardBody>
        <h1 className="display-1">404</h1>
        <h2>Page Not Found</h2>
        <p className="text-muted mb-4">
          The page you are looking for does not exist or has been moved.
        </p>
        <Button color="primary" tag={Link} to="/dashboard">
          Go to Dashboard
        </Button>
      </CardBody>
    </Card>
  </div>
);
