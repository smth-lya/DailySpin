// Основной файл JavaScript фронтенда

// === Модуль для работы с авторизацией ===
const AuthModule = (() => {
    const apiUrl = '/api/auth'; // Эндпоинт авторизации

    // Показ модального окна
    const showModal = () => {
        const modal = document.getElementById('auth-modal');
        modal.classList.add('visible');
    };

    // Скрытие модального окна
    const hideModal = () => {
        const modal = document.getElementById('auth-modal');
        modal.classList.remove('visible');
    };

    // Отправка данных авторизации на сервер
    const login = async (email, password) => {
        try {
            const response = await fetch(`${apiUrl}/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ email, password })
            });

            if (!response.ok) {
                throw new Error('Ошибка авторизации');
            }

            const data = await response.json();
            SessionModule.setUser(data.user);
            hideModal();
            TaskModule.loadTasks();
        } catch (error) {
            console.error('Ошибка при входе:', error.message);
        }
    };

    return {
        showModal,
        hideModal,
        login
    };
})();

// === Модуль для работы с сессией ===
const SessionModule = (() => {
    let currentUser = null;

    const setUser = (user) => {
        currentUser = user;
        document.getElementById('user-info').textContent = `Привет, ${user.name}`;
        document.getElementById('auth-button').classList.add('hidden');
    };

    const getUser = () => currentUser;

    return {
        setUser,
        getUser
    };
})();

// === Модуль для работы с задачами ===
const TaskModule = (() => {
    const apiUrl = '/api/tasks'; // Эндпоинт задач

    const loadTasks = async () => {
        const user = SessionModule.getUser();
        if (!user) {
            AuthModule.showModal();
            return;
        }

        try {
            const response = await fetch(`${apiUrl}`, {
                headers: {
                    'Authorization': `Bearer ${user.token}`
                }
            });

            if (!response.ok) {
                throw new Error('Ошибка загрузки задач');
            }

            const tasks = await response.json();
            renderTasks(tasks);
        } catch (error) {
            console.error('Ошибка при загрузке задач:', error.message);
        }
    };

    const renderTasks = (tasks) => {
        const tasksContainer = document.getElementById('tasks-container');
        tasksContainer.innerHTML = '';

        tasks.forEach(task => {
            const taskElement = document.createElement('div');
            taskElement.className = 'task';
            taskElement.textContent = task.title;
            tasksContainer.appendChild(taskElement);
        });
    };

    return {
        loadTasks
    };
})();

// === Основной запуск ===
window.addEventListener('DOMContentLoaded', () => {
    // Добавление событий для авторизации
    document.getElementById('auth-button').addEventListener('click', AuthModule.showModal);

    document.getElementById('auth-modal-close').addEventListener('click', AuthModule.hideModal);

    document.getElementById('auth-form').addEventListener('submit', (event) => {
        event.preventDefault();
        const email = document.getElementById('auth-email').value;
        const password = document.getElementById('auth-password').value;
        AuthModule.login(email, password);
    });

    // Загрузка задач пользователя
    TaskModule.loadTasks();
});

// === HTML Шаблон страницы ===
document.body.innerHTML = `
    <header>
        <div id="user-info">Вы не авторизованы</div>
        <button id="auth-button">Войти</button>
    </header>
    <main>
        <div id="tasks-container">Задачи загружаются...</div>
    </main>
    <div id="auth-modal" class="modal hidden">
        <div class="modal-content">
            <span id="auth-modal-close" class="modal-close">&times;</span>
            <form id="auth-form">
                <label for="auth-email">Email:</label>
                <input type="email" id="auth-email" required>

                <label for="auth-password">Пароль:</label>
                <input type="password" id="auth-password" required>

                <button type="submit">Войти</button>
            </form>
        </div>
    </div>
`;

// === CSS Стили ===
const style = document.createElement('style');
style.textContent = `
    body {
        font-family: Arial, sans-serif;
        margin: 0;
        padding: 0;
    }

    header {
        background-color: #282c34;
        color: white;
        padding: 10px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    #tasks-container {
        padding: 20px;
    }

    .task {
        border: 1px solid #ddd;
        margin: 5px 0;
        padding: 10px;
        border-radius: 4px;
        background-color: #f9f9f9;
    }

    .modal {
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background-color: rgba(0, 0, 0, 0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        visibility: hidden;
        opacity: 0;
        transition: visibility 0s, opacity 0.3s;
    }

    .modal.visible {
        visibility: visible;
        opacity: 1;
    }

    .modal-content {
        background-color: white;
        padding: 20px;
        border-radius: 8px;
        width: 300px;
    }

    .modal-close {
        position: absolute;
        top: 10px;
        right: 10px;
        cursor: pointer;
    }

    button {
        cursor: pointer;
    }
`;
document.head.appendChild(style);
