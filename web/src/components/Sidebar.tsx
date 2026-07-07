import { NavLink, useLocation } from 'react-router-dom';
import { Nav, NavItem } from 'reactstrap';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: '📊' },
  { to: '/profile', label: 'Profile', icon: '👤' },
];

const SidebarNavLink = ({ to, label, icon }: { to: string; label: string; icon: string }) => {
  const location = useLocation();
  const isActive = location.pathname === to;

  return (
    <NavLink to={to} className={`nav-link ${isActive ? 'active' : ''}`}>
      <span className="nav-icon">{icon}</span>
      {label}
    </NavLink>
  );
};

export const Sidebar = () => (
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
