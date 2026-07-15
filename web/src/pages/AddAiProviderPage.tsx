import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Button, Form, FormGroup, Input, Label, Spinner } from 'reactstrap';
import { aiProviderService } from '../services/aiProviderService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const AddAiProviderPage = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canCreate = hasPermission(PERMISSIONS.AiProviderCreate);

  const { data: kinds = [] } = useQuery({
    queryKey: ['ai-provider-kinds', currentOrganization?.id],
    queryFn: aiProviderService.listKinds,
    enabled: !!currentOrganization?.id && canCreate,
  });

  const [name, setName] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [providerKind, setProviderKind] = useState('OpenAi');
  const [apiKey, setApiKey] = useState('');
  const [baseUrl, setBaseUrl] = useState('');
  const [deploymentName, setDeploymentName] = useState('');
  const [apiVersion, setApiVersion] = useState('');

  const selectedKind = kinds.find((k) => k.providerKind === providerKind);

  const mutation = useMutation({
    mutationFn: aiProviderService.create,
    onSuccess: (provider) => {
      queryClient.invalidateQueries({ queryKey: ['ai-providers', currentOrganization?.id] });
      toast.success('AI provider created');
      navigate(`/ai/providers/${provider.id}`);
    },
    onError: (error: Error) => toast.error(error.message),
  });

  if (!canCreate) {
    return <Alert color="warning">You don&apos;t have permission to create AI providers.</Alert>;
  }

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    mutation.mutate({
      name,
      displayName,
      providerKind,
      apiKey,
      baseUrl: baseUrl || undefined,
      deploymentName: deploymentName || undefined,
      apiVersion: apiVersion || undefined,
      isEnabled: true,
      priority: 0,
    });
  };

  return (
    <div>
      <h1 className="page-title mb-3">Add AI Provider</h1>
      <Form onSubmit={onSubmit} style={{ maxWidth: 560 }}>
        <FormGroup>
          <Label>Provider kind</Label>
          <Input type="select" value={providerKind} onChange={(e) => setProviderKind(e.target.value)} innerRef={undefined}>
            {kinds.map((kind) => (
              <option key={kind.providerKind} value={kind.providerKind}>{kind.displayName}</option>
            ))}
          </Input>
        </FormGroup>
        <FormGroup>
          <Label>Name</Label>
          <Input value={name} onChange={(e) => setName(e.target.value)} required />
        </FormGroup>
        <FormGroup>
          <Label>Display name</Label>
          <Input value={displayName} onChange={(e) => setDisplayName(e.target.value)} required />
        </FormGroup>
        {(selectedKind?.requiresApiKey !== false) && (
          <FormGroup>
            <Label>API key</Label>
            <Input type="password" value={apiKey} onChange={(e) => setApiKey(e.target.value)} required={selectedKind?.requiresApiKey} />
          </FormGroup>
        )}
        <FormGroup>
          <Label>Base URL {selectedKind?.requiresBaseUrl ? '(required)' : '(optional)'}</Label>
          <Input
            value={baseUrl}
            onChange={(e) => setBaseUrl(e.target.value)}
            placeholder={selectedKind?.defaultBaseUrl}
            required={selectedKind?.requiresBaseUrl}
          />
        </FormGroup>
        {providerKind === 'AzureOpenAi' && (
          <>
            <FormGroup>
              <Label>Deployment name</Label>
              <Input value={deploymentName} onChange={(e) => setDeploymentName(e.target.value)} />
            </FormGroup>
            <FormGroup>
              <Label>API version</Label>
              <Input value={apiVersion} onChange={(e) => setApiVersion(e.target.value)} placeholder="2024-02-15-preview" />
            </FormGroup>
          </>
        )}
        <div className="d-flex gap-2">
          <Button color="primary" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? <Spinner size="sm" /> : 'Create'}
          </Button>
          <Button tag={Link} to="/ai/providers" color="secondary" outline>Cancel</Button>
        </div>
      </Form>
    </div>
  );
};
