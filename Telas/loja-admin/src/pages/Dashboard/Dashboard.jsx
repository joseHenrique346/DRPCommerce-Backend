// src/pages/Dashboard/Dashboard.jsx
// Página inicial do painel — métricas + tabela resumida de produtos

import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ShoppingBag,
  ShoppingCart,
  Tag,
  Users,
  TrendingUp,
  Search,
  ChevronLeft,
  ChevronRight,
  Pencil,
  Trash2,
  Eye,
  Plus,
  Package,
  AlertCircle,
  RefreshCw,
} from 'lucide-react';

import Topbar from '../../components/Topbar/Topbar.jsx';
import MetricCard from '../../components/MetricCard/MetricCard.jsx';
import ProductForm from '../../components/ProductForm/ProductForm.jsx';
import ConfirmModal from '../../components/ConfirmModal/ConfirmModal.jsx';
import { useProducts } from '../../hooks/useProducts.js';
import { useToast } from '../../hooks/useToast.js';
import ToastContainer from '../../components/Toast/ToastContainer.jsx';

const PAGE_SIZE = 8;

const formatBRL = (value) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value ?? 0);

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
      <td>
        <div style={{ display: 'flex', gap: 6 }}>
          <div className="skeleton" style={{ width: 32, height: 32, borderRadius: 6 }} />
          <div className="skeleton" style={{ width: 32, height: 32, borderRadius: 6 }} />
          <div className="skeleton" style={{ width: 32, height: 32, borderRadius: 6 }} />
        </div>
      </td>
    </tr>
  );
}

