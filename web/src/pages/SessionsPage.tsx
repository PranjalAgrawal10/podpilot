import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Spinner, Table } from 'reactstrap';
import { securityService } from '../services/securityService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useSecurityHub } from '../hooks/useSecurityHub';
import { PERMISSIONS } from '../types';

export const SessionsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.SecurityRead);
  useSecurityHub(currentOrganization?.id);

  const { data: sessions = [], isLoading, error } = useQuery({
    queryKey: ['security-sessions', currentOrganization?.id],
    queryFn: securityService.listSessions,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view sessions.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view sessions.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title mb-1">Sessions</h1>
      <p className="text-muted mb-4">Active and recent sessions for {currentOrganization.name}.</p>

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load sessions'}</Alert>
      )}
      {!isLoading && !error && sessions.length === 0 && (
        <Alert color="info">No sessions found.</Alert>
      )}

      {sessions.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Session</th>
              <th>User</th>
              <th>IP</th>
              <th>User agent</th>
              <th>Started</th>
              <th>Last seen</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {sessions.map((session) => (
              <tr key={session.id}>
                <td className="text-break" style={{ maxWidth: 160 }}>
                  {session.sessionId}
                </td>
                <td className="text-break" style={{ maxWidth: 140 }}>
                  {session.userId}
                </td>
                <td>{session.ipAddress || '—'}</td>
                <td className="text-break" style={{ maxWidth: 220 }}>
                  {session.userAgent || '—'}
                </td>
                <td className="text-nowrap">{new Date(session.startedAt).toLocaleString()}</td>
                <td className="text-nowrap">{new Date(session.lastSeenAt).toLocaleString()}</td>
                <td>
                  <Badge color={session.isActive ? 'success' : 'secondary'}>
                    {session.isActive ? 'Active' : 'Inactive'}
                  </Badge>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
