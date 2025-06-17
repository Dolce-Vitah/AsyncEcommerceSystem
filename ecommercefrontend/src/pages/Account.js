import React, { useState } from 'react';
import '../assets/Section.css';
import { useHistory } from '../hooks/useHistory.js';

const API_BASE = 'http://localhost:8080';

export default function Accounts() {
    const [tab, setTab] = useState('create');
    const [userId, setUserId] = useState('');
    const [accountId, setAccountId] = useState('');
    const [balance, setBalance] = useState(null);
    const [amount, setAmount] = useState('');
    const [error, setError] = useState('');
    const [history, addHistory] = useHistory([]);


    const createAccount = async () => {
        setError('');
        try {
            const res = await fetch(`${API_BASE}/accounts`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ userId })
            });
            if (!res.ok) {
                const msg = await res.text();
                setError(`Ошибка создания аккаунта: ${msg}`);
                addHistory(`Ошибка создания заказа: ${msg}`);
                return;
            }
            const data = await res.json();
            setAccountId(data.accountId);
            addHistory(`Создан аккаунт: ${data.accountId} (user: ${userId})`);
        } catch (err) {
            setError('Ошибка сети при создании аккаунта');
        }
    };

    const topUp = async () => {
        setError('');
        try {
            const res = await fetch(`${API_BASE}/accounts/${accountId}/topup`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ amount: Number(amount) })
            });
            if (!res.ok) {
                const msg = await res.text();
                setError(`Ошибка пополнения: ${msg}`);
                addHistory(`Ошибка пополнения: ${msg}`);
                return;
            }
            alert('Пополнено на ' + amount);
            addHistory(`Пополнен аккаунт ${accountId} на ${amount}`);
        } catch (err) {
            setError('Ошибка сети при пополнении');
        }
    };

    const fetchBalance = async () => {
        setError('');
        try {
            const res = await fetch(`${API_BASE}/accounts/${accountId}/balance`);
            if (!res.ok) {
                const msg = await res.text();
                setError(`Ошибка получения баланса: ${msg}`);
                addHistory(`Ошибка получения баланса: ${msg}`);
                setBalance(null);
                return;
            }
            const data = await res.json();
            setBalance(data.balance);
            addHistory(`На аккаунте ${accountId} всего средств: ${data.balance}`);
        } catch (err) {
            setError('Ошибка сети при получении баланса');
            setBalance(null);
        }
    };

    return (
        <div className="page-flex-container">
            <div className="page-main">
                <h2>Аккаунты</h2>
                <div className="page-tabs">
                    <button className={tab === 'create' ? 'active' : ''} onClick={() => setTab('create')}>Создать аккаунт</button>
                    <button className={tab === 'topup' ? 'active' : ''} onClick={() => setTab('topup')}>Пополнить</button>
                    <button className={tab === 'balance' ? 'active' : ''} onClick={() => setTab('balance')}>Проверить баланс</button>
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
                            <button className="page-btn" onClick={createAccount}>Создать аккаунт</button>
                            {accountId && (
                                <div className="page-info">
                                    ID созданного аккаунта: <b>{accountId}</b>
                                </div>
                            )}
                        </div>
                    )}
                    {tab === 'topup' && (
                        <div className="page-section">
                            <label>ID аккаунта:</label>
                            <input
                                className="page-input"
                                value={accountId}
                                onChange={e => setAccountId(e.target.value)}
                                placeholder="Введите ID аккаунта"
                            />
                            <label>Сумма:</label>
                            <input
                                className="page-input"
                                value={amount}
                                onChange={e => setAmount(e.target.value)}
                                placeholder="Введите сумму"
                                type="number"
                                min="0"
                            />
                            <button className="page-btn" onClick={topUp}>Пополнить</button>
                        </div>
                    )}
                    {tab === 'balance' && (
                        <div className="page-section">
                            <label>ID аккаунта:</label>
                            <input
                                className="page-input"
                                value={accountId}
                                onChange={e => setAccountId(e.target.value)}
                                placeholder="Введите ID аккаунта"
                            />
                            <button className="page-btn" onClick={fetchBalance}>Получить баланс</button>
                            {balance !== null && (
                                <div className="page-balance">
                                    Баланс: <b>{balance}</b>
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