export default function Dashboard() {
  const navigate = useNavigate();
  const { products, loading, error, refetch, addProduct, editProduct, removeProduct } = useProducts();
  const { toasts, addToast, removeToast } = useToast();

  // UI state
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [selectAll, setSelectAll] = useState(false);
  const [selected, setSelected] = useState(new Set());
  const [modalProduct, setModalProduct] = useState(undefined); // undefined=fechado, null=criar, object=editar
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState(false);

  // Filtro de busca
  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    return products.filter(
      (p) =>
        p.name?.toLowerCase().includes(q) ||
        p.sku?.toLowerCase().includes(q) ||
        p.brand?.toLowerCase().includes(q)
    );
  }, [products, search]);

  // Paginação
  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const paginated = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  const handleSearch = (val) => {
    setSearch(val);
    setPage(1);
  };

  // Seleção
  const toggleSelect = (id) => {
    setSelected((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  };
  const handleSelectAll = (checked) => {
    setSelectAll(checked);
    setSelected(checked ? new Set(paginated.map((p) => p.id)) : new Set());
  };

  // Métricas calculadas
  const activeProducts = products.filter((p) => p.isActive).length;
  const totalRevenue = products.reduce((acc, p) => acc + (p.price || 0), 0);

  // CRUD handlers
  const handleSave = async (payload) => {
    setSaving(true);
    const isEditing = Boolean(payload.id);
    const result = isEditing
      ? await editProduct(payload)
      : await addProduct(payload);
    setSaving(false);

    if (result.ok) {
      addToast(isEditing ? 'Produto atualizado com sucesso!' : 'Produto cadastrado com sucesso!', 'success');
      setModalProduct(undefined);
    } else {
      addToast(result.error || 'Erro ao salvar produto.', 'error');
    }
  };

  const handleDeleteConfirm = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    const result = await removeProduct(deleteTarget.id);
    setDeleting(false);
    if (result.ok) {
      addToast(`"${deleteTarget.name}" foi removido.`, 'success');
    } else {
      addToast(result.error || 'Erro ao remover produto.', 'error');
    }
    setDeleteTarget(null);
  };

  return (
    <>
      <Topbar pageTitle="Administração da Loja" />

      <main className="page-container">

        {/* Métricas */}
        <section className="metrics-grid" aria-label="Resumo de métricas">
          <MetricCard
            label="Vendas Totais"
            value={formatBRL(74560)}
            subtext={<><TrendingUp size={12} /> +15% Este Mês</>}
            subtextVariant="positive"
            icon={<ShoppingBag size={18} style={{ color: '#4f6ef7' }} />}
            iconBg="rgba(79,110,247,0.15)"
          />
          <MetricCard
            label="Novos Pedidos"
            value="389"
            icon={<ShoppingCart size={18} style={{ color: '#22c55e' }} />}
            iconBg="rgba(34,197,94,0.15)"
          />
          <MetricCard
            label="Produtos Ativos"
            value={loading ? '—' : activeProducts.toLocaleString('pt-BR')}
            subtext={
              !loading && products.length > 0
                ? `${Math.round((activeProducts / products.length) * 100)}% do Catálogo`
                : undefined
            }
            icon={<Tag size={18} style={{ color: '#f59e0b' }} />}
            iconBg="rgba(245,158,11,0.15)"
          />
          <MetricCard
            label="Clientes Recentes"
            value="98"
            icon={<Users size={18} style={{ color: '#ec4899' }} />}
            iconBg="rgba(236,72,153,0.15)"
          />
        </section>

        {/* Catálogo */}
        <section className="catalog-section" aria-label="Catálogo de Produtos">
          {/* Header */}
          <div className="catalog-header">
            <h2 className="catalog-title">Catálogo de Produtos</h2>
            <div className="catalog-controls">
              <button
                className="btn-primary"
                onClick={() => setModalProduct(null)}
                id="open-add-product-btn"
              >
                <Plus size={15} />
                Adicionar Novo Produto
              </button>
            </div>
          </div>

          {/* Toolbar */}
          <div className="table-toolbar">
            <div className="search-input-wrap">
              <Search />
              <input
                id="product-search-input"
                className="search-input"
                type="text"
                placeholder="Buscar por nome, SKU ou marca…"
                value={search}
                onChange={(e) => handleSearch(e.target.value)}
                aria-label="Buscar produtos"
              />
            </div>

            <div className="pagination-controls">
              <span>Página</span>
              <input
                className="page-input"
                type="number"
                min={1}
                max={totalPages}
                value={page}
                onChange={(e) => {
                  const v = Number(e.target.value);
                  if (v >= 1 && v <= totalPages) setPage(v);
                }}
                aria-label="Número da página"
                id="page-input"
              />
              <span>de {totalPages}</span>
              <button
                className="page-btn"
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                aria-label="Página anterior"
                id="page-prev-btn"
              >
                <ChevronLeft />
              </button>
              <button
                className="page-btn"
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                aria-label="Próxima página"
                id="page-next-btn"
              >
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
                <button className="btn-secondary" onClick={refetch} style={{ marginTop: 8 }} id="retry-btn">
                  <RefreshCw size={14} style={{ marginRight: 6 }} />
                  Tentar novamente
                </button>
              </div>
            ) : (
              <table className="product-table" aria-label="Tabela de produtos">
                <thead>
                  <tr>
                    <th>
                      <input
                        type="checkbox"
                        className="checkbox"
                        checked={selectAll}
                        onChange={(e) => handleSelectAll(e.target.checked)}
                        aria-label="Selecionar todos"
                        id="select-all-checkbox"
                      />
                    </th>
                    <th>Foto</th>
                    <th>Nome do Produto</th>
                    <th>SKU</th>
                    <th>Preço</th>
                    <th>Status</th>
                    <th>Ações</th>
                  </tr>
                </thead>
                <tbody>
                  {loading ? (
                    Array.from({ length: PAGE_SIZE }).map((_, i) => <SkeletonRow key={i} />)
                  ) : paginated.length === 0 ? (
                    <tr>
                      <td colSpan={7}>
                        <div className="empty-state">
                          <Package />
                          <h3>Nenhum produto encontrado</h3>
                          <p>
                            {search
                              ? `Nenhum resultado para "${search}".`
                              : 'Adicione seu primeiro produto usando o botão acima.'}
                          </p>
                        </div>
                      </td>
                    </tr>
                  ) : (
                    paginated.map((product) => (
                      <tr key={product.id}>
                        <td>
                          <input
                            type="checkbox"
                            className="checkbox"
                            checked={selected.has(product.id)}
                            onChange={() => toggleSelect(product.id)}
                            aria-label={`Selecionar ${product.name}`}
                          />
                        </td>
                        <td>
                          <div className="product-thumb">
                            {product.imageUrls ? (
                              <img
                                src={product.imageUrls.split(';')[0]}
                                alt={product.name}
                                onError={(e) => { e.target.style.display = 'none'; }}
                              />
                            ) : (
                              <Package />
                            )}
                          </div>
                        </td>
                        <td>
                          <div className="product-name">{product.name}</div>
                          {product.brand && (
                            <div className="product-sku">{product.brand}</div>
                          )}
                        </td>
                        <td>
                          <span style={{ fontFamily: 'monospace', fontSize: 12, color: 'var(--text-secondary)' }}>
                            {product.sku}
                          </span>
                        </td>
                        <td>
                          <span className="product-price">{formatBRL(product.price)}</span>
                        </td>
                        <td>
                          <span className={`status-badge ${product.isActive ? 'active' : 'inactive'}`}>
                            <span className="status-dot" />
                            {product.isActive ? 'Ativo' : 'Inativo'}
                          </span>
                        </td>
                        <td>
                          <div className="actions-cell">
                            <button
                              className="btn-icon"
                              onClick={() => setModalProduct(product)}
                              title="Editar produto"
                              aria-label={`Editar ${product.name}`}
                              id={`edit-product-${product.id}`}
                            >
                              <Pencil />
                            </button>
                            <button
                              className="btn-icon"
                              onClick={() => setDeleteTarget(product)}
                              title="Remover produto"
                              aria-label={`Remover ${product.name}`}
                              id={`delete-product-${product.id}`}
                              style={{ '--hover-bg': 'var(--danger-muted)', '--hover-color': 'var(--danger)' }}
                              onMouseEnter={(e) => {
                                e.currentTarget.style.background = 'var(--danger-muted)';
                                e.currentTarget.style.borderColor = 'var(--danger)';
                                e.currentTarget.style.color = 'var(--danger)';
                              }}
                              onMouseLeave={(e) => {
                                e.currentTarget.style.background = '';
                                e.currentTarget.style.borderColor = '';
                                e.currentTarget.style.color = '';
                              }}
                            >
                              <Trash2 />
                            </button>
                            <button
                              className="btn-icon"
                              onClick={() => navigate(`/products/${product.id}`)}
                              title="Visualizar produto"
                              aria-label={`Visualizar ${product.name}`}
                              id={`view-product-${product.id}`}
                            >
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

          {/* Table Footer Pagination */}
          {!loading && !error && filtered.length > 0 && (
            <div className="table-footer">
              <div className="pagination-controls">
                <span>Página</span>
                <input
                  className="page-input"
                  type="number"
                  min={1}
                  max={totalPages}
                  value={page}
                  onChange={(e) => {
                    const v = Number(e.target.value);
                    if (v >= 1 && v <= totalPages) setPage(v);
                  }}
                  aria-label="Número da página (rodapé)"
                />
                <span>de {totalPages}</span>
                <button
                  className="page-btn"
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                  disabled={page === 1}
                  aria-label="Página anterior (rodapé)"
                >
                  <ChevronLeft />
                </button>
                <button
                  className="page-btn"
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                  disabled={page === totalPages}
                  aria-label="Próxima página (rodapé)"
                >
                  <ChevronRight />
                </button>
              </div>
            </div>
          )}
        </section>

        {/* Footer */}
        <footer className="app-footer" style={{ marginTop: 24 }}>
          React.js Framework &nbsp;|&nbsp; API Integration: /api/Product/GetAll
        </footer>
      </main>

      {/* Product Form Modal */}
      {modalProduct !== undefined && (
        <ProductForm
          product={modalProduct}
          onSave={handleSave}
          onClose={() => setModalProduct(undefined)}
          saving={saving}
        />
      )}

      {/* Confirm Delete Modal */}
      {deleteTarget && (
        <ConfirmModal
          title="Remover Produto"
          description={`Tem certeza que deseja remover "${deleteTarget.name}"? Esta ação não pode ser desfeita.`}
          onConfirm={handleDeleteConfirm}
          onCancel={() => setDeleteTarget(null)}
          loading={deleting}
        />
      )}

      {/* Toast Notifications */}
      <ToastContainer toasts={toasts} removeToast={removeToast} />
    </>
  );
}
