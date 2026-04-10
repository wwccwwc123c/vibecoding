const API_BASE = "http://localhost:5231";

const createForm = document.getElementById("createForm");
const titleInput = document.getElementById("titleInput");
const isDoneInput = document.getElementById("isDoneInput");
const todoList = document.getElementById("todoList");
const message = document.getElementById("message");
const refreshBtn = document.getElementById("refreshBtn");

function setMessage(text = "") {
  message.textContent = text;
}

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `HTTP ${response.status}`);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

async function loadTodos() {
  try {
    setMessage("");
    const todos = await request("/api/todos");
    renderTodos(todos);
  } catch (error) {
    setMessage(`加载失败: ${error.message}`);
  }
}

function renderTodos(todos) {
  todoList.innerHTML = "";

  if (!Array.isArray(todos) || todos.length === 0) {
    const empty = document.createElement("li");
    empty.textContent = "暂无待办";
    todoList.appendChild(empty);
    return;
  }

  for (const todo of todos) {
    const item = document.createElement("li");
    item.className = "todo-item";

    const main = document.createElement("div");
    main.className = "todo-main";

    const editRow = document.createElement("div");
    editRow.className = "todo-edit-row";

    const doneCheckbox = document.createElement("input");
    doneCheckbox.type = "checkbox";
    doneCheckbox.checked = todo.isDone === true;

    const titleInputInline = document.createElement("input");
    titleInputInline.type = "text";
    titleInputInline.maxLength = 100;
    titleInputInline.value = todo.title ?? "";
    if (todo.isDone) {
      titleInputInline.classList.add("done");
    }

    doneCheckbox.addEventListener("change", () => {
      if (doneCheckbox.checked) {
        titleInputInline.classList.add("done");
      } else {
        titleInputInline.classList.remove("done");
      }
    });

    editRow.appendChild(doneCheckbox);
    editRow.appendChild(titleInputInline);

    const meta = document.createElement("div");
    meta.className = "todo-meta";
    meta.textContent = `ID: ${todo.id} | 创建时间: ${formatDate(todo.createdAt)}`;

    main.appendChild(editRow);
    main.appendChild(meta);

    const actions = document.createElement("div");
    actions.className = "actions";

    const saveBtn = document.createElement("button");
    saveBtn.textContent = "保存";
    saveBtn.addEventListener("click", async () => {
      await updateTodo(todo.id, {
        title: titleInputInline.value.trim(),
        isDone: doneCheckbox.checked
      });
    });

    const deleteBtn = document.createElement("button");
    deleteBtn.className = "danger";
    deleteBtn.textContent = "删除";
    deleteBtn.addEventListener("click", async () => {
      await deleteTodo(todo.id);
    });

    actions.appendChild(saveBtn);
    actions.appendChild(deleteBtn);

    item.appendChild(main);
    item.appendChild(actions);
    todoList.appendChild(item);
  }
}

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

async function createTodo(payload) {
  try {
    setMessage("");
    await request("/api/todos", {
      method: "POST",
      body: JSON.stringify(payload)
    });
    await loadTodos();
  } catch (error) {
    setMessage(`新增失败: ${error.message}`);
  }
}

async function updateTodo(id, payload) {
  try {
    if (!payload.title) {
      setMessage("标题不能为空");
      return;
    }
    setMessage("");
    await request(`/api/todos/${id}`, {
      method: "PUT",
      body: JSON.stringify(payload)
    });
    await loadTodos();
  } catch (error) {
    setMessage(`更新失败: ${error.message}`);
  }
}

async function deleteTodo(id) {
  try {
    setMessage("");
    await request(`/api/todos/${id}`, { method: "DELETE" });
    await loadTodos();
  } catch (error) {
    setMessage(`删除失败: ${error.message}`);
  }
}

createForm.addEventListener("submit", async (event) => {
  event.preventDefault();
  const title = titleInput.value.trim();
  if (!title) {
    setMessage("请输入待办内容");
    return;
  }

  await createTodo({
    title,
    isDone: isDoneInput.checked
  });
  titleInput.value = "";
  isDoneInput.checked = false;
});

refreshBtn.addEventListener("click", async () => {
  await loadTodos();
});

loadTodos();
