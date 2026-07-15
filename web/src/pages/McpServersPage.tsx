import { useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import {
  Alert,
  Badge,
  Button,
  Form,
  FormGroup,
  Input,
  Label,
  Spinner,
  Table,
} from 'reactstrap';
import { mcpService } from '../services/mcpService';
import { useOrganization } from '../contexts/OrganizationContext';
import { usePluginHub } from '../hooks/usePluginHub';
import { PERMISSIONS, type CreateMcpServerRequest } from '../types';

export const McpServersPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.McpRead);
  const canManage = hasPermission(PERMISSIONS.McpManage);
  usePluginHub(currentOrganization?.id);

  const [name, setName] = useState('');
  const [serverKind, setServerKind] = useState('');
  const [endpoint, setEndpoint] = useState('');
  const [credential, setCredential] = useState('');
  const [authScheme, setAuthScheme] = useState('None');

  const { data: servers = [], isLoading, error } = useQuery({
    queryKey: ['mcp-servers', currentOrganization?.id],
    queryFn: mcpService.listServers,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: kinds = [] } = useQuery({
    queryKey: ['mcp-kinds', currentOrganization?.id],
    queryFn: mcpService.listKinds,
    enabled: !!currentOrganization?.id && canRead,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateMcpServerRequest) => mcpService.createServer(data),
    onSuccess: () => {
      toast.success('MCP server registered');
      setName('');
      setEndpoint('');
      setCredential('');
      setAuthScheme('None');
      queryClient.invalidateQueries({ queryKey: ['mcp-servers', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['mcp-tools', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['plugin-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => mcpService.deleteServer(id),
    onSuccess: () => {
      toast.success('MCP server deleted');
      queryClient.invalidateQueries({ queryKey: ['mcp-servers', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['mcp-tools', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['mcp-resources', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['plugin-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage MCP servers.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view MCP servers.</Alert>;
  }

  const onKindChange = (kind: string) => {
    setServerKind(kind);
    const meta = kinds.find((k) => k.serverKind === kind);
    if (meta) {
      if (meta.defaultEndpoint) setEndpoint(meta.defaultEndpoint);
      setAuthScheme(meta.defaultAuthScheme || 'None');
    }
  };

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Mcp.Manage to register servers.');
      return;
    }
    createMutation.mutate({
      name,
      serverKind: serverKind || 'Custom',
      endpoint,
      authScheme,
      credential: credential || undefined,
      discoverOnCreate: true,
    });
  };

  return (
    <div>
      <h1 className="page-title mb-1">MCP Servers</h1>
      <p className="text-muted mb-4">Register and manage Model Context Protocol servers.</p>

      {canManage && (
        <Form onSubmit={onSubmit} className="mb-4 p-3 border rounded" style={{ maxWidth: 720 }}>
          <h2 className="h6 mb-3">Register server</h2>
          <FormGroup>
            <Label>Name</Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </FormGroup>
          <FormGroup>
            <Label>Kind</Label>
            <Input
              type="select"
              value={serverKind}
              onChange={(e) => onKindChange(e.target.value)}
              required
            >
              <option value="">Select kind…</option>
              {kinds.map((k) => (
                <option key={k.serverKind} value={k.serverKind}>
                  {k.displayName}
                </option>
              ))}
              {kinds.length === 0 && <option value="Custom">Custom</option>}
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Endpoint</Label>
            <Input value={endpoint} onChange={(e) => setEndpoint(e.target.value)} required />
          </FormGroup>
          <FormGroup>
            <Label>Auth scheme</Label>
            <Input
              type="select"
              value={authScheme}
              onChange={(e) => setAuthScheme(e.target.value)}
            >
              <option value="None">None</option>
              <option value="Bearer">Bearer</option>
              <option value="ApiKey">ApiKey</option>
              <option value="Basic">Basic</option>
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Credential</Label>
            <Input
              type="password"
              value={credential}
              onChange={(e) => setCredential(e.target.value)}
              placeholder="Optional unless required by kind"
            />
          </FormGroup>
          <Button color="primary" type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? <Spinner size="sm" /> : 'Register'}
          </Button>
        </Form>
      )}

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load servers'}</Alert>}
      {!isLoading && !error && servers.length === 0 && (
        <Alert color="info">No MCP servers registered yet.</Alert>
      )}

      {servers.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Kind</th>
              <th>Endpoint</th>
              <th>Status</th>
              <th>Tools</th>
              <th>Auth</th>
              {canManage && <th />}
            </tr>
          </thead>
          <tbody>
            {servers.map((server) => (
              <tr key={server.id}>
                <td>
                  <div className="fw-semibold">{server.name}</div>
                  <div className="text-muted small">v{server.version}</div>
                </td>
                <td>{server.serverKind}</td>
                <td className="text-break" style={{ maxWidth: 240 }}>{server.endpoint}</td>
                <td>
                  <Badge color={server.status.toLowerCase() === 'connected' ? 'success' : 'secondary'}>
                    {server.status}
                  </Badge>
                </td>
                <td>{server.toolCount}</td>
                <td>{server.authScheme}{server.hasCredential ? ' · credential' : ''}</td>
                {canManage && (
                  <td className="text-end">
                    <Button
                      color="danger"
                      outline
                      size="sm"
                      disabled={deleteMutation.isPending}
                      onClick={() => {
                        if (window.confirm(`Delete MCP server "${server.name}"?`)) {
                          deleteMutation.mutate(server.id);
                        }
                      }}
                    >
                      Delete
                    </Button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
