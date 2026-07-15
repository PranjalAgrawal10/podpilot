import { useState, type FormEvent } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Button, Form, FormGroup, Input, Label, Spinner, Table } from 'reactstrap';
import { routingService } from '../services/routingService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type SimulateRoutingResponse } from '../types';

const STRATEGIES = [
  '',
  'LowestCost',
  'LowestLatency',
  'HighestAccuracy',
  'Balanced',
  'ProviderPriority',
  'CustomRules',
];

export const RoutingSimulatePage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canManage = hasPermission(PERMISSIONS.RoutingManage);
  const canRead = hasPermission(PERMISSIONS.RoutingRead);

  const [prompt, setPrompt] = useState('Write a TypeScript function that merges two sorted arrays.');
  const [strategy, setStrategy] = useState('');
  const [result, setResult] = useState<SimulateRoutingResponse | null>(null);

  const historyQuery = useQuery({
    queryKey: ['routing-history', currentOrganization?.id],
    queryFn: () => routingService.listHistory(20),
    enabled: !!currentOrganization?.id && canRead,
  });

  const mutation = useMutation({
    mutationFn: routingService.simulate,
    onSuccess: (data) => {
      setResult(data);
      toast.success('Simulation complete');
      historyQuery.refetch();
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    if (!canManage) {
      toast.error('You need Routing.Manage to run simulations.');
      return;
    }
    mutation.mutate({
      prompt,
      strategy: strategy || undefined,
      path: '/v1/chat/completions',
    });
  };

  return (
    <div>
      <h1 className="page-title mb-3">Routing Simulation & Cost Estimator</h1>
      <p className="text-muted">Preview task classification, predicted model/provider, cost, and latency without executing inference.</p>

      <Form onSubmit={onSubmit} className="mb-4" style={{ maxWidth: 720 }}>
        <FormGroup>
          <Label>Prompt</Label>
          <Input type="textarea" rows={5} value={prompt} onChange={(e) => setPrompt(e.target.value)} required />
        </FormGroup>
        <FormGroup>
          <Label>Strategy override (optional)</Label>
          <Input type="select" value={strategy} onChange={(e) => setStrategy(e.target.value)} disabled={!canManage}>
            <option value="">Organization default</option>
            {STRATEGIES.filter(Boolean).map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Input>
        </FormGroup>
        {canManage && (
          <Button color="primary" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? <Spinner size="sm" /> : 'Simulate'}
          </Button>
        )}
      </Form>

      {result && (
        <div className="mb-4 p-3 border rounded">
          <h2 className="h5">Prediction</h2>
          <div className="row g-2">
            <div className="col-md-4"><strong>Task:</strong> {result.taskType} ({result.complexity})</div>
            <div className="col-md-4"><strong>Strategy:</strong> {result.strategy}</div>
            <div className="col-md-4"><strong>Score:</strong> {result.overallScore?.toFixed(1) ?? '—'}</div>
            <div className="col-md-4"><strong>Provider:</strong> {result.predictedProvider || '—'}</div>
            <div className="col-md-4"><strong>Model:</strong> {result.predictedModel || '—'}</div>
            <div className="col-md-4"><strong>Est. cost:</strong> ${result.estimatedCostUsd.toFixed(6)}</div>
            <div className="col-md-4"><strong>Est. latency:</strong> {result.estimatedLatencyMs} ms</div>
            <div className="col-md-4"><strong>Tokens in/out:</strong> {result.estimatedInputTokens}/{result.estimatedOutputTokens}</div>
          </div>
          <p className="mt-2 mb-0 text-muted">{result.decisionReason}</p>

          {result.rankedAlternatives.length > 0 && (
            <>
              <h3 className="h6 mt-3">Alternatives</h3>
              <Table size="sm" responsive>
                <thead>
                  <tr><th>Model</th><th>Provider</th><th>Score</th><th>Cost score</th><th>Latency score</th></tr>
                </thead>
                <tbody>
                  {result.rankedAlternatives.map((a) => (
                    <tr key={`${a.providerId}-${a.modelId}`}>
                      <td>{a.modelName}</td>
                      <td>{a.providerName}</td>
                      <td>{a.overallScore.toFixed(1)}</td>
                      <td>{a.costScore.toFixed(0)}</td>
                      <td>{a.latencyScore.toFixed(0)}</td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </>
          )}
        </div>
      )}

      <h2 className="h5">Recent routing history</h2>
      <Table size="sm" responsive>
        <thead>
          <tr>
            <th>When</th>
            <th>Task</th>
            <th>Model</th>
            <th>Provider</th>
            <th>Cost</th>
            <th>Latency</th>
            <th>Sim</th>
          </tr>
        </thead>
        <tbody>
          {(historyQuery.data || []).map((h) => (
            <tr key={h.id}>
              <td>{new Date(h.decidedAt).toLocaleString()}</td>
              <td>{h.taskType}</td>
              <td>{h.selectedModelName || '—'}</td>
              <td>{h.selectedProviderName || '—'}</td>
              <td>${h.estimatedCostUsd.toFixed(6)}</td>
              <td>{h.estimatedLatencyMs} ms</td>
              <td>{h.isSimulation ? 'Yes' : 'No'}</td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  );
};
