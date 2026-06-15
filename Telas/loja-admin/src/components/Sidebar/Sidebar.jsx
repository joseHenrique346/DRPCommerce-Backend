// src/components/Sidebar/Sidebar.jsx

import { NavLink, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Package,
  ShoppingCart,
  Users,
  Settings,
  Zap,
} from 'lucide-react';

const navItems = [
  { to: '/',         icon: LayoutDashboard, label: 'Dashboard' },
  { to: '/products', icon: Package,         label: 'Produtos'  },
  { to: '/orders',   icon: ShoppingCart,    label: 'Pedidos'   },
  { to: '/customers',icon: Users,           label: 'Clientes'  },
  { to: '/settings', icon: Settings,        label: 'Config.'   },
];

export default function Sidebar() {
  const location = useLocation();

  return (
    <aside className="sidebar" aria-label="Navegação principal">
      {/* Logo */}
      <div className="sidebar-logo" title="DROP Commerce">
        <Zap />
      </div>

      {/* Navigation */}
      <nav className="sidebar-nav">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            className={({ isActive }) =>
              `sidebar-item${isActive ? ' active' : ''}`
            }
            title={label}
            id={`sidebar-nav-${label.toLowerCase().replace('.', '').trim()}`}
          >
            <Icon />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>

      <div className="sidebar-divider" />

      <div className="sidebar-bottom">
        {/* Avatar do usuário no rodapé da sidebar */}
        <div className="sidebar-item" style={{ cursor: 'default' }} title="Admin">
          <div
            style={{
              width: 28,
              height: 28,
              borderRadius: '50%',
              background: 'var(--grad-accent)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: 11,
              fontWeight: 700,
              color: '#fff',
              flexShrink: 0,
            }}
          >
            AD
          </div>
        </div>
      </div>
    </aside>
  );
}
