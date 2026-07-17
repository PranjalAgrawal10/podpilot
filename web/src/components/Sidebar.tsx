import { NavLink, useLocation } from 'react-router-dom';
import { Nav, NavItem } from 'reactstrap';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

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
  const { currentOrganization, hasPermission } = useOrganization();
  const canReadDeployments = hasPermission(PERMISSIONS.DeploymentRead);

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

  const deployNavItems = [
    { to: '/deployments', label: 'Deployments', icon: '🚀' },
    { to: '/deployments/create', label: 'Create', icon: '➕' },
    { to: '/deployments/templates', label: 'Templates', icon: '📦' },
    { to: '/deployments/models', label: 'Models', icon: '📚' },
    { to: '/deployments/gpus', label: 'GPUs', icon: '🎮' },
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

  const aiEngineNavItems = [
    { to: '/ai/providers', label: 'AI Providers', icon: '🧬' },
    { to: '/ai/models', label: 'Model Registry', icon: '📚' },
    { to: '/ai/routing', label: 'Fallback Policies', icon: '🔁' },
    { to: '/ai/health', label: 'AI Health', icon: '🩺' },
    { to: '/routing', label: 'Smart Routing', icon: '🔀' },
    { to: '/routing/policies', label: 'Routing Strategy', icon: '🎯' },
    { to: '/routing/models', label: 'Model Ranking', icon: '🏆' },
    { to: '/routing/simulate', label: 'Simulate', icon: '🧪' },
  ];

  const extensibilityNavItems = [
    { to: '/plugins', label: 'Plugins', icon: '🧩' },
    { to: '/plugins/marketplace', label: 'Marketplace', icon: '🛒' },
    { to: '/mcp/servers', label: 'MCP Servers', icon: '🔗' },
    { to: '/mcp/tools', label: 'MCP Tools', icon: '🛠️' },
  ];

  const securityNavItems = [
    { to: '/security', label: 'Overview', icon: '🛡️' },
    { to: '/security/audit', label: 'Audit Logs', icon: '📋' },
    { to: '/security/secrets', label: 'Secrets', icon: '🔑' },
    { to: '/security/identity-providers', label: 'Identity Providers', icon: '🪪' },
    { to: '/security/policies', label: 'Policies', icon: '📜' },
    { to: '/security/compliance', label: 'Compliance', icon: '✅' },
    { to: '/security/sessions', label: 'Sessions', icon: '💻' },
    { to: '/security/devices', label: 'Trusted Devices', icon: '📱' },
  ];

  const commercialNavItems = [
    { to: '/billing', label: 'Billing', icon: '💳' },
    { to: '/billing/usage', label: 'Usage', icon: '📐' },
    { to: '/downloads', label: 'Downloads', icon: '⬇️' },
    { to: '/docs', label: 'Documentation', icon: '📖' },
    { to: '/system-status', label: 'System Status', icon: '🟢' },
    { to: '/onboarding', label: 'Onboarding', icon: '🧭' },
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
        {canReadDeployments && (
          <>
            <NavItem>
              <SidebarSection title="Deployments" />
            </NavItem>
            {deployNavItems.map((item) => (
              <NavItem key={item.to}>
                <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
              </NavItem>
            ))}
          </>
        )}
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
        <NavItem>
          <SidebarSection title="AI Engine" />
        </NavItem>
        {aiEngineNavItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
        <NavItem>
          <SidebarSection title="Extensibility" />
        </NavItem>
        {extensibilityNavItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
        <NavItem>
          <SidebarSection title="Security" />
        </NavItem>
        {securityNavItems.map((item) => (
          <NavItem key={item.to}>
            <SidebarNavLink to={item.to} label={item.label} icon={item.icon} />
          </NavItem>
        ))}
        <NavItem>
          <SidebarSection title="Commercial" />
        </NavItem>
        {commercialNavItems.map((item) => (
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
