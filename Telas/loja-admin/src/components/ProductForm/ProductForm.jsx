// src/components/ProductForm/ProductForm.jsx
// Modal de cadastro e edição de produto
// Campos baseados no schema Product.cs do StoreCommerce.Domain

import { useState, useEffect } from 'react';
import { X, Package } from 'lucide-react';

const EMPTY_FORM = {
  name: '',
  slug: '',
  sku: '',
  barCode: '',
  description: '',
  price: '',
  costPrice: '',
  brand: '',
  imageUrls: '',
  weight: '',
  height: '',
  width: '',
  length: '',
  categoryId: '',
  supplierId: '',
  enterpriseId: '',
  isActive: true,
  isDigital: false,
};

function slugify(text) {
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[^a-z0-9\s-]/g, '')
    .trim()
    .replace(/\s+/g, '-');
}

export default function ProductForm({ product, onSave, onClose, saving }) {
  const isEditing = Boolean(product?.id);
  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (product) {
      setForm({
        ...EMPTY_FORM,
        name: product.name ?? '',
        slug: product.slug ?? '',
        sku: product.sku ?? '',
        barCode: product.barCode ?? '',
        description: product.description ?? '',
        price: product.price ?? '',
        costPrice: product.costPrice ?? '',
        brand: product.brand ?? '',
        imageUrls: product.imageUrls ?? '',
        weight: product.weight ?? '',
        height: product.height ?? '',
        width: product.width ?? '',
        length: product.length ?? '',
        categoryId: product.categoryId ?? '',
        supplierId: product.supplierId ?? '',
        enterpriseId: product.enterpriseId ?? '',
        isActive: product.isActive ?? true,
        isDigital: product.isDigital ?? false,
      });
    }
  }, [product]);

  const set = (field, value) => {
    setForm((prev) => {
      const updated = { ...prev, [field]: value };
      // Auto-slug ao digitar nome (somente criação)
      if (field === 'name' && !isEditing) {
        updated.slug = slugify(value);
      }
      return updated;
    });
    // Limpa erro do campo ao editar
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validate = () => {
    const e = {};
    if (!form.name.trim()) e.name = 'Nome é obrigatório.';
    if (!form.sku.trim()) e.sku = 'SKU é obrigatório.';
    if (!form.price || isNaN(Number(form.price)) || Number(form.price) < 0)
      e.price = 'Preço inválido.';
    if (!form.categoryId) e.categoryId = 'Categoria é obrigatória.';
    if (!form.enterpriseId) e.enterpriseId = 'Enterprise ID é obrigatório.';
    return e;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }

    const payload = {
      ...(isEditing ? { id: product.id } : {}),
      name: form.name.trim(),
      slug: form.slug.trim() || slugify(form.name),
      sku: form.sku.trim(),
      barCode: form.barCode.trim(),
      description: form.description.trim(),
      price: Number(form.price),
      costPrice: Number(form.costPrice) || 0,
      brand: form.brand.trim(),
      imageUrls: form.imageUrls.trim(),
      weight: Number(form.weight) || 0,
      height: Number(form.height) || 0,
      width: Number(form.width) || 0,
      length: Number(form.length) || 0,
      categoryId: Number(form.categoryId),
      supplierId: form.supplierId ? Number(form.supplierId) : null,
      enterpriseId: Number(form.enterpriseId),
      isActive: form.isActive,
      isDigital: form.isDigital,
    };

    onSave(payload);
  };

  return (
    <div className="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="product-form-title">
      <div className="modal">
        {/* Header */}
        <div className="modal-header">
          <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <Package size={18} style={{ color: 'var(--accent)' }} />
            <h2 id="product-form-title">
              {isEditing ? 'Editar Produto' : 'Cadastrar Produto'}
            </h2>
          </div>
          <button
            className="modal-close"
            onClick={onClose}
            aria-label="Fechar modal"
            id="product-form-close-btn"
            type="button"
          >
            <X />
          </button>
        </div>

        {/* Body */}
        <form id="product-form" onSubmit={handleSubmit}>
          <div className="modal-body">
            <div className="form-grid">

              {/* SEÇÃO: Identificação */}
              <div className="form-section-title">Identificação</div>

              <div className="form-group span-2">
                <label className="form-label required" htmlFor="pf-name">Nome do Produto</label>
                <input
                  id="pf-name"
                  className={`form-input${errors.name ? ' error' : ''}`}
                  type="text"
                  placeholder="Ex: Tênis Esportivo Nike Zoom"
                  value={form.name}
                  onChange={(e) => set('name', e.target.value)}
                />
                {errors.name && <span className="form-error">{errors.name}</span>}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-slug">Slug (URL)</label>
                <input
                  id="pf-slug"
                  className="form-input"
                  type="text"
                  placeholder="tenis-esportivo-nike-zoom"
                  value={form.slug}
                  onChange={(e) => set('slug', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label required" htmlFor="pf-sku">SKU</label>
                <input
                  id="pf-sku"
                  className={`form-input${errors.sku ? ' error' : ''}`}
                  type="text"
                  placeholder="Ex: SKU-NKA001"
                  value={form.sku}
                  onChange={(e) => set('sku', e.target.value)}
                />
                {errors.sku && <span className="form-error">{errors.sku}</span>}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-barcode">Código de Barras</label>
                <input
                  id="pf-barcode"
                  className="form-input"
                  type="text"
                  placeholder="EAN-13 / UPC"
                  value={form.barCode}
                  onChange={(e) => set('barCode', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-brand">Marca</label>
                <input
                  id="pf-brand"
                  className="form-input"
                  type="text"
                  placeholder="Ex: Nike"
                  value={form.brand}
                  onChange={(e) => set('brand', e.target.value)}
                />
              </div>

              <div className="form-group span-2">
                <label className="form-label" htmlFor="pf-description">Descrição</label>
                <textarea
                  id="pf-description"
                  className="form-textarea"
                  placeholder="Descreva o produto em detalhes…"
                  value={form.description}
                  onChange={(e) => set('description', e.target.value)}
                  rows={3}
                />
              </div>

              {/* SEÇÃO: Preço & Categoria */}
              <div className="form-section-title">Preço &amp; Classificação</div>

              <div className="form-group">
                <label className="form-label required" htmlFor="pf-price">Preço de Venda (R$)</label>
                <input
                  id="pf-price"
                  className={`form-input${errors.price ? ' error' : ''}`}
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0,00"
                  value={form.price}
                  onChange={(e) => set('price', e.target.value)}
                />
                {errors.price && <span className="form-error">{errors.price}</span>}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-costprice">Preço de Custo (R$)</label>
                <input
                  id="pf-costprice"
                  className="form-input"
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0,00"
                  value={form.costPrice}
                  onChange={(e) => set('costPrice', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label required" htmlFor="pf-enterprise">Enterprise ID</label>
                <input
                  id="pf-enterprise"
                  className={`form-input${errors.enterpriseId ? ' error' : ''}`}
                  type="number"
                  min="1"
                  placeholder="ID da empresa (tenant)"
                  value={form.enterpriseId}
                  onChange={(e) => set('enterpriseId', e.target.value)}
                />
                {errors.enterpriseId && <span className="form-error">{errors.enterpriseId}</span>}
              </div>

              <div className="form-group">
                <label className="form-label required" htmlFor="pf-category">Category ID</label>
                <input
                  id="pf-category"
                  className={`form-input${errors.categoryId ? ' error' : ''}`}
                  type="number"
                  min="1"
                  placeholder="ID da categoria"
                  value={form.categoryId}
                  onChange={(e) => set('categoryId', e.target.value)}
                />
                {errors.categoryId && <span className="form-error">{errors.categoryId}</span>}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-supplier">Supplier ID</label>
                <input
                  id="pf-supplier"
                  className="form-input"
                  type="number"
                  min="1"
                  placeholder="ID do fornecedor (opcional)"
                  value={form.supplierId}
                  onChange={(e) => set('supplierId', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-images">URLs de Imagens</label>
                <input
                  id="pf-images"
                  className="form-input"
                  type="text"
                  placeholder="URL1;URL2;URL3 (separadas por ;)"
                  value={form.imageUrls}
                  onChange={(e) => set('imageUrls', e.target.value)}
                />
              </div>

              {/* SEÇÃO: Dimensões */}
              <div className="form-section-title">Dimensões &amp; Logística</div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-weight">Peso (kg)</label>
                <input
                  id="pf-weight"
                  className="form-input"
                  type="number"
                  step="0.001"
                  min="0"
                  placeholder="0.000"
                  value={form.weight}
                  onChange={(e) => set('weight', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-height">Altura (cm)</label>
                <input
                  id="pf-height"
                  className="form-input"
                  type="number"
                  step="0.1"
                  min="0"
                  placeholder="0.0"
                  value={form.height}
                  onChange={(e) => set('height', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-width">Largura (cm)</label>
                <input
                  id="pf-width"
                  className="form-input"
                  type="number"
                  step="0.1"
                  min="0"
                  placeholder="0.0"
                  value={form.width}
                  onChange={(e) => set('width', e.target.value)}
                />
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="pf-length">Comprimento (cm)</label>
                <input
                  id="pf-length"
                  className="form-input"
                  type="number"
                  step="0.1"
                  min="0"
                  placeholder="0.0"
                  value={form.length}
                  onChange={(e) => set('length', e.target.value)}
                />
              </div>

              {/* SEÇÃO: Status */}
              <div className="form-section-title">Status</div>

              <div className="form-group">
                <label className="form-label">Produto Ativo</label>
                <div className="toggle-group">
                  <label className="toggle" htmlFor="pf-active">
                    <input
                      id="pf-active"
                      type="checkbox"
                      checked={form.isActive}
                      onChange={(e) => set('isActive', e.target.checked)}
                    />
                    <span className="toggle-slider" />
                  </label>
                  <span className="toggle-label">
                    {form.isActive ? 'Ativo' : 'Inativo'}
                  </span>
                </div>
              </div>

              <div className="form-group">
                <label className="form-label">Produto Digital</label>
                <div className="toggle-group">
                  <label className="toggle" htmlFor="pf-digital">
                    <input
                      id="pf-digital"
                      type="checkbox"
                      checked={form.isDigital}
                      onChange={(e) => set('isDigital', e.target.checked)}
                    />
                    <span className="toggle-slider" />
                  </label>
                  <span className="toggle-label">
                    {form.isDigital ? 'Digital' : 'Físico'}
                  </span>
                </div>
              </div>

            </div>
          </div>

          {/* Footer */}
          <div className="modal-footer">
            <button
              type="button"
              className="btn-secondary"
              onClick={onClose}
              disabled={saving}
              id="product-form-cancel-btn"
            >
              Cancelar
            </button>
            <button
              type="submit"
              className="btn-primary"
              disabled={saving}
              id="product-form-submit-btn"
            >
              {saving
                ? (isEditing ? 'Salvando…' : 'Cadastrando…')
                : (isEditing ? 'Salvar Alterações' : '+ Cadastrar Produto')}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
