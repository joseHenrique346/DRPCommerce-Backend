// src/App.jsx
// Roteamento principal da aplicação

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ShoppingCart, Users, Settings } from 'lucide-react';

import Sidebar from './components/Sidebar/Sidebar.jsx';
import Dashboard from './pages/Dashboard/Dashboard.jsx';
import Products from './pages/Products/Products.jsx';
import ComingSoon from './pages/ComingSoon/ComingSoon.jsx';

function AppLayout({ children }) {
  return (
    <div className="app-layout">
      <Sidebar />
      <div className="main-content">
        {children}
      </div>
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AppLayout>
        <Routes>
          {/* Dashboard — visão geral com métricas e produtos */}
          <Route path="/" element={<Dashboard />} />

          {/* Produtos — gerenciamento completo */}
          <Route path="/products" element={<Products />} />
          <Route path="/products/:id" element={<Products />} />

          {/* Pedidos — disponível em breve */}
          <Route
            path="/orders"
            element={
              <ComingSoon
                pageTitle="Pedidos"
                pageIcon={ShoppingCart}
                description="A gestão de pedidos e transações estará disponível em breve. Acompanhe o status de cada pedido, pagamento e entrega em tempo real."
              />
            }
          />

          {/* Clientes — disponível em breve */}
          <Route
            path="/customers"
            element={
              <ComingSoon
                pageTitle="Clientes"
                pageIcon={Users}
                description="O módulo de clientes está em desenvolvimento. Em breve você poderá visualizar e gerenciar toda a base de clientes do seu e-commerce."
              />
            }
          />

          {/* Configurações — disponível em breve */}
          <Route
            path="/settings"
            element={
              <ComingSoon
                pageTitle="Configurações"
                pageIcon={Settings}
                description="As configurações da loja estarão disponíveis em breve. Configure integrações, preferências e permissões de acesso."
              />
            }
          />

          {/* Fallback — 404 */}
          <Route
            path="*"
            element={
              <ComingSoon
                pageTitle="Página não encontrada"
                description="A página que você está procurando não existe ou foi movida."
              />
            }
          />
        </Routes>
      </AppLayout>
    </BrowserRouter>
  );
}
