import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import {
  Card,
  CardBody,
  Form,
  FormGroup,
  Label,
  Input,
  Button,
  Spinner,
  Alert,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { providerService } from '../services/providerService';
import { RegionSelector } from '../components/providers/RegionSelector';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type UpdateProviderRequest } from '../types';

export const EditProviderPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { hasPermission } = useOrganization();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [apiKeyChanged, setApiKeyChanged] = useState(false);

  const canUpdate = hasPermission(PERMISSIONS.ProviderUpdate);

  const { data: provider, isLoading, error } = useQuery({
    queryKey: ['provider', id],
    queryFn: () => providerService.getById(id!),
    enabled: !!id && canUpdate,
  });

  const { data: regions = [] } = useQuery({
    queryKey: ['provider-regions', id],
    queryFn: () => providerService.getRegions(id!),
    enabled: !!id && canUpdate,
  });

  const { register, handleSubmit, setValue, watch, formState: { errors } } = useForm<UpdateProviderRequest>({
    values: provider
      ? {
          name: provider.name,
          displayName: provider.displayName,
          description: provider.description ?? '',
          defaultRegion: provider.defaultRegion ?? '',
          isEnabled: provider.isEnabled,
        }
      : undefined,
  });

  const nameField = register('name', { required: 'Name is required' });
  const displayNameField = register('displayName', { required: 'Display name is required' });
  const descriptionField = register('description');
  const apiKeyField = register('apiKey');
  const defaultRegionField = register('defaultRegion');
  const isEnabledField = register('isEnabled');

  const defaultRegion = watch('defaultRegion');

  const onSubmit = async (data: UpdateProviderRequest) => {
    if (!id) return;

    if (apiKeyChanged && !data.apiKey) {
      toast.error('Enter a new API key or leave the field empty to keep the existing key');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload: UpdateProviderRequest = { ...data };
      if (!apiKeyChanged) {
        delete payload.apiKey;
      }
      await providerService.update(id, payload);
      await queryClient.invalidateQueries({ queryKey: ['providers'] });
      await queryClient.invalidateQueries({ queryKey: ['provider', id] });
      toast.success('Provider updated');
      navigate(`/providers/${id}`);
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to update provider';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!canUpdate) {
    return (
      <Alert color="warning">
        You don&apos;t have permission to edit providers.
      </Alert>
    );
  }

  if (isLoading) {
    return (
      <div className="text-center py-5">
        <Spinner />
      </div>
    );
  }

  if (error || !provider) {
    return (
      <Alert color="danger">
        {error instanceof Error ? error.message : 'Provider not found'}
      </Alert>
    );
  }

  return (
    <div>
      <h1 className="page-title">Edit Provider</h1>
      <p className="text-muted mb-4">{provider.displayName}</p>

      <Card className="auth-card provider-form-card" style={{ maxWidth: 640 }}>
        <CardBody>
          <Form onSubmit={handleSubmit(onSubmit)}>
            <FormGroup>
              <Label for="editName">Name</Label>
              <Input
                id="editName"
                innerRef={nameField.ref}
                name={nameField.name}
                onBlur={nameField.onBlur}
                onChange={nameField.onChange}
                invalid={!!errors.name}
              />
              {errors.name && (
                <div className="invalid-feedback d-block">{errors.name.message}</div>
              )}
            </FormGroup>

            <FormGroup>
              <Label for="editDisplayName">Display Name</Label>
              <Input
                id="editDisplayName"
                innerRef={displayNameField.ref}
                name={displayNameField.name}
                onBlur={displayNameField.onBlur}
                onChange={displayNameField.onChange}
                invalid={!!errors.displayName}
              />
              {errors.displayName && (
                <div className="invalid-feedback d-block">{errors.displayName.message}</div>
              )}
            </FormGroup>

            <FormGroup>
              <Label for="editDescription">Description</Label>
              <Input
                id="editDescription"
                type="textarea"
                rows={3}
                innerRef={descriptionField.ref}
                name={descriptionField.name}
                onBlur={descriptionField.onBlur}
                onChange={descriptionField.onChange}
              />
            </FormGroup>

            {regions.length > 0 ? (
              <RegionSelector
                id="editDefaultRegion"
                regions={regions}
                value={defaultRegion}
                onChange={(regionId) => setValue('defaultRegion', regionId)}
              />
            ) : (
              <FormGroup>
                <Label for="editDefaultRegion">Default Region</Label>
                <Input
                  id="editDefaultRegion"
                  innerRef={defaultRegionField.ref}
                  name={defaultRegionField.name}
                  onBlur={defaultRegionField.onBlur}
                  onChange={defaultRegionField.onChange}
                />
              </FormGroup>
            )}

            <FormGroup check className="mb-3">
              <Input
                id="editIsEnabled"
                type="checkbox"
                innerRef={isEnabledField.ref}
                name={isEnabledField.name}
                onBlur={isEnabledField.onBlur}
                onChange={isEnabledField.onChange}
              />
              <Label for="editIsEnabled" check>
                Provider enabled
              </Label>
            </FormGroup>

            <FormGroup>
              <Label for="editApiKey">API Key (leave empty to keep current)</Label>
              <Input
                id="editApiKey"
                type="password"
                placeholder="Enter new API key to rotate"
                innerRef={apiKeyField.ref}
                name={apiKeyField.name}
                onBlur={apiKeyField.onBlur}
                onChange={(e) => {
                  apiKeyField.onChange(e);
                  setApiKeyChanged(!!e.target.value);
                }}
              />
              {apiKeyChanged && (
                <small className="text-muted">
                  After saving, re-validate the connection from the provider details page.
                </small>
              )}
            </FormGroup>

            <div className="d-flex gap-2">
              <Button color="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Save Changes'}
              </Button>
              <Button color="secondary" outline onClick={() => navigate(`/providers/${id}`)}>
                Cancel
              </Button>
            </div>
          </Form>
        </CardBody>
      </Card>
    </div>
  );
};
