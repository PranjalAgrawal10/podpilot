import { useQuery } from '@tanstack/react-query';
import { Alert, Spinner, Table } from 'reactstrap';
import { auditService } from '../services/auditService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useSecurityHub } from '../hooks/useSecurityHub';
import { PERMISSIONS } from '../types';

export const AuditLogsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.AuditRead);
  useSecurityHub(currentOrganization?.id);

  const { data: events = [], isLoading, error } = useQuery({
    queryKey: ['audit', currentOrganization?.id],
    queryFn: () => auditService.list({ take: 200 }),
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view audit logs.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view audit logs.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title mb-1">Audit logs</h1>
      <p className="text-muted mb-4">Enterprise audit trail for {currentOrganization.name}.</p>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load audit events'}</Alert>
      )}
      {!isLoading && !error && events.length === 0 && (
        <Alert color="info">No audit events found.</Alert>
      )}

      {events.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>When</th>
              <th>Actor</th>
              <th>Category</th>
              <th>Event</th>
              <th>Entity</th>
              <th>Summary</th>
              <th>IP</th>
            </tr>
          </thead>
          <tbody>
            {events.map((event) => (
              <tr key={event.id}>
                <td className="text-nowrap">{new Date(event.occurredAt).toLocaleString()}</td>
                <td>{event.actorEmail || '—'}</td>
                <td>{event.category}</td>
                <td>{event.eventType}</td>
                <td>
                  {event.entityType
                    ? `${event.entityType}${event.entityId ? ` (${event.entityId})` : ''}`
                    : '—'}
                </td>
                <td>{event.summary}</td>
                <td>{event.ipAddress || '—'}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
