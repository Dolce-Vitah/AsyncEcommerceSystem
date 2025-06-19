import React from 'react';
import '../assets/App.css';
import image from '../assets/meme.jpg';

const Home = () => (
    <div className="home-flex">
        <div className="home-center">
            <h1>Welcome to My Last KPO Assignment!</h1>
            <p className="home-lead">В разделе "Аккаунты" можно выполнить следующие действия:</p>
            <ul className="home-list">
                <li>Создать новый аккаунт</li>
                <li>Пополнить баланс аккаунта</li>
                <li>Проверить баланс аккаунта</li>
            </ul>
            <p className="home-lead">А в разделе "Заказы" доступны действия:</p>
            <ul className="home-list">
                <li>Создать новый заказ</li>
                <li>Просмотреть заказы конкретного пользователя</li>
                <li>Проверить статус заказа</li>
            </ul>
        </div>
        <div className="home-image-container">
            <img src={image} alt="meme" className="home-image" />
        </div>
    </div>
);

export default Home;
