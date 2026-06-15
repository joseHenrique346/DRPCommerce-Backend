// src/pages/ComingSoon/ComingSoon.jsx
// Tela genérica para seções ainda não implementadas

import { Clock } from 'lucide-react';
import Topbar from '../../components/Topbar/Topbar.jsx';

export default function ComingSoon({ pageTitle, pageIcon: PageIcon, description }) {
  return (
    <>
      <Topbar pageTitle={pageTitle} />

      <main className="page-container" style={{ height: 'calc(100% - var(--topbar-h))' }}>
        <div className="coming-soon-page">
          <div className="coming-soon-icon">
            {PageIcon ? <PageIcon size={36} /> : <Clock size={36} />}
          </div>

          <div className="coming-soon-badge">
            <Clock size={12} />
            Disponível em Breve
          </div>

          <h2>{pageTitle}</h2>

          <p>
            {description ||
              'Esta seção está em desenvolvimento e estará disponível em uma próxima versão. Fique ligado nas atualizações!'}
          </p>

          <div
            style={{
              display: 'flex',
              gap: 8,
              marginTop: 8,
            }}
          >
            {[1, 2, 3].map((i) => (
              <div
                key={i}
                style={{
                  width: 8,
                  height: 8,
                  borderRadius: '50%',
                  background: i === 1 ? 'var(--accent)' : 'var(--border)',
                  animation: i === 1 ? 'pulse 1.5s ease-in-out infinite' : 'none',
                }}
              />
            ))}
          </div>
        </div>
      </main>

      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; transform: scale(1); }
          50%       { opacity: 0.4; transform: scale(0.8); }
        }
      `}</style>
    </>
  );
}
