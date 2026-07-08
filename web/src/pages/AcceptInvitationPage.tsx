import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Card, CardBody, Button, Spinner, Alert } from 'reactstrap';
import { toast } from 'react-toastify';
import { invitationService } from '../services/invitationService';
import { useAuth } from '../contexts/AuthContext';

export const AcceptInvitationPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const { isAuthenticated, isLoading: authLoading, refreshUser } = useAuth();
  const [isAccepting, setIsAccepting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const token = searchParams.get('token');

  useEffect(() => {
    if (!authLoading && !isAuthenticated) {
      const returnUrl = token ? `/invitations/accept?token=${encodeURIComponent(token)}` : '/login';
      navigate(`/login?returnUrl=${encodeURIComponent(returnUrl)}`);
    }
  }, [authLoading, isAuthenticated, navigate, token]);

  const handleAccept = async () => {
    if (!token) return;
    setIsAccepting(true);
    setError(null);
    try {
      await invitationService.accept({ token });
      await refreshUser();
      toast.success('Invitation accepted! Welcome to the organization.');
      navigate('/organizations');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to accept invitation';
      setError(message);
      toast.error(message);
    } finally {
      setIsAccepting(false);
    }
  };

  if (authLoading) {
    return (
      <div className="text-center py-5">
        <Spinner />
      </div>
    );
  }

  if (!token) {
    return (
      <Alert color="danger">Invalid invitation link. No token provided.</Alert>
    );
  }

  return (
    <div>
      <h1 className="page-title">Accept Invitation</h1>
      <Card className="auth-card" style={{ maxWidth: 480 }}>
        <CardBody>
          <p className="mb-4">
            You&apos;ve been invited to join an organization on PodPilot. Click below to accept
            the invitation.
          </p>

          {error && <Alert color="danger">{error}</Alert>}

          <Button color="primary" disabled={isAccepting} onClick={() => void handleAccept()}>
            {isAccepting ? 'Accepting...' : 'Accept Invitation'}
          </Button>
        </CardBody>
      </Card>
    </div>
  );
};
