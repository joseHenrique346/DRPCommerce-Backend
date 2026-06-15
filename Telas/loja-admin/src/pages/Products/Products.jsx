// src/pages/Products/Products.jsx
// Página dedicada de Produtos — lista completa + gerenciamento

import { useState, useMemo } from 'react';
import {
  Search, ChevronLeft, ChevronRight, Pencil, Trash2, Eye,
  Plus, Package, AlertCircle, RefreshCw, Filter, Download,
} from 'lucide-react';

import Topbar from '../../components/Topbar/Topbar.jsx';
import ProductForm from '../../components/ProductForm/ProductForm.jsx';
import ConfirmModal from '../../components/ConfirmModal/ConfirmModal.jsx';
import ToastContainer from '../../components/Toast/ToastContainer.jsx';
import { useProducts } from '../../hooks/useProducts.js';
import { useToast } from '../../hooks/useToast.js';

const PAGE_SIZE = 10;
const formatBRL = (v) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v ?? 0);

function SkeletonRow() {
  return (
    <tr className="skeleton-row">
      <td><div className="skeleton skeleton-cell" style={{ width: 16 }} /></td>
      <td><div className="skeleton skeleton-thumb" /></td>
      <td>
        <div className="skeleton skeleton-cell" style={{ width: '70%', marginBottom: 6 }} />
        <div className="skeleton skeleton-cell" style={{ width: '40%', height: 11 }} />
      </td>
      <td><div className="skeleton skeleton-cell" style={{ width: 80 }} /></td>
      <td><div className="skeleton skeleton-cell" style={{ width: 70 }} /></td>
      <td><div className="skeleton skeleton-cell" style={{ width: 60, borderRadius: 20 }} /></td>
      <td><div className="skeleton skeleton-cell" style={{ width: 80 }} /></td>
      <td>
        <div style={{ display: 'flex', gap: 6 }}>
          {[1,2,3].map(i => (
            <div key={i} className="skeleton" style={{ width: 32, height: 32, borderRadius: 6 }} />
          ))}
        </div>
      </td>
    </tr>
  );
}

