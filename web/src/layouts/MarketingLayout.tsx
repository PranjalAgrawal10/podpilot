import { Link, NavLink, Outlet } from 'react-router-dom';
import { Button } from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import { useTheme } from '../contexts/ThemeContext';

const NAV = [
  { to: '/features', label: 'Features' },
  { to: '/pricing', label: 'Pricing' },
  { to: '/documentation', label: 'Docs' },
  { to: '/roadmap', label: 'Roadmap' },
  { to: '/blog', label: 'Blog' },
  { to: '/community', label: 'Community' },
  { to: '/contact', label: 'Contact' },
];

export const MarketingLayout = () => {
  const { isAuthenticated } = useAuth();
  const { theme, toggleTheme } = useTheme();

  return (
    <div className="marketing-layout">
      <header className="marketing-nav">
        <Link to="/" className="marketing-brand">
          PodPilot
        </Link>
        <nav className="marketing-nav-links">
          {NAV.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `marketing-nav-link${isActive ? ' active' : ''}`
              }
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="marketing-nav-actions">
          <Button color="link" className="theme-toggle" onClick={toggleTheme}>
            {theme === 'light' ? '🌙' : '☀️'}
          </Button>
          {isAuthenticated ? (
            <Button tag={Link} to="/dashboard" color="primary" size="sm">
              Dashboard
            </Button>
          ) : (
            <>
              <Button tag={Link} to="/login" color="link" size="sm" className="text-decoration-none">
                Sign in
              </Button>
              <Button tag={Link} to="/register" color="primary" size="sm">
                Get started
              </Button>
            </>
          )}
        </div>
      </header>
      <main className="marketing-main">
        <Outlet />
      </main>
      <footer className="marketing-footer">
        <div>
          <strong>PodPilot</strong> — AI infrastructure autopilot for GPU pods.
        </div>
        <div className="marketing-footer-links">
          <Link to="/status">Status</Link>
          <Link to="/documentation">Docs</Link>
          <Link to="/contact">Contact</Link>
        </div>
      </footer>
    </div>
  );
};
