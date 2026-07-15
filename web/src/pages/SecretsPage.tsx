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
import { secretsService } from '../services/secretsService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type CreateSecretRequest } from '../types';

const SECRET_KINDS = [
  'Generic',
  'ProviderApiKey',
  'AiProviderCredential',
  'JwtSigningKey',
  'DatabasePassword',
  'RedisPassword',
];

const BACKEND_KINDS = ['LocalEncrypted', 'AzureKeyVault', 'AwsSecretsManager', 'HashiCorpVault'];

export const SecretsPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.SecretsRead);
  const canManage = hasPermission(PERMISSIONS.SecretsManage);

  const [name, setName] = useState('');
  const [secretKind, setSecretKind] = useState('Generic');
  const [backendKind, setBackendKind] = useState('LocalEncrypted');
  const [value, setValue] = useState('');

  const { data: secrets = [], isLoading, error } = useQuery({
    queryKey: ['secrets', currentOrganization?.id],
    queryFn: secretsService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const createMutation = useMutation({
    mutationFn: (data: CreateSecretRequest) => secretsService.create(data),
    onSuccess: () => {
      toast.success('Secret created');
      setName('');
      setValue('');
      setSecretKind('Generic');
      setBackendKind('LocalEncrypted');
      queryClient.invalidateQueries({ queryKey: ['secrets', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['security-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => secretsService.delete(id),
    onSuccess: () => {
      toast.success('Secret deleted');
      queryClient.invalidateQueries({ queryKey: ['secrets', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['security-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage secrets.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view secrets.</Alert>;
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Secrets.Manage to create secrets.');
      return;
    }
    createMutation.mutate({ name, secretKind, backendKind, value });
  };

  return (
    <div>
      <h1 className="page-title mb-1">Secrets</h1>
      <p className="text-muted mb-4">
        Store secret metadata for {currentOrganization.name}. Values are never shown after create.
      </p>

      {canManage && (
        <Form onSubmit={onSubmit} className="mb-4 p-3 border rounded" style={{ maxWidth: 720 }}>
          <h2 className="h6 mb-3">Create secret</h2>
          <FormGroup>
            <Label>Name</Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </FormGroup>
          <FormGroup>
            <Label>Kind</Label>
            <Input type="select" value={secretKind} onChange={(e) => setSecretKind(e.target.value)}>
              {SECRET_KINDS.map((kind) => (
                <option key={kind} value={kind}>
                  {kind}
                </option>
              ))}
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Backend</Label>
            <Input type="select" value={backendKind} onChange={(e) => setBackendKind(e.target.value)}>
              {BACKEND_KINDS.map((kind) => (
                <option key={kind} value={kind}>
                  {kind}
                </option>
              ))}
            </Input>
          </FormGroup>
          <FormGroup>
            <Label>Value</Label>
            <Input
              type="password"
              value={value}
              onChange={(e) => setValue(e.target.value)}
              required
              autoComplete="new-password"
            />
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
        <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load secrets'}</Alert>
      )}
      {!isLoading && !error && secrets.length === 0 && (
        <Alert color="info">No secrets yet.</Alert>
      )}

      {secrets.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Kind</th>
              <th>Backend</th>
              <th>Version</th>
              <th>Status</th>
              <th>Expires</th>
              {canManage && <th />}
            </tr>
          </thead>
          <tbody>
            {secrets.map((secret) => (
              <tr key={secret.id}>
                <td className="fw-semibold">{secret.name}</td>
                <td>{secret.secretKind}</td>
                <td>{secret.backendKind}</td>
                <td>{secret.version}</td>
                <td>
                  <Badge color={secret.isEnabled ? 'success' : 'secondary'}>
                    {secret.isEnabled ? 'Enabled' : 'Disabled'}
                  </Badge>
                </td>
                <td>
                  {secret.expiresAt ? new Date(secret.expiresAt).toLocaleDateString() : '—'}
                </td>
                {canManage && (
                  <td className="text-end">
                    <Button
                      color="danger"
                      size="sm"
                      outline
                      disabled={deleteMutation.isPending}
                      onClick={() => {
                        if (window.confirm(`Delete secret "${secret.name}"?`)) {
                          deleteMutation.mutate(secret.id);
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
