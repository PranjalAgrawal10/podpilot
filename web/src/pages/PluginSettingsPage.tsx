import { useEffect, useState, type FormEvent } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Button, Form, FormGroup, Input, Label, Spinner } from 'reactstrap';
import { pluginService } from '../services/pluginService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const PluginSettingsPage = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PluginRead);
  const canManage = hasPermission(PERMISSIONS.PluginManage);
  const installationId = id;

  const { data: settings = [], isLoading, error } = useQuery({
    queryKey: ['plugin-settings', currentOrganization?.id, installationId],
    queryFn: () => pluginService.getSettings(installationId!),
    enabled: !!currentOrganization?.id && !!installationId && canRead,
  });

  const [values, setValues] = useState<Record<string, string>>({});

  useEffect(() => {
    const next: Record<string, string> = {};
    for (const setting of settings) {
      next[setting.key] = setting.isSecret ? '' : (setting.value ?? '');
    }
    setValues(next);
  }, [settings]);

  const mutation = useMutation({
    mutationFn: () => {
      const payload: Record<string, string> = {};
      const secretKeys: string[] = [];
      for (const setting of settings) {
        const value = values[setting.key] ?? '';
        if (setting.isSecret && !value) continue;
        payload[setting.key] = value;
        if (setting.isSecret) secretKeys.push(setting.key);
      }
      return pluginService.updateSettings(installationId!, {
        settings: payload,
        secretKeys,
      });
    },
    onSuccess: () => {
      toast.success('Settings saved');
      queryClient.invalidateQueries({
        queryKey: ['plugin-settings', currentOrganization?.id, installationId],
      });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;
  if (isLoading) return <div className="text-center py-5"><Spinner /></div>;
  if (error) return <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load settings'}</Alert>;

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Plugin.Manage to update settings.');
      return;
    }
    mutation.mutate();
  };

  return (
    <div style={{ maxWidth: 640 }}>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="page-title mb-0">Plugin Settings</h1>
        <Button tag={Link} to={`/plugins/${installationId}`} color="secondary" outline size="sm">
          Back
        </Button>
      </div>
      <p className="text-muted">Edit configuration keys for this installation. Secret values are never returned; leave blank to keep the existing value.</p>

      {settings.length === 0 && <Alert color="info">No settings configured for this plugin.</Alert>}

      {settings.length > 0 && (
        <Form onSubmit={onSubmit}>
          {settings.map((setting) => (
            <FormGroup key={setting.key}>
              <Label>
                {setting.key}
                {setting.isSecret && (
                  <span className="text-muted small ms-2">
                    (secret{setting.hasValue ? ', set' : ''})
                  </span>
                )}
              </Label>
              <Input
                type={setting.isSecret ? 'password' : 'text'}
                value={values[setting.key] ?? ''}
                placeholder={setting.isSecret && setting.hasValue ? '••••••••' : undefined}
                onChange={(e) => setValues((prev) => ({ ...prev, [setting.key]: e.target.value }))}
                disabled={!canManage}
              />
            </FormGroup>
          ))}
          {canManage && (
            <Button color="primary" type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? <Spinner size="sm" /> : 'Save settings'}
            </Button>
          )}
        </Form>
      )}
    </div>
  );
};
