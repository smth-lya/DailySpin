const addTaskBtn = document.getElementById('add-task-btn');
const newTaskInput = document.getElementById('new-task');
const taskList = document.querySelector('.task-list');

addTaskBtn.addEventListener('click', () => {
    const taskText = newTaskInput.value.trim();
    if (taskText) {
        const li = document.createElement('li');
        li.className = 'task-item';
        li.innerHTML = `
            <input type="checkbox" id="task-${Date.now()}">
            <label for="task-${Date.now()}">${taskText}</label>
        `;
        taskList.appendChild(li);
        newTaskInput.value = '';
    }
});
