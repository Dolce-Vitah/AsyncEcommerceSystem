import React, { useState } from 'react';
import '../assets/Section.css';
import { useHistory } from '../hooks/useHistory.js';

const API_BASE = 'http://localhost:8080';

export default function Orders() {
    const [tab, setTab] = useState('create');
    const [userId, setUserId] = useState('');
    const [amount, setAmount] = useState('');
    const [orderId, setOrderId] = useState('');
    const [orders, setOrders] = useState([]);
    const [status, setStatus] = useState('');
    const [error, setError] = useState('');
    const [history, addHistory] = useHistory([]);


    const createOrder = async () => {
        setError('');
        try {
            const res = await fetch(`${API_BASE}/orders`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId, amount: Number(amount) })
            });
            if (!res.ok) {
                const msg = await res.text();
                setError(`Ошибка создания заказа: ${msg}`);
                addHistory(`Ошибка создания заказа: ${ msg }`);
                return;
            }
            const data = await res.json();
            setOrderId(data.orderId);
            addHistory(`Создан заказ: ${data.orderId} (user: ${userId}, amount: ${amount})`);
            alert('Создан заказ: ' + data.orderId);
        } catch (err) {
            setError('Ошибка сети при создании заказа');
        }
    };

    const fetchOrders = async () => {
        setError('');
        try {
            const res = await fetch(`${API_BASE}/orders?userId=${userId}`);
            if (!res.ok) {
                const msg = await res.text();
                setError(`Ошибка получения заказов: ${msg}`);
                addHistory(`Ошибка получения заказов: ${msg}`);
                setOrders([]);
                return;
            }
            setOrders(await res.json());
        } catch (err) {
            setError('Ошибка сети при получении заказов');
            setOrders([]);
        }
    };

    const fetchStatus = async () => {
        setError('');
        try {
            const res = await fetch(`${API_BASE}/orders/${orderId}/status`);
            if (!res.ok) {
                const msg = await res.text();
                setError(`Ошибка получения статуса: ${msg}`);
                addHistory(`Ошибка получения статуса: ${msg}`);
                setStatus('');
                return;
            }
            const data = await res.json();
            const status = convertToStatus(data.status);
            setStatus(status);
            addHistory(`Получен статус заказа ${orderId}: ${status}`);
        } catch (err) {
            setError('Ошибка сети при получении статуса');
            setStatus('');
        }
    };

    const convertToStatus = (status) => {
        if (status == 0) {
            return 'Ожидает обработки';
        } else if (status == 1) {
            return 'Оплачено';
        } else {
            return 'Отменено';
        }
    };

    return (
        <div className="page-flex-container">
            <div className="page-main">
                <h2>Заказы</h2>
                <div className="page-tabs">
                    <button className={tab === 'create' ? 'active' : ''} onClick={() => setTab('create')}>Создать заказ</button>
                    <button className={tab === 'list' ? 'active' : ''} onClick={() => setTab('list')}>Список заказов</button>
                    <button className={tab === 'status' ? 'active' : ''} onClick={() => setTab('status')}>Проверить статус</button>
                </div>
                <div className="page-tab-content">
                    {error && <div className="page-error">{error}</div>}
                    {tab === 'create' && (
                        <div className="page-section">
                            <label>ID пользователя:</label>
                            <input
                                className="page-input"
                                value={userId}
                                onChange={e => setUserId(e.target.value)}
                                placeholder="Введите ID пользователя"
                            />
                            <label>Сумма заказа:</label>
                            <input
                                className="page-input"
                                value={amount}
                                onChange={e => setAmount(e.target.value)}
                                placeholder="Введите сумму заказа"
                                type="number"
                                min="0"
                            />
                            <button className="page-btn" onClick={createOrder}>Создать заказ</button>
                            {orderId && (
                                <div className="page-info">
                                    ID созданного заказа: <b>{orderId}</b>
                                </div>
                            )}
                        </div>
                    )}
                    {tab === 'list' && (
                        <div className="page-section">
                            <label>ID пользователя:</label>
                            <input
                                className="page-input"
                                value={userId}
                                onChange={e => setUserId(e.target.value)}
                                placeholder="Введите ID пользователя"
                            />
                            <button className="page-btn" onClick={fetchOrders}>Показать заказы</button>
                            <ul style={{ marginTop: 16, width: '100%', paddingLeft: 0, listStyle: 'none' }}>
                                {orders.map(o => (
                                    <li key={o.id} style={{
                                        background: '#e0e7ff',
                                        borderRadius: '6px',
                                        padding: '8px 12px',
                                        marginBottom: '8px',
                                        color: '#1e40af',
                                        fontWeight: 500
                                    }}>
                                        {o.id} — {o.status}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}
                    {tab === 'status' && (
                        <div className="page-section">
                            <label>ID закза:</label>
                            <input
                                className="page-input"
                                value={orderId}
                                onChange={e => setOrderId(e.target.value)}
                                placeholder="Введите ID заказа"
                            />
                            <button className="page-btn" onClick={fetchStatus}>Получить статус</button>
                            {status && (
                                <div className="page-balance">
                                    Статус: <b>{status}</b>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </div>
            <div className="page-history">
                <h3>История действий</h3>
                {history.length === 0 ? (
                    <div className="page-history-empty">Нет истории действий</div>
                ) : (
                    <ul className="page-history-list">
                        {history.map((item, index) => (
                            <li key={index} className="page-history-item">
                                <span className="page-history-time">{item.timestamp}</span> - {item.message}
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </div>
    );
}