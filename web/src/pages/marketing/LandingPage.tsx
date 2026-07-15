import { Link } from 'react-router-dom';
import { Button } from 'reactstrap';
import { useAuth } from '../../contexts/AuthContext';

export const LandingPage = () => {
  const { isAuthenticated } = useAuth();

  return (
    <div className="marketing-landing">
      <section className="marketing-hero">
        <div className="marketing-hero-content">
          <p className="marketing-brand-hero">PodPilot</p>
          <h1 className="marketing-headline">GPU pods that wake when you need them.</h1>
          <p className="marketing-lead">
            Autopilot for idle shutdown, gateway routing, and multi-provider GPU fleets — without the
            ops grind.
          </p>
          <div className="marketing-cta-group">
            {isAuthenticated ? (
              <Button tag={Link} to="/dashboard" color="primary" size="lg">
                Open dashboard
              </Button>
            ) : (
              <Button tag={Link} to="/register" color="primary" size="lg">
                Start free
              </Button>
            )}
            <Button tag={Link} to="/features" color="secondary" outline size="lg">
              See features
            </Button>
          </div>
        </div>
        <div className="marketing-hero-visual" aria-hidden="true">
          <div className="marketing-hero-grid" />
          <div className="marketing-hero-panel">
            <span className="marketing-hero-panel-label">Fleet</span>
            <span className="marketing-hero-panel-stat">12 pods · 3 awake</span>
            <span className="marketing-hero-panel-label">Gateway</span>
            <span className="marketing-hero-panel-stat">42 ms p50</span>
          </div>
        </div>
      </section>
    </div>
  );
};