export default function Products() {
  const { products, loading, error, refetch, addProduct, editProduct, removeProduct } = useProducts();
  const { toasts, addToast, removeToast } = useToast();

  const [search, setSearch]             = useState('');
  const [filterStatus, setFilterStatus] = useState('all'); // all | active | inactive
  const [page, setPage]                 = useState(1);
  const [selected, setSelected]         = useState(new Set());
  const [modalProduct, setModalProduct] = useState(undefined);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [saving, setSaving]             = useState(false);
  const [deleting, setDeleting]         = useState(false);

  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    return products.filter((p) => {
      const matchSearch =
        p.name?.toLowerCase().includes(q) ||
        p.sku?.toLowerCase().includes(q)   ||
        p.brand?.toLowerCase().includes(q);
      const matchStatus =
        filterStatus === 'all'      ? true :
        filterStatus === 'active'   ? p.isActive :
        !p.isActive;
      return matchSearch && matchStatus;
    });
  }, [products, search, filterStatus]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated  = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const handleSearch = (v) => { setSearch(v); setPage(1); };
  const handleFilter = (v) => { setFilterStatus(v); setPage(1); };

  const toggleSelect = (id) => {
    setSelected((prev) => {
      const n = new Set(prev);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });
  };

  const handleSave = async (payload) => {
    setSaving(true);
    const isEditing = Boolean(payload.id);
    const result = isEditing ? await editProduct(payload) : await addProduct(payload);
    setSaving(false);
    if (result.ok) {
      addToast(isEditing ? 'Produto atualizado!' : 'Produto cadastrado!', 'success');
      setModalProduct(undefined);
    } else {
      addToast(result.error || 'Erro ao salvar.', 'error');
    }
  };

  const handleDeleteConfirm = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    const result = await removeProduct(deleteTarget.id);
    setDeleting(false);
    if (result.ok) {
      addToast(`"${deleteTarget.name}" removido.`, 'success');
    } else {
      addToast(result.error || 'Erro ao remover.', 'error');
    }
    setDeleteTarget(null);
  };

  const activeCount   = products.filter((p) => p.isActive).length;
  const inactiveCount = products.length - activeCount;

  return (
    <>
      <Topbar pageTitle="Produtos" />

      <main className="page-container">

        {/* Stats bar */}
        <div style={{ display: 'flex', gap: 16, marginBottom: 24, flexWrap: 'wrap' }}>
          {[
            { label: 'Total', value: products.length,  color: 'var(--accent)' },
            { label: 'Ativos', value: activeCount,     color: 'var(--success)' },
            { label: 'Inativos', value: inactiveCount, color: 'var(--text-muted)' },
          ].map(({ label, value, color }) => (
            <div
              key={label}
              style={{
                background: 'var(--bg-card)',
                border: '1px solid var(--border)',
                borderRadius: 'var(--radius-md)',
                padding: '12px 20px',
                display: 'flex',
                flexDirection: 'column',
                gap: 2,
                minWidth: 100,
              }}
            >
              <span style={{ fontSize: 11, color: 'var(--text-muted)', fontWeight: 600, textTransform: 'uppercase', letterSpacing: '0.07em' }}>
                {label}
              </span>
              <span style={{ fontSize: 24, fontWeight: 800, color, lineHeight: 1.1 }}>
                {loading ? '—' : value}
              </span>
            </div>
          ))}
        </div>

        {/* Main table section */}
        <section className="catalog-section" aria-label="Lista de Produtos">
          <div className="catalog-header">
            <h2 className="catalog-title">Gerenciar Produtos</h2>
            <div className="catalog-controls">
              <button className="btn-secondary" title="Exportar" id="export-products-btn">
                <Download size={14} />
                Exportar
              </button>
              <button
                className="btn-primary"
                onClick={() => setModalProduct(null)}
                id="products-add-btn"
              >
                <Plus size={15} />
                Novo Produto
              </button>
            </div>
          </div>

          {/* Toolbar */}
          <div className="table-toolbar">
            <div style={{ display: 'flex', gap: 10, flex: 1, alignItems: 'center', flexWrap: 'wrap' }}>
              <div className="search-input-wrap" style={{ maxWidth: 280 }}>
                <Search />
                <input
                  id="products-search-input"
                  className="search-input"
                  type="text"
                  placeholder="Buscar nome, SKU, marca…"
                  value={search}
                  onChange={(e) => handleSearch(e.target.value)}
                  aria-label="Buscar produtos"
                />
              </div>

              {/* Filter pills */}
              <div style={{ display: 'flex', gap: 6, alignItems: 'center' }}>
                <Filter size={13} style={{ color: 'var(--text-muted)' }} />
                {[
                  { val: 'all',      label: 'Todos'    },
                  { val: 'active',   label: 'Ativos'   },
                  { val: 'inactive', label: 'Inativos' },
                ].map(({ val, label }) => (
                  <button
                    key={val}
                    onClick={() => handleFilter(val)}
                    id={`filter-${val}-btn`}
                    style={{
                      padding: '5px 12px',
                      border: '1px solid',
                      borderRadius: 20,
                      fontSize: 12,
                      fontWeight: 600,
                      cursor: 'pointer',
                      transition: 'var(--transition)',
                      borderColor: filterStatus === val ? 'var(--accent)' : 'var(--border)',
                      background: filterStatus === val ? 'var(--accent-muted)' : 'transparent',
                      color: filterStatus === val ? 'var(--accent)' : 'var(--text-secondary)',
                    }}
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>

            <div className="pagination-controls">
              <span>Página</span>
              <input className="page-input" type="number" min={1} max={totalPages} value={page}
                onChange={(e) => { const v = Number(e.target.value); if (v >= 1 && v <= totalPages) setPage(v); }}
              />
              <span>de {totalPages}</span>
              <button className="page-btn" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} id="products-prev-btn" aria-label="Anterior">
                <ChevronLeft />
              </button>
              <button className="page-btn" onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages} id="products-next-btn" aria-label="Próxima">
                <ChevronRight />
              </button>
            </div>
          </div>

          {/* Table */}
          <div className="product-table-wrapper">
            {error ? (
              <div className="error-state" role="alert">
                <AlertCircle />
                <h3>Erro ao carregar produtos</h3>
                <p>{error}</p>
                <button className="btn-secondary" onClick={refetch} style={{ marginTop: 8 }} id="products-retry-btn">
                  <RefreshCw size={14} style={{ marginRight: 6 }} /> Tentar novamente
                </button>
              </div>
            ) : (
              <table className="product-table" aria-label="Tabela de produtos">
                <thead>
                  <tr>
                    <th>
                      <input type="checkbox" className="checkbox" id="products-select-all"
                        onChange={(e) => setSelected(e.target.checked ? new Set(paginated.map(p => p.id)) : new Set())}
                        checked={paginated.length > 0 && paginated.every(p => selected.has(p.id))}
                        aria-label="Selecionar todos"
                      />
                    </th>
                    <th>Foto</th>
                    <th>Nome do Produto</th>
                    <th>SKU</th>
                    <th>Preço</th>
                    <th>Custo</th>
                    <th>Status</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    Array.from({ length: PAGE_SIZE }).map((_, i) => <SkeletonRow key={i} />)
                  ) : paginated.length === 0 ? (
                    <tr>
                      <td colSpan={8}>
                        <div className="empty-state">
                          <Package />
                          <h3>Nenhum produto encontrado</h3>
                          <p>{search ? `Sem resultados para "${search}".` : 'Comece cadastrando seu primeiro produto.'}</p>
                          <button className="btn-primary" onClick={() => setModalProduct(null)} style={{ marginTop: 8 }} id="empty-add-product-btn">
                            <Plus size={14} /> Adicionar Produto
                          </button>
                        </div>
                      </td>
                    </tr>
                  ) : (
                    paginated.map((p) => (
                      <tr key={p.id}>
                        <td>
                          <input type="checkbox" className="checkbox"
                            checked={selected.has(p.id)} onChange={() => toggleSelect(p.id)}
                            aria-label={`Selecionar ${p.name}`}
                          />
                        </td>
                        <td>
                          <div className="product-thumb">
                            {p.imageUrls ? (
                              <img src={p.imageUrls.split(';')[0]} alt={p.name}
                                onError={e => { e.target.style.display = 'none'; }} />
                            ) : <Package />}
                          </div>
                        </td>
                        <td>
                          <div className="product-name">{p.name}</div>
                          {p.brand && <div className="product-sku">{p.brand}</div>}
                        </td>
                        <td style={{ fontFamily: 'monospace', fontSize: 12, color: 'var(--text-secondary)' }}>
                          {p.sku}
                        </td>
                        <td><span className="product-price">{formatBRL(p.price)}</span></td>
                        <td style={{ color: 'var(--text-secondary)', fontSize: 13 }}>{formatBRL(p.costPrice)}</td>
                        <td>
                          <span className={`status-badge ${p.isActive ? 'active' : 'inactive'}`}>
                            <span className="status-dot" />
                            {p.isActive ? 'Ativo' : 'Inativo'}
                          </span>
                        </td>
                        <td>
                          <div className="actions-cell">
                            <button className="btn-icon" onClick={() => setModalProduct(p)}
                              title="Editar" aria-label={`Editar ${p.name}`} id={`products-edit-${p.id}`}>
                              <Pencil />
                            </button>
                            <button className="btn-icon" onClick={() => setDeleteTarget(p)}
                              title="Remover" aria-label={`Remover ${p.name}`} id={`products-delete-${p.id}`}
                              onMouseEnter={e => { Object.assign(e.currentTarget.style, { background: 'var(--danger-muted)', borderColor: 'var(--danger)', color: 'var(--danger)' }); }}
                              onMouseLeave={e => { Object.assign(e.currentTarget.style, { background: '', borderColor: '', color: '' }); }}>
                              <Trash2 />
                            </button>
                            <button className="btn-icon" title="Visualizar" aria-label={`Visualizar ${p.name}`} id={`products-view-${p.id}`}>
                              <Eye />
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            )}
          </div>

          {!loading && !error && filtered.length > 0 && (
            <div className="table-footer">
              <div className="pagination-controls">
                <span style={{ marginRight: 8, color: 'var(--text-muted)' }}>
                  {filtered.length} produto{filtered.length !== 1 ? 's' : ''}
                </span>
                <span>Página</span>
                <input className="page-input" type="number" min={1} max={totalPages} value={page}
                  onChange={(e) => { const v = Number(e.target.value); if (v >= 1 && v <= totalPages) setPage(v); }}
                />
                <span>de {totalPages}</span>
                <button className="page-btn" onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} aria-label="Anterior">
                  <ChevronLeft />
                </button>
                <button className="page-btn" onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages} aria-label="Próxima">
                  <ChevronRight />
                </button>
              </div>
            </div>
          )}
        </section>

        <footer className="app-footer" style={{ marginTop: 24 }}>
          React.js Framework &nbsp;|&nbsp; API Integration: /api/Product/GetAll
        </footer>
      </main>

      {modalProduct !== undefined && (
        <ProductForm product={modalProduct} onSave={handleSave} onClose={() => setModalProduct(undefined)} saving={saving} />
      )}

      {deleteTarget && (
        <ConfirmModal
          title="Remover Produto"
          description={`Tem certeza que deseja remover "${deleteTarget.name}"?`}
          onConfirm={handleDeleteConfirm}
          onCancel={() => setDeleteTarget(null)}
          loading={deleting}
        />
      )}

      <ToastContainer toasts={toasts} removeToast={removeToast} />
    </>
  );
}
