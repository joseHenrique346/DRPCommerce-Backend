// src/hooks/useProducts.js

import { useState, useEffect, useCallback } from 'react';
import {
  getAllProducts,
  createProduct,
  updateProduct,
  deleteProduct,
  deleteProductRange,
} from '../services/productService';

export function useProducts() {
  const [products, setProducts] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const fetchProducts = useCallback(async (signal) => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await getAllProducts(signal);

      if (result.isSuccess) {
        setProducts(result.value ?? []);
      } else {
        setError(result.errors?.join(', ') || 'Erro ao carregar produtos.');
      }
    } catch (err) {
      if (err.name === 'AbortError' || err.code === 'ERR_CANCELED') return;

      const message =
        err.response?.data?.errors?.join(', ') ||
        err.message ||
        'Não foi possível conectar à API.';

      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    fetchProducts(controller.signal);

    return () => controller.abort();
  }, [fetchProducts]);

  const refetch = useCallback(() => {
    const controller = new AbortController();
    fetchProducts(controller.signal);
    return () => controller.abort();
  }, [fetchProducts]);

  const addProduct = useCallback(async (payload) => {
    const result = await createProduct(payload);
    if (result.isSuccess) {
      await fetchProducts();
      return { ok: true };
    }
    return { ok: false, error: result.errors?.join(', ') || 'Erro ao criar produto.' };
  }, [fetchProducts]);

  const editProduct = useCallback(async (payload) => {
    const result = await updateProduct(payload);
    if (result.isSuccess) {
      await fetchProducts();
      return { ok: true };
    }
    return { ok: false, error: result.errors?.join(', ') || 'Erro ao atualizar produto.' };
  }, [fetchProducts]);

  const removeProduct = useCallback(async (id) => {
    const result = await deleteProduct(id);
    if (result.isSuccess) {
      setProducts((prev) => prev.filter((p) => p.id !== id));
      return { ok: true };
    }
    return { ok: false, error: result.errors?.join(', ') || 'Erro ao remover produto.' };
  }, []);

  const removeProductRange = useCallback(async (ids) => {
    const result = await deleteProductRange(ids);
    if (result.isSuccess) {
      setProducts((prev) => prev.filter((p) => !ids.includes(p.id)));
      return { ok: true };
    }
    return { ok: false, error: result.errors?.join(', ') || 'Erro ao remover produtos.' };
  }, []);

  return {
    products,
    loading: isLoading,
    error,
    refetch,
    addProduct,
    editProduct,
    removeProduct,
    removeProductRange,
  };
}
