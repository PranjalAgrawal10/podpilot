import { useState, type FormEvent } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
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
import { PERMISSIONS, type ExecuteMcpToolResponse } from '../types';

export const McpToolsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.McpRead);
  const canManage = hasPermission(PERMISSIONS.McpManage);
  usePluginHub(currentOrganization?.id);

  const [toolName, setToolName] = useState('');
  const [serverId, setServerId] = useState('');
  const [argumentsJson, setArgumentsJson] = useState('{}');
  const [result, setResult] = useState<ExecuteMcpToolResponse | null>(null);

  const { data: tools = [], isLoading, error } = useQuery({
    queryKey: ['mcp-tools', currentOrganization?.id],
    queryFn: mcpService.listTools,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: servers = [] } = useQuery({
    queryKey: ['mcp-servers', currentOrganization?.id],
    queryFn: mcpService.listServers,
    enabled: !!currentOrganization?.id && canRead,
  });

  const executeMutation = useMutation({
    mutationFn: mcpService.executeTool,
    onSuccess: (data) => {
      setResult(data);
      if (data.succeeded) {
        toast.success(`Tool executed in ${data.durationMs} ms`);
      } else {
        toast.error(data.errorMessage || 'Tool execution failed');
      }
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view MCP tools.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view MCP tools.</Alert>;
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Mcp.Manage to execute tools.');
      return;
    }
    try {
      JSON.parse(argumentsJson || '{}');
    } catch {
      toast.error('Arguments must be valid JSON');
      return;
    }
    executeMutation.mutate({
      toolName,
      serverId: serverId || undefined,
      argumentsJson: argumentsJson || '{}',
    });
  };

  return (
    <div>
      <h1 className="page-title mb-1">MCP Tools</h1>
      <p className="text-muted mb-4">Discovered tools from connected MCP servers.</p>

      {canManage && (
        <Form onSubmit={onSubmit} className="mb-4 p-3 border rounded" style={{ maxWidth: 720 }}>
          <h2 className="h6 mb-3">Execute tool</h2>
          <FormGroup>
            <Label>Tool name</Label>
            <Input
              list="mcp-tool-names"
              value={toolName}
              onChange={(e) => setToolName(e.target.value)}
              required
            />
            <datalist id="mcp-tool-names">
              {tools.map((t) => (
                <option key={t.id} value={t.name} />
              ))}
            </datalist>
          </FormGroup>
          <FormGroup>
            <Label>Server (optional)</Label>
            <Input
              type="select"
              value={serverId}
              onChange={(e) => setServerId(e.target.value)}
            >
              <option value="">Any matching server</option>
              {servers.map((s) => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Arguments (JSON)</Label>
            <Input
              type="textarea"
              rows={4}
              value={argumentsJson}
              onChange={(e) => setArgumentsJson(e.target.value)}
            />
          </FormGroup>
          <Button color="primary" type="submit" disabled={executeMutation.isPending}>
            {executeMutation.isPending ? <Spinner size="sm" /> : 'Execute'}
          </Button>
        </Form>
      )}

      {result && (
        <div className="mb-4 p-3 border rounded">
          <h2 className="h6">Result</h2>
          <p className="mb-1">
            <Badge color={result.succeeded ? 'success' : 'danger'}>
              {result.succeeded ? 'Succeeded' : 'Failed'}
            </Badge>
            <span className="ms-2 text-muted">{result.durationMs} ms</span>
          </p>
          {result.errorMessage && <Alert color="danger" className="mt-2">{result.errorMessage}</Alert>}
          <pre className="mb-0 small bg-light p-2 rounded" style={{ whiteSpace: 'pre-wrap' }}>
            {result.contentJson || '(empty)'}
          </pre>
        </div>
      )}

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load tools'}</Alert>}
      {!isLoading && !error && tools.length === 0 && (
        <Alert color="info">No MCP tools discovered yet. Register a server to start discovery.</Alert>
      )}

      {tools.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Server</th>
              <th>Description</th>
              <th>Status</th>
              {canManage && <th />}
            </tr>
          </thead>
          <tbody>
            {tools.map((tool) => (
              <tr key={tool.id}>
                <td className="fw-semibold">{tool.name}</td>
                <td>{tool.serverName}</td>
                <td>{tool.description || '—'}</td>
                <td>
                  <Badge color={tool.isEnabled ? 'success' : 'secondary'}>
                    {tool.isEnabled ? 'Enabled' : 'Disabled'}
                  </Badge>
                </td>
                {canManage && (
                  <td className="text-end">
                    <Button
                      color="link"
                      className="p-0"
                      onClick={() => {
                        setToolName(tool.name);
                        setServerId(tool.mcpServerId);
                        setArgumentsJson('{}');
                      }}
                    >
                      Use
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
