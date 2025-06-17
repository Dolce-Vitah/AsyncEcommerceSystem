import { useState, useEffect } from 'react';

const HISTORY_KEY = 'app-action-history';

export function useHistory() {
    const [history, setHistory] = useState(() => {
        const stored = sessionStorage.getItem(HISTORY_KEY);
        return stored ? JSON.parse(stored) : [];
    });

    useEffect(() => {
        sessionStorage.setItem(HISTORY_KEY, JSON.stringify(history));
    }, [history]);

    const addHistory = (message) => {
        setHistory(h => {
            const newHistory = [
                { message, timestamp: new Date().toLocaleTimeString() },
                ...h
            ];
            return newHistory.slice(0, 100);
        });
    };

    return [history, addHistory];
}
