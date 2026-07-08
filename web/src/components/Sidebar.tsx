import { NavLink, useLocation } from 'react-router-dom';
import { Nav, NavItem } from 'reactstrap';
import { useOrganization } from '../contexts/OrganizationContext';

const SidebarNavLink = ({ to, label, icon }: { to: string; label: string; icon: string }) => {
  const location = useLocation();
  const isActive = location.pathname === to || location.pathname.startsWith(`${to}/`);

  return (
    <NavLink to={to} className={`nav-link ${isActive ? 'active' : ''}`}>
      <span className="nav-icon">{icon}</span>
      {label}
    </NavLink>
  );
};

export const Sidebar = () => {
  const { currentOrganization } = useOrganization();

  const settingsPath = currentOrganization
    ? `/organizations/${currentOrganization.id}/settings`
    : '/organizations';

  const navItems = [
    { to: '/dashboard', label: 'Dashboard', icon: '📊' },
    { to: '/organizations', label: 'Organizations', icon: '🏢' },
    { to: '/pods', label: 'Pods', icon: '🖥️' },
    { to: '/models', label: 'Models', icon: '🧠' },
    { to: '/gateway', label: 'AI Gateway', icon: '🤖' },
    { to: '/providers', label: 'Providers', icon: '🔌' },
    { to: '/members', label: 'Members', icon: '👥' },
    { to: '/profile', label: 'Profile', icon: '👤' },
    { to: settingsPath, label: 'Settings', icon: '⚙️' },
  ];

  return (
    <aside className="sidebar">
      <div className="sidebar-brand">
        <span className="brand-icon">🚀</span>
        <span className="brand-text">PodPilot</span>
      </div>
      <Nav vertical className="sidebar-nav">
        {navItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
      </Nav>
      <div className="sidebar-footer">
        <small className="text-muted">AI Infrastructure Autopilot</small>
      </div>
    </aside>
  );
};
