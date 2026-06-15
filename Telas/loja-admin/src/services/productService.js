// src/services/productService.js
// Serviço de API para o recurso Product
// Endpoints base: /api/Product/
// Padrão BaseController: Add, AddRange, Update, UpdateRange,
//                        Delete/{id}, DeleteRange, GetAll, GetById/{id}, GetListByListId

import axios from 'axios';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: `${BASE_URL}/api/Product`,
  headers: { 'Content-Type': 'application/json' },
});

// Interceptor para anexar JWT automaticamente
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

/**
 * Retorna lista completa de produtos (com filtro de tenant via JWT).
 * Endpoint: GET /api/Product/GetAll
 * @returns {Promise<{ isSuccess: boolean, value: Product[], errors: string[] }>}
 */
export const getAllProducts = (signal) =>
  api.get('/GetAll', { signal }).then((r) => r.data);

/**
 * Retorna um produto pelo ID.
 * Endpoint: GET /api/Product/GetById/{id}
 */
export const getProductById = (id) => api.get(`/GetById/${id}`).then((r) => r.data);

/**
 * Retorna lista de produtos por IDs.
 * Endpoint: POST /api/Product/GetListByListId
 */
export const getProductsByIds = (ids) =>
  api.post('/GetListByListId', ids).then((r) => r.data);

/**
 * Cria um produto (wrap automático em AddRange).
 * Endpoint: POST /api/Product/Add
 * @param {CreateProductPayload} payload
 */
export const createProduct = (payload) => api.post('/Add', payload).then((r) => r.data);

/**
 * Atualiza um produto.
 * Endpoint: PUT /api/Product/Update
 * @param {UpdateProductPayload} payload
 */
export const updateProduct = (payload) => api.put('/Update', payload).then((r) => r.data);

/**
 * Remove (soft delete) um produto.
 * Endpoint: DELETE /api/Product/Delete/{id}
 */
export const deleteProduct = (id) => api.delete(`/Delete/${id}`).then((r) => r.data);

/**
 * Remove múltiplos produtos.
 * Endpoint: DELETE /api/Product/DeleteRange
 */
export const deleteProductRange = (ids) =>
  api.delete('/DeleteRange', { data: ids }).then((r) => r.data);
