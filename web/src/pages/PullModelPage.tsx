import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Alert, Button, Card, CardBody, CardTitle, Form, FormGroup, Input, Label } from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { modelService } from '../services/modelService';
import { podService } from '../services/podService';
import { ModelSelector } from '../components/models/ModelSelector';
import { PERMISSIONS } from '../types';

export const PullModelPage = () => {
  const navigate = useNavigate();
  const { currentOrganization, hasPermission } = useOrganization();
  const canPull = hasPermission(PERMISSIONS.ModelPull);

  const [podId, setPodId] = useState('');
  const [model, setModel] = useState('llama3:latest');

  const { data: pods = [] } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id,
  });

  const pullMutation = useMutation({
    mutationFn: () => modelService.pull({ podId, model }),
    onSuccess: () => {
      toast.success('Model pull started');
      navigate('/models/downloads');
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to start pull'),
  });

  if (!canPull) {
    return <Alert color="warning">You do not have permission to pull models.</Alert>;
  }

  return (
    <div>
      <div className="mb-4">
        <Button tag={Link} to="/models" color="link" className="p-0 mb-2">
          ← Back to Models
        </Button>
        <h1 className="page-title">Pull Model</h1>
        <p className="text-muted">Download a model from the Ollama registry to a GPU pod.</p>
      </div>

      <Card>
        <CardBody>
          <CardTitle tag="h5">Model Pull</CardTitle>
          <Form
            onSubmit={(event) => {
              event.preventDefault();
              pullMutation.mutate();
            }}
          >
            <ModelSelector pods={pods} value={podId} onChange={setPodId} />
            <FormGroup>
              <Label for="modelName">Model Reference</Label>
              <Input
                id="modelName"
                value={model}
                onChange={(event) => setModel(event.target.value)}
                placeholder="llama3:latest"
                required
              />
              <small className="text-muted">Use Ollama format, e.g. mistral:7b or qwen2.5:latest</small>
            </FormGroup>
            <Button color="primary" type="submit" disabled={!podId || pullMutation.isPending}>
              Start Pull
            </Button>
          </Form>
        </CardBody>
      </Card>
    </div>
  );
};
