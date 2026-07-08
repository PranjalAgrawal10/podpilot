import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  CardTitle,
  Col,
  Form,
  FormGroup,
  Input,
  Label,
  Row,
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { gatewayService } from '../services/gatewayService';
import { useGatewayHub } from '../hooks/useGatewayHub';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS } from '../types';
import {
  buildGatewayUrl,
  GATEWAY_ENDPOINTS,
  getGatewayBaseUrl,
} from '../utils/gatewayUrl';

const copyText = async (value: string, label: string) => {
  try {
    await navigator.clipboard.writeText(value);
    toast.success(`${label} copied`);
  } catch {
    toast.error('Failed to copy to clipboard');
  }
};

export const GatewayPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const queryClient = useQueryClient();
  const canRead = hasPermission(PERMISSIONS.GatewayRead);
  const canManage = hasPermission(PERMISSIONS.GatewayManage);

  const [keyName, setKeyName] = useState('');
  const [isPersonal, setIsPersonal] = useState(false);
  const [revealedKey, setRevealedKey] = useState<string | null>(null);

  useGatewayHub(currentOrganization?.id);

  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['gateway-stats', currentOrganization?.id],
    queryFn: gatewayService.getStats,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 5000,
  });

  const { data: requests = [], isLoading: requestsLoading } = useQuery({
    queryKey: ['gateway-requests', currentOrganization?.id],
    queryFn: () => gatewayService.listRequests(25),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 5000,
  });

  const { data: apiKeys = [], isLoading: keysLoading } = useQuery({
    queryKey: ['gateway-api-keys', currentOrganization?.id],
    queryFn: gatewayService.listApiKeys,
    enabled: !!currentOrganization?.id && canRead,
  });

  const createKeyMutation = useMutation({
    mutationFn: () =>
      gatewayService.createApiKey({
        name: keyName,
        isPersonal,
      }),
    onSuccess: (created) => {
      setRevealedKey(created.plaintextKey ?? null);
      setKeyName('');
      queryClient.invalidateQueries({ queryKey: ['gateway-api-keys', currentOrganization?.id] });
      toast.success('Gateway API key created');
    },
    onError: () => toast.error('Failed to create API key'),
  });

  const revokeKeyMutation = useMutation({
    mutationFn: (keyId: string) => gatewayService.revokeApiKey(keyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['gateway-api-keys', currentOrganization?.id] });
      toast.success('API key revoked');
    },
  });

  const rotateKeyMutation = useMutation({
    mutationFn: (keyId: string) => gatewayService.rotateApiKey(keyId),
    onSuccess: (rotated) => {
      setRevealedKey(rotated.plaintextKey ?? null);
      queryClient.invalidateQueries({ queryKey: ['gateway-api-keys', currentOrganization?.id] });
      toast.success('API key rotated');
    },
  });

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view the AI Gateway.</Alert>;
  }

  const gatewayBaseUrl = getGatewayBaseUrl();
  const curlExample = `curl ${buildGatewayUrl('/chat/completions')} \\
  -H "Authorization: Bearer sk-your-api-key" \\
  -H "Content-Type: application/json" \\
  -d '{"model":"llama3","messages":[{"role":"user","content":"Hello"}]}'`;

  return (
    <div>
      <h1 className="page-title">AI Gateway</h1>
      <p className="text-muted mb-4">
        Monitor live inference traffic, manage API keys, and route models to GPU pods.
      </p>

      <Row className="g-4 mb-4">
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h6">Live Requests</CardTitle>
              <p className="stat-value">{statsLoading ? '—' : stats?.activeRequests ?? 0}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h6">Streaming</CardTitle>
              <p className="stat-value">{statsLoading ? '—' : stats?.streamingRequests ?? 0}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h6">Waiting Pods</CardTitle>
              <p className="stat-value">{statsLoading ? '—' : stats?.waitingPods ?? 0}</p>
            </CardBody>
          </Card>
        </Col>
        <Col md={3}>
          <Card className="stat-card">
            <CardBody>
              <CardTitle tag="h6">Avg Latency</CardTitle>
              <p className="stat-value">
                {statsLoading ? '—' : `${Math.round(stats?.averageLatencyMs ?? 0)}ms`}
              </p>
            </CardBody>
          </Card>
        </Col>
      </Row>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">API Endpoints</CardTitle>
          <p className="text-muted mb-3">
            Use these URLs in Cursor, Claude Code, or any OpenAI/Anthropic-compatible client.
          </p>

          <div className="gateway-endpoint-row mb-3">
            <span className="text-muted">Base URL</span>
            <div className="gateway-endpoint-value">
              <code>{gatewayBaseUrl}</code>
              <Button
                size="sm"
                color="secondary"
                outline
                onClick={() => copyText(gatewayBaseUrl, 'Base URL')}
              >
                Copy
              </Button>
            </div>
          </div>

          <Table responsive className="gateway-endpoints-table mb-3">
            <thead>
              <tr>
                <th>Method</th>
                <th>Endpoint</th>
                <th>Description</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {GATEWAY_ENDPOINTS.map((endpoint) => {
                const fullUrl = buildGatewayUrl(endpoint.path);
                return (
                  <tr key={endpoint.path}>
                    <td>
                      <Badge color={endpoint.method === 'GET' ? 'info' : 'primary'}>
                        {endpoint.method}
                      </Badge>
                    </td>
                    <td>
                      <code>{fullUrl}</code>
                    </td>
                    <td className="text-muted">{endpoint.description}</td>
                    <td className="text-end">
                      <Button
                        size="sm"
                        color="secondary"
                        outline
                        onClick={() => copyText(fullUrl, 'Endpoint URL')}
                      >
                        Copy
                      </Button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </Table>

          <div className="gateway-endpoint-row mb-3">
            <span className="text-muted">Authentication</span>
            <div className="gateway-endpoint-value">
              <code>Authorization: Bearer sk-…</code>
              <span className="text-muted">or</span>
              <code>x-api-key: sk-…</code>
            </div>
          </div>

          <div>
            <div className="d-flex justify-content-between align-items-center mb-2">
              <span className="text-muted">Example (OpenAI chat)</span>
              <Button
                size="sm"
                color="secondary"
                outline
                onClick={() => copyText(curlExample, 'cURL example')}
              >
                Copy
              </Button>
            </div>
            <pre className="gateway-curl-example mb-0">{curlExample}</pre>
          </div>
        </CardBody>
      </Card>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">Live Requests</CardTitle>
          {requestsLoading ? (
            <LoadingSpinner />
          ) : (
            <Table responsive hover className="mt-3">
              <thead>
                <tr>
                  <th>Path</th>
                  <th>Model</th>
                  <th>Status</th>
                  <th>Wake</th>
                  <th>Latency</th>
                  <th>Started</th>
                </tr>
              </thead>
              <tbody>
                {requests.length === 0 ? (
                  <tr>
                    <td colSpan={6} className="text-muted">
                      No gateway requests yet.
                    </td>
                  </tr>
                ) : (
                  requests.map((request) => (
                    <tr key={request.id}>
                      <td>{request.path}</td>
                      <td>{request.model ?? '—'}</td>
                      <td>
                        <Badge color={request.status === 'Failed' ? 'danger' : 'primary'}>
                          {request.status}
                        </Badge>
                      </td>
                      <td>{request.wakeTriggered ? 'Yes' : 'No'}</td>
                      <td>{request.totalLatencyMs ? `${request.totalLatencyMs}ms` : '—'}</td>
                      <td>{new Date(request.startedAt).toLocaleString()}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <CardTitle tag="h5">API Keys</CardTitle>

          {revealedKey && (
            <Alert color="success" className="mt-3">
              <strong>Copy your API key now — it will not be shown again:</strong>
              <pre className="mt-2 mb-0">{revealedKey}</pre>
            </Alert>
          )}

          {canManage && (
            <Form
              className="mt-3"
              onSubmit={(event) => {
                event.preventDefault();
                createKeyMutation.mutate();
              }}
            >
              <Row className="g-3 align-items-end">
                <Col md={4}>
                  <FormGroup>
                    <Label for="keyName">Key Name</Label>
                    <Input
                      id="keyName"
                      value={keyName}
                      onChange={(event) => setKeyName(event.target.value)}
                      required
                    />
                  </FormGroup>
                </Col>
                <Col md={3}>
                  <FormGroup check className="mb-2">
                    <Input
                      type="checkbox"
                      id="isPersonal"
                      checked={isPersonal}
                      onChange={(event) => setIsPersonal(event.target.checked)}
                    />
                    <Label check for="isPersonal">
                      Personal key
                    </Label>
                  </FormGroup>
                </Col>
                <Col md={3}>
                  <Button color="primary" type="submit" disabled={createKeyMutation.isPending}>
                    Create API Key
                  </Button>
                </Col>
              </Row>
            </Form>
          )}

          {keysLoading ? (
            <LoadingSpinner />
          ) : (
            <Table responsive hover className="mt-3">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Prefix</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Limits</th>
                  {canManage && <th>Actions</th>}
                </tr>
              </thead>
              <tbody>
                {apiKeys.map((key) => (
                  <tr key={key.id}>
                    <td>{key.name}</td>
                    <td>
                      <code>{key.keyPrefix}…</code>
                    </td>
                    <td>{key.keyType}</td>
                    <td>
                      <Badge color={key.isRevoked ? 'secondary' : 'success'}>
                        {key.isRevoked ? 'Revoked' : 'Active'}
                      </Badge>
                    </td>
                    <td>
                      {key.rateLimitPerMinute}/min · {key.rateLimitPerDay}/day
                    </td>
                    {canManage && (
                      <td>
                        {!key.isRevoked && (
                          <>
                            <Button
                              size="sm"
                              color="warning"
                              className="me-2"
                              onClick={() => rotateKeyMutation.mutate(key.id)}
                            >
                              Rotate
                            </Button>
                            <Button
                              size="sm"
                              color="danger"
                              onClick={() => revokeKeyMutation.mutate(key.id)}
                            >
                              Revoke
                            </Button>
                          </>
                        )}
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
