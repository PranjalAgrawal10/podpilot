import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Col, ListGroup, ListGroupItem, Row, Spinner } from 'reactstrap';
import { commercialService } from '../services/commercialService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

const DOWNLOADS = [
  { name: 'CLI (Windows)', href: 'https://github.com/podpilot/podpilot/releases', note: 'podpilot.exe' },
  { name: 'CLI (macOS / Linux)', href: 'https://github.com/podpilot/podpilot/releases', note: 'podpilot binary' },
  { name: 'TypeScript SDK', href: '/documentation#sdk', note: 'npm package' },
  { name: 'Python SDK', href: '/documentation#sdk', note: 'pip package' },
  { name: '.NET SDK', href: '/documentation#sdk', note: 'NuGet' },
  { name: 'Go SDK', href: '/documentation#sdk', note: 'module' },
];

export const DownloadsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead =
    hasPermission(PERMISSIONS.BillingRead) || hasPermission(PERMISSIONS.BillingView);

  const { data: release, isLoading, error } = useQuery({
    queryKey: ['release-status', currentOrganization?.id],
    queryFn: commercialService.getReleaseStatus,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view downloads.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view downloads.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title mb-1">Downloads</h1>
      <p className="text-muted mb-4">CLI, SDKs, and release channels.</p>

      {isLoading && (
        <div className="text-center py-3">
          <Spinner size="sm" />
        </div>
      )}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load release status'}
        </Alert>
      )}

      {release && (
        <Card className="mb-4">
          <CardBody>
            <div className="d-flex flex-wrap justify-content-between gap-3">
              <div>
                <h5 className="mb-1">Release status</h5>
                <p className="mb-0">
                  Current <strong>{release.currentVersion}</strong>
                  {release.latestVersion && (
                    <>
                      {' '}
                      · Latest <strong>{release.latestVersion}</strong>
                    </>
                  )}
                </p>
                <Badge color="secondary" className="mt-2">
                  {release.channel}
                </Badge>
                {release.updateAvailable && (
                  <Badge color="info" className="mt-2 ms-2">
                    Update available
                  </Badge>
                )}
                {release.releaseNotes && (
                  <p className="small text-muted mt-2 mb-0">{release.releaseNotes}</p>
                )}
              </div>
              {release.downloadUrl && (
                <Button color="primary" href={release.downloadUrl} tag="a" target="_blank" rel="noreferrer">
                  Download latest
                </Button>
              )}
            </div>
          </CardBody>
        </Card>
      )}

      <Row>
        <Col md={8}>
          <Card>
            <CardBody>
              <h5 className="mb-3">Packages</h5>
              <ListGroup flush>
                {DOWNLOADS.map((item) => (
                  <ListGroupItem
                    key={item.name}
                    className="d-flex justify-content-between align-items-center"
                  >
                    <div>
                      <strong>{item.name}</strong>
                      <div className="small text-muted">{item.note}</div>
                    </div>
                    <Button color="link" size="sm" href={item.href} tag="a">
                      Open
                    </Button>
                  </ListGroupItem>
                ))}
              </ListGroup>
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
