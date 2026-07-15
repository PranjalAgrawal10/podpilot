import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Badge, Button, Card, CardBody, Col, ListGroup, ListGroupItem, Row, Spinner } from 'reactstrap';
import { complianceService } from '../services/complianceService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const CompliancePage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ComplianceRead);
  const canManage = hasPermission(PERMISSIONS.ComplianceManage);

  const { data: status, isLoading, error } = useQuery({
    queryKey: ['compliance', currentOrganization?.id],
    queryFn: complianceService.getStatus,
    enabled: !!currentOrganization?.id && canRead,
  });

  const exportMutation = useMutation({
    mutationFn: complianceService.export,
    onSuccess: (result) => {
      toast.success('Compliance export ready');
      const blob = new Blob([result.jsonPayload], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = `compliance-export-${new Date(result.exportedAt).toISOString().slice(0, 10)}.json`;
      anchor.click();
      URL.revokeObjectURL(url);
      queryClient.invalidateQueries({ queryKey: ['compliance', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['security-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view compliance.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view compliance.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Compliance</h1>
          <p className="text-muted mb-0">Compliance status for {currentOrganization.name}.</p>
        </div>
        {canManage && (
          <Button
            color="primary"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? <Spinner size="sm" /> : 'Export'}
          </Button>
        )}
      </div>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load compliance status'}
        </Alert>
      )}

      {status && (
        <>
          <Row className="mb-4">
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Overall status</div>
                  <h3 className="h5 mb-0">{status.overallStatus}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Frameworks</div>
                  <div className="d-flex flex-wrap gap-1 mt-2">
                    <Badge color={status.gdprEnabled ? 'success' : 'secondary'}>GDPR</Badge>
                    <Badge color={status.soc2Enabled ? 'success' : 'secondary'}>SOC 2</Badge>
                    <Badge color={status.iso27001Enabled ? 'success' : 'secondary'}>ISO 27001</Badge>
                  </div>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Data retention</div>
                  <h3>{status.dataRetentionDays} days</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <div className="text-muted">Log retention</div>
                  <h3>{status.logRetentionDays} days</h3>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Row className="mb-4">
            <Col md={6}>
              <Card>
                <CardBody>
                  <div className="text-muted">Last export</div>
                  <div>
                    {status.lastExportAt
                      ? new Date(status.lastExportAt).toLocaleString()
                      : 'Never'}
                  </div>
                </CardBody>
              </Card>
            </Col>
            <Col md={6}>
              <Card>
                <CardBody>
                  <div className="text-muted">Last erasure</div>
                  <div>
                    {status.lastErasureAt
                      ? new Date(status.lastErasureAt).toLocaleString()
                      : 'Never'}
                  </div>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <h2 className="h5 mb-3">Control checklist</h2>
          {status.controlChecklist.length === 0 ? (
            <Alert color="info">No checklist items.</Alert>
          ) : (
            <ListGroup>
              {status.controlChecklist.map((item) => (
                <ListGroupItem key={item}>{item}</ListGroupItem>
              ))}
            </ListGroup>
          )}
        </>
      )}
    </div>
  );
};
