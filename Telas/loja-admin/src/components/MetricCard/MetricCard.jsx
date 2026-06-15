// src/components/MetricCard/MetricCard.jsx

export default function MetricCard({ label, value, subtext, subtextVariant = 'neutral', icon, iconBg }) {
  return (
    <div className="metric-card" role="region" aria-label={label}>
      <div className="metric-card-header">
        <span className="metric-label">{label}</span>
        {icon && (
          <div
            className="metric-icon"
            style={{ background: iconBg || 'var(--accent-muted)' }}
            aria-hidden="true"
          >
            {icon}
          </div>
        )}
      </div>

      <div className="metric-value">{value}</div>

      {subtext && (
        <div className={`metric-subtext ${subtextVariant}`}>
          {subtext}
        </div>
      )}
    </div>
  );
}
