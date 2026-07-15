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
import { securityService } from '../services/securityService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type CreateIdentityProviderRequest } from '../types';

const PROVIDER_KINDS = ['EntraId', 'Google', 'Okta', 'Auth0', 'CustomOidc', 'Saml'];
const PROTOCOLS = ['Oidc', 'OAuth2', 'Saml2'];

export const IdentityProvidersPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.SecurityRead);
  const canManage = hasPermission(PERMISSIONS.SecurityManage);

  const [name, setName] = useState('');
  const [providerKind, setProviderKind] = useState('EntraId');
  const [protocol, setProtocol] = useState('Oidc');
  const [clientId, setClientId] = useState('');
  const [clientSecret, setClientSecret] = useState('');
  const [issuer, setIssuer] = useState('');
  const [scopes, setScopes] = useState('openid profile email');

  const { data: providers = [], isLoading, error } = useQuery({
    queryKey: ['identity-providers', currentOrganization?.id],
    queryFn: securityService.listIdentityProviders,
    enabled: !!currentOrganization?.id && canRead,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateIdentityProviderRequest) => securityService.createIdentityProvider(data),
    onSuccess: () => {
      toast.success('Identity provider created');
      setName('');
      setClientId('');
      setClientSecret('');
      setIssuer('');
      setScopes('openid profile email');
      setProviderKind('EntraId');
      setProtocol('Oidc');
      queryClient.invalidateQueries({ queryKey: ['identity-providers', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => securityService.deleteIdentityProvider(id),
    onSuccess: () => {
      toast.success('Identity provider deleted');
      queryClient.invalidateQueries({ queryKey: ['identity-providers', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage identity providers.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view identity providers.</Alert>;
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Security.Manage to create identity providers.');
      return;
    }
    createMutation.mutate({
      name,
      providerKind,
      protocol,
      clientId: clientId || undefined,
      clientSecret: clientSecret || undefined,
      issuer: issuer || undefined,
      scopes,
      isEnabled: true,
    });
  };

  return (
    <div>
      <h1 className="page-title mb-1">Identity providers</h1>
      <p className="text-muted mb-4">SSO providers for {currentOrganization.name}.</p>

      {canManage && (
        <Form onSubmit={onSubmit} className="mb-4 p-3 border rounded" style={{ maxWidth: 720 }}>
          <h2 className="h6 mb-3">Add provider</h2>
          <FormGroup>
            <Label>Name</Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </FormGroup>
          <FormGroup>
            <Label>Provider kind</Label>
            <Input type="select" value={providerKind} onChange={(e) => setProviderKind(e.target.value)}>
              {PROVIDER_KINDS.map((kind) => (
                <option key={kind} value={kind}>
                  {kind}
                </option>
              ))}
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Protocol</Label>
            <Input type="select" value={protocol} onChange={(e) => setProtocol(e.target.value)}>
              {PROTOCOLS.map((p) => (
                <option key={p} value={p}>
                  {p}
                </option>
              ))}
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Issuer</Label>
            <Input value={issuer} onChange={(e) => setIssuer(e.target.value)} />
          </FormGroup>
          <FormGroup>
            <Label>Client ID</Label>
            <Input value={clientId} onChange={(e) => setClientId(e.target.value)} />
          </FormGroup>
          <FormGroup>
            <Label>Client secret</Label>
            <Input
              type="password"
              value={clientSecret}
              onChange={(e) => setClientSecret(e.target.value)}
              autoComplete="new-password"
            />
          </FormGroup>
          <FormGroup>
            <Label>Scopes</Label>
            <Input value={scopes} onChange={(e) => setScopes(e.target.value)} />
          </FormGroup>
          <Button color="primary" type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? <Spinner size="sm" /> : 'Create'}
          </Button>
        </Form>
      )}

      {isLoading && (
        <div className="text-center py-5">
          <Spinner />
        </div>
      )}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load identity providers'}
        </Alert>
      )}
      {!isLoading && !error && providers.length === 0 && (
        <Alert color="info">No identity providers configured.</Alert>
      )}

      {providers.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Kind</th>
              <th>Protocol</th>
              <th>Issuer</th>
              <th>Status</th>
              {canManage && <th />}
            </tr>
          </thead>
          <tbody>
            {providers.map((provider) => (
              <tr key={provider.id}>
                <td>
                  <div className="fw-semibold">{provider.name}</div>
                  <div className="text-muted small">{provider.clientId || '—'}</div>
                </td>
                <td>{provider.providerKind}</td>
                <td>{provider.protocol}</td>
                <td className="text-break" style={{ maxWidth: 220 }}>
                  {provider.issuer || '—'}
                </td>
                <td>
                  <Badge color={provider.isEnabled ? 'success' : 'secondary'}>
                    {provider.isEnabled ? 'Enabled' : 'Disabled'}
                  </Badge>
                </td>
                {canManage && (
                  <td className="text-end">
                    <Button
                      color="danger"
                      size="sm"
                      outline
                      disabled={deleteMutation.isPending}
                      onClick={() => {
                        if (window.confirm(`Delete identity provider "${provider.name}"?`)) {
                          deleteMutation.mutate(provider.id);
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
