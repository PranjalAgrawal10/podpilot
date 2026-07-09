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

const SidebarSection = ({ title }: { title: string }) => (
  <div className="sidebar-section-label">{title}</div>
);

export const Sidebar = () => {
  const { currentOrganization } = useOrganization();

  const settingsPath = currentOrganization
    ? `/organizations/${currentOrganization.id}/settings`
    : '/organizations';

  const mainNavItems = [
    { to: '/dashboard', label: 'Dashboard', icon: '📊' },
    { to: '/organizations', label: 'Organizations', icon: '🏢' },
    { to: '/pods', label: 'Pods', icon: '🖥️' },
    { to: '/models', label: 'Models', icon: '🧠' },
    { to: '/gateway', label: 'AI Gateway', icon: '🤖' },
    { to: '/scheduler', label: 'Scheduler', icon: '⏱️' },
    { to: '/providers', label: 'Providers', icon: '🔌' },
  ];

  const orchestrationNavItems = [
    { to: '/orchestration/pools', label: 'Pod Pools', icon: '🏊' },
    { to: '/orchestration/scaling', label: 'Auto Scaling', icon: '📈' },
    { to: '/orchestration/capacity', label: 'Capacity', icon: '📉' },
    { to: '/orchestration/health', label: 'Health', icon: '💚' },
    { to: '/orchestration/load-balancer', label: 'Load Balancer', icon: '⚖️' },
  ];

  const observabilityNavItems = [
    { to: '/observability', label: 'Overview', icon: '📡' },
    { to: '/observability/metrics', label: 'Metrics', icon: '📈' },
    { to: '/observability/analytics', label: 'Analytics', icon: '📊' },
    { to: '/observability/health', label: 'Health', icon: '❤️' },
    { to: '/observability/alerts', label: 'Alerts', icon: '🔔' },
    { to: '/observability/costs', label: 'Costs', icon: '💰' },
  ];

  const accountNavItems = [
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
        {mainNavItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
        <NavItem>
          <SidebarSection title="Orchestration" />
        </NavItem>
        {orchestrationNavItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
        <NavItem>
          <SidebarSection title="Observability" />
        </NavItem>
        {observabilityNavItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
        {accountNavItems.map((item) => (
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
