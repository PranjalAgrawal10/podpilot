import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Col, Row, Spinner } from 'reactstrap';
import { deploymentService } from '../services/deploymentService';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS } from '../types';

export const DeploymentTemplatesPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.DeploymentRead);
  const canManage = hasPermission(PERMISSIONS.DeploymentManage);

  const { data: templates = [], isLoading, error } = useQuery({
    queryKey: ['deployment-templates', currentOrganization?.id],
    queryFn: deploymentService.listTemplates,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to browse templates.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view deployment templates.</Alert>;
  }

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Deployment templates</h1>
          <p className="text-muted mb-0">One-click presets for coding, chat, vision, and reasoning pods.</p>
        </div>
        <div className="d-flex gap-2">
          <Button tag={Link} to="/deployments" color="secondary" outline size="sm">
            Deployments
          </Button>
          {canManage && (
            <Button tag={Link} to="/deployments/create" color="primary" size="sm">
              Deploy AI Pod
            </Button>
          )}
        </div>
      </div>

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{getApiErrorMessage(error, 'Failed to load templates')}</Alert>}

      <Row>
        {templates.map((t) => (
          <Col key={t.id} md={6} lg={4} className="mb-4">
            <Card className="h-100">
              <CardBody>
                <div className="d-flex justify-content-between mb-2">
                  <h5 className="mb-0">{t.name}</h5>
                  <Badge color="primary">{t.kind}</Badge>
                </div>
                <p className="text-muted small mb-2">{t.code}</p>
                <p className="mb-3">{t.description || 'No description.'}</p>
                <p className="small mb-1">
                  Runtime: <strong>{t.runtime}</strong>
                </p>
                <p className="small mb-1">
                  GPU: <strong>{t.recommendedGpuCode}</strong>
                </p>
                <p className="small text-muted mb-3">
                  Models: {t.defaultModelCodes.length > 0 ? t.defaultModelCodes.join(', ') : 'Custom'}
                </p>
                {canManage && (
                  <Button
                    tag={Link}
                    to="/deployments/create"
                    color="link"
                    className="p-0"
                  >
                    Use template
                  </Button>
                )}
              </CardBody>
            </Card>
          </Col>
        ))}
      </Row>

      {!isLoading && templates.length === 0 && (
        <Alert color="info">No templates in the catalog yet.</Alert>
      )}
    </div>
  );
};
