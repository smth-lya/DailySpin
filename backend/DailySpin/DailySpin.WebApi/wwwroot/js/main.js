const AuthModule = (() => {
    const apiUrl = '/api/auth';
    let jwtToken = null;

    const usernamePattern = /^[a-z0-9_]+$/;
    const usernameMinLength = 3;
    const usernameMaxLength = 32;
    const passwordPattern = /^[a-zA-Z0-9_]+$/;
    const passwordMinLength = 6;
    const passwordMaxLength = 32;

    const setToken = (token) => {
        jwtToken = token;
        localStorage.setItem('jwtToken', token);
    };

    const getToken = () => {
        if (!jwtToken) {
            jwtToken = localStorage.getItem('jwtToken');
        }
        return jwtToken;
    };

    const clearToken = () => {
        jwtToken = null;
        localStorage.removeItem('jwtToken');
    };

    const validateToken = async () => {
        const token = getToken();
        if (!token) return false;

        try {
            const response = await fetch(`${apiUrl}/validate`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });

            return response.ok;
        } catch (error) {
            console.error('Ошибка проверки токена:', error.message);
            return false;
        }
    };

    const fetchCurrentUser = async () => {
        try {
            const response = await fetch(`${apiUrl}/user`, {
                headers: {
                    'Authorization': `Bearer ${getToken()}`
                }
            });

            if (!response.ok) {
                throw new Error('Ошибка получения данных пользователя');
            }

            return await response.json();
        } catch (error) {
            console.error('Ошибка получения текущего пользователя:', error.message);
            return null;
        }
    };

    const toggleForm = (type) => {
        const loginForm = document.getElementById('login-form');
        const registerForm = document.getElementById('register-form');

        if (type === 'login') {
            loginForm.classList.remove('hidden');
            registerForm.classList.add('hidden');
        } else {
            loginForm.classList.add('hidden');
            registerForm.classList.remove('hidden');
        }
    };

    const validateUsername = (username) => {
        if (username.length < usernameMinLength) {
            return `Логин должен быть не короче ${usernameMinLength} символов.`;
        }
        if (username.length > usernameMaxLength) {
            return `Логин должен быть не длиннее ${usernameMaxLength} символов.`;
        }
        if (!usernamePattern.test(username)) {
            return 'Логин должен содержать только строчные буквы, цифры и символы подчеркивания.';
        }
        return '';
    };

    const validatePassword = (password) => {
        if (password.length < passwordMinLength) {
            return `Пароль должен быть не короче ${passwordMinLength} символов.`;
        }
        if (password.length > passwordMaxLength) {
            return `Пароль должен быть не длиннее ${passwordMaxLength} символов.`;
        }
        if (!passwordPattern.test(password)) {
            return 'Пароль должен содержать только буквы, цифры и символы подчеркивания.';
        }
        return '';
    };

    const displayError = (element, errorMessage) => {
        const errorElement = element.nextElementSibling;
        if (errorMessage) {
            errorElement.textContent = errorMessage;
            errorElement.classList.add('error');
            errorElement.classList.remove('valid');
        } else {
            errorElement.textContent = '';
            errorElement.classList.remove('error');
            errorElement.classList.add('valid');
        }
    };

    const validateAndDisplayErrors = (formType) => {
        const usernameField = document.getElementById(`${formType}-username`);
        const passwordField = document.getElementById(`${formType}-password`);

        const usernameErrorMessage = validateUsername(usernameField.value);
        displayError(usernameField, usernameErrorMessage);

        const passwordErrorMessage = validatePassword(passwordField.value);
        displayError(passwordField, passwordErrorMessage);

        const isUsernameValid = !usernameErrorMessage;
        const isPasswordValid = !passwordErrorMessage;

        return isUsernameValid && isPasswordValid;
    };

    const login = async (username, password) => {
        try {
            const response = await fetch(`${apiUrl}/signin`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            if (!response.ok) {
                throw new Error('Ошибка авторизации');
            }

            const data = await response.json();
            setToken(data.accessToken);
            SessionModule.setUser(data.user);
            TaskModule.loadTasks();
            ModalModule.hideModal();
        } catch (error) {
            console.error('Ошибка при входе:', error.message);
        }
    };

    const register = async (username, password) => {
        try {
            const response = await fetch(`${apiUrl}/signup`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            if (!response.ok) {
                throw new Error('Ошибка регистрации');
            }

            console.log('Регистрация успешна! Теперь вы можете войти.');
            toggleForm('login');
        } catch (error) {
            console.error('Ошибка при регистрации:', error.message);
        }
    };

    return {
        setToken,
        getToken,
        clearToken,
        validateToken,
        fetchCurrentUser,
        toggleForm,
        login,
        register,
        validateAndDisplayErrors
    };
})();

const SessionModule = (() => {
    let currentUser = null;

    const setUser = (user) => {
        currentUser = user;
        document.getElementById('user-info').textContent = `Привет, ${user.username}`;
        document.getElementById('auth-button').classList.add('hidden');
    };

    const getUser = () => currentUser;

    return { setUser, getUser };
})();

const TaskModule = (() => {
    const apiUrl = '/api/user/me';

    const loadTasks = async () => {
        try {
            const response = await fetch(apiUrl, {
                headers: { 'Authorization': `Bearer ${AuthModule.getToken()}` }
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
            taskElement.textContent = task.name;
            tasksContainer.appendChild(taskElement);
        });
    };

    return { loadTasks };
})();

const ModalModule = (() => {
    const showModal = (type = 'login') => {
        const modal = document.getElementById('auth-modal');
        modal.classList.remove('hidden');
        modal.classList.add('visible');
        AuthModule.toggleForm(type);
    };

    const hideModal = () => {
        const modal = document.getElementById('auth-modal');
        modal.classList.add('hidden');
        modal.classList.remove('visible');
    };

    return { showModal, hideModal };
})();

const App = (() => {

    const renderHTML = () => {
        document.body.innerHTML = `
            <header class="site-header">
                <div class="logo">
                    DailySpin
                </div>
                <nav class="nav-bar">
                    <ul>
                        <li><a href="/wheel" class="nav-item"><i class="fas fa-dice"></i> Колесо фортуны</a></li>
                        <li><a href="/tasks" class="nav-item"><i class="fas fa-tasks"></i> Доска задач</a></li>
                        <li><a href="/pdf-export" class="nav-item"><i class="fas fa-file-pdf"></i> Экспорт статистики</a></li>
                        <li><a href="/profile" class="nav-item"><i class="fas fa-user"></i> Профиль</a></li>
                    </ul>
                </nav>
                <div id="user-info">Вы не авторизованы</div>
                <button id="auth-button">Войти</button>
            </header>
            <main>
                <div id="tasks-container">Задачи загружаются...</div>
            </main>
            <div id="auth-modal" class="modal hidden">
                <div class="modal-content">
                    <span id="auth-modal-close" class="modal-close">&times;</span>
                    <form id="login-form" class="auth-form">
                        <label for="login-username">Имя пользователя:</label>
                        <input type="text" id="login-username" autocomplete="username" required>
                        <div class="error-message"></div>
                        <label for="login-password">Пароль:</label>
                        <input type="password" id="login-password" autocomplete="current-password" required>
                        <div class="error-message"></div>
                        <button type="submit">Войти</button>
                        <p>Нет аккаунта? <a id="toggle-to-register" href="#">Зарегистрироваться</a></p>
                    </form>
                    <form id="register-form" class="auth-form hidden">
                        <label for="register-username">Имя пользователя:</label>
                        <input type="text" id="register-username" autocomplete="new-username" required>
                        <div class="error-message"></div>
                        <label for="register-password">Пароль:</label>
                        <input type="password" id="register-password" autocomplete="new-password" required>
                        <div class="error-message"></div>
                        <button type="submit">Зарегистрироваться</button>
                        <p>Уже есть аккаунт? <a id="toggle-to-login" href="#">Войти</a></p>
                    </form>
                </div>
            </div>
        `;
    };

    const init = async () => {
        try {
            renderHTML();

            const isValidToken = await AuthModule.validateToken();
            if (!isValidToken) {
                ModalModule.showModal('login');
                return;
            }

            const user = await AuthModule.fetchCurrentUser();
            if (!user) throw new Error('Ошибка загрузки пользователя');

            SessionModule.setUser(user);
            await TaskModule.loadTasks();
        } catch (error) {
            console.error('Ошибка инициализации приложения:', error.message);
            ModalModule.showModal('login');
        }
    };
        
    return { init };
})();

window.addEventListener('DOMContentLoaded', () => {
    App.init();

    document.getElementById('auth-button').addEventListener('click', () => ModalModule.showModal('login'));
    document.getElementById('auth-modal-close').addEventListener('click', ModalModule.hideModal);

    document.getElementById('login-form').addEventListener('submit', (event) => {
        event.preventDefault();
        const username = document.getElementById('login-username').value;
        const password = document.getElementById('login-password').value;

        if (AuthModule.validateAndDisplayErrors('login')) {
            AuthModule.login(username, password);
        }
    });

    document.getElementById('register-form').addEventListener('submit', (event) => {
        event.preventDefault();
        const username = document.getElementById('register-username').value;
        const password = document.getElementById('register-password').value;

        if (AuthModule.validateAndDisplayErrors('register')) {
            AuthModule.register(username, password);
        }
    });

    document.getElementById('toggle-to-register').addEventListener('click', () => AuthModule.toggleForm('register'));
    document.getElementById('toggle-to-login').addEventListener('click', () => AuthModule.toggleForm('login'));
});
