import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardTitle, Col, Row, Badge } from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import { healthService } from '../services/authService';
import { LoadingSpinner } from '../components/LoadingSpinner';

export const DashboardPage = () => {
  const { user } = useAuth();

  const { data: health, isLoading: healthLoading } = useQuery({
    queryKey: ['health'],
    queryFn: healthService.getHealth,
    refetchInterval: 30000,
  });

  return (
    <div>
      <h1 className="page-title">Dashboard</h1>
      <p className="text-muted mb-4">
        Welcome back, {user?.firstName}! Manage your AI infrastructure from here.
      </p>

      <Row className="g-4">
        <Col md={4}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">Organizations</CardTitle>
              <p className="stat-value">{user?.organizations.length ?? 0}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">Role</CardTitle>
              <p className="stat-value">
                {user?.roles.map((role) => (
                  <Badge key={role} color="primary" className="me-1">
                    {role}
                  </Badge>
                ))}
              </p>
            </CardBody>
          </Card>
        </Col>
        <Col md={4}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h5">System Status</CardTitle>
              {healthLoading ? (
                <LoadingSpinner />
              ) : (
                <Badge color={health?.status === 'Healthy' ? 'success' : 'danger'}>
                  {health?.status ?? 'Unknown'}
                </Badge>
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>

      <Card className="mt-4">
        <CardBody>
          <CardTitle tag="h5">Getting Started</CardTitle>
          <p>
            PodPilot is your AI Infrastructure Autopilot. This foundation provides authentication,
            organization management, and health monitoring. GPU pod management, model deployment,
            and inference providers will be available in upcoming releases.
          </p>
        </CardBody>
      </Card>
    </div>
  );
};
