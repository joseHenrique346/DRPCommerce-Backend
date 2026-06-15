// src/components/Topbar/Topbar.jsx

import { ChevronDown } from 'lucide-react';

export default function Topbar({ pageTitle, pageBadge }) {
  return (
    <header className="topbar">
      <div className="topbar-title">
        <h1>{pageTitle}</h1>
        {pageBadge && <span className="topbar-badge">{pageBadge}</span>}
      </div>

      <div className="topbar-right">
        <div className="topbar-user" id="topbar-user-menu" role="button" aria-label="Menu do usuário">
          <div className="topbar-avatar" aria-hidden="true">AD</div>
          <div className="topbar-user-info">
            <span className="topbar-user-name">Olá, Admin</span>
            <span className="topbar-user-role">Administrador</span>
          </div>
          <ChevronDown size={14} style={{ color: 'var(--text-muted)', marginLeft: 4 }} />
        </div>
      </div>
    </header>
  );
}
