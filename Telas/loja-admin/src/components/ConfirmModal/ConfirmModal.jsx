// src/components/ConfirmModal/ConfirmModal.jsx

import { Trash2 } from 'lucide-react';

export default function ConfirmModal({ title, description, onConfirm, onCancel, loading }) {
  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="confirm-title">
      <div className="confirm-modal">
        <div className="confirm-icon" aria-hidden="true">
          <Trash2 />
        </div>
        <h3 id="confirm-title">{title}</h3>
        <p>{description}</p>
        <div className="confirm-actions">
          <button className="btn-secondary" onClick={onCancel} disabled={loading} id="confirm-cancel-btn">
            Cancelar
          </button>
          <button className="btn-danger" onClick={onConfirm} disabled={loading} id="confirm-delete-btn">
            {loading ? 'Removendo…' : 'Confirmar'}
          </button>
        </div>
      </div>
    </div>
  );
}
