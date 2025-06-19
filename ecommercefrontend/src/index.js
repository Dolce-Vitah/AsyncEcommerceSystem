import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import Accounts from './pages/Account.js';
import Orders from './pages/Order.js';
import Home from './pages/Home.js';
import './assets/App.css';

const App = () => (
    <BrowserRouter>
        <nav className="main-navbar">
            <div className="navbar-content">
                <Link className="navbar-brand" to="/">Ecommerce</Link>
                <div className="navbar-links">
                    <Link className="nav-link" to="/accounts">Аккаунты</Link>
                    <Link className="nav-link" to="/orders">Заказы</Link>
                </div>
            </div>
        </nav>
        <div className="main-container">
            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/accounts/*" element={<Accounts />} />
                <Route path="/orders/*" element={<Orders />} />
                <Route path="*" element={<NotFound />} />
            </Routes>
        </div>
    </BrowserRouter>
);

const NotFound = () => (
    <div className="notfound">
        <h2>404 - Page Not Found</h2>
        <p>Выберите раздел из навигации.</p>
    </div>
);

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(<App />);
