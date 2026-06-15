// src/components/Toast/ToastContainer.jsx

import { CheckCircle, XCircle, X } from 'lucide-react';

export default function ToastContainer({ toasts, removeToast }) {
  if (!toasts.length) return null;

  return (
    <div className="toast-container" role="alert" aria-live="polite">
      {toasts.map((t) => (
        <div key={t.id} className={`toast ${t.type}`}>
          {t.type === 'success' ? (
            <CheckCircle size={16} style={{ color: 'var(--success)', flexShrink: 0 }} />
          ) : (
            <XCircle size={16} style={{ color: 'var(--danger)', flexShrink: 0 }} />
          )}
          <span style={{ flex: 1 }}>{t.message}</span>
          <button
            onClick={() => removeToast(t.id)}
            style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)', padding: 0 }}
            aria-label="Fechar notificação"
          >
            <X size={14} />
          </button>
        </div>
      ))}
    </div>
  );
}
