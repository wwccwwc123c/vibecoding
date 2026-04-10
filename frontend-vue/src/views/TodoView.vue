<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { api, TOKEN_KEY } from '../api/client'

const router = useRouter()

const loading = ref(false)
const todos = ref([])

const createForm = ref({
  title: '',
  isDone: false
})

async function loadTodos() {
  loading.value = true
  try {
    const res = await api.get('/api/todos')
    todos.value = (res.data ?? []).map((x) => ({
      ...x,
      editingTitle: x.title
    }))
  } catch (error) {
    ElMessage.error(getError(error, '加载失败'))
  } finally {
    loading.value = false
  }
}

async function createTodo() {
  const title = createForm.value.title.trim()
  if (!title) {
    ElMessage.warning('请输入待办标题')
    return
  }

  try {
    await api.post('/api/todos', {
      title,
      isDone: createForm.value.isDone
    })
    ElMessage.success('新增成功')
    createForm.value.title = ''
    createForm.value.isDone = false
    await loadTodos()
  } catch (error) {
    ElMessage.error(getError(error, '新增失败'))
  }
}

async function saveTodo(row) {
  const title = (row.editingTitle ?? '').trim()
  if (!title) {
    ElMessage.warning('标题不能为空')
    return
  }

  try {
    await api.put(`/api/todos/${row.id}`, {
      title,
      isDone: row.isDone
    })
    ElMessage.success('保存成功')
    await loadTodos()
  } catch (error) {
    ElMessage.error(getError(error, '保存失败'))
  }
}

async function removeTodo(row) {
  try {
    await ElMessageBox.confirm(`确认删除「${row.title}」?`, '提示', {
      type: 'warning',
      confirmButtonText: '删除',
      cancelButtonText: '取消'
    })
  } catch {
    return
  }

  try {
    await api.delete(`/api/todos/${row.id}`)
    ElMessage.success('删除成功')
    await loadTodos()
  } catch (error) {
    ElMessage.error(getError(error, '删除失败'))
  }
}

function formatDate(value) {
  if (!value) return '-'
  const d = new Date(value)
  return Number.isNaN(d.getTime()) ? value : d.toLocaleString()
}

function getError(error, fallback) {
  return error?.response?.data || error?.message || fallback
}

function logout() {
  localStorage.removeItem(TOKEN_KEY)
  router.push('/login')
}

onMounted(loadTodos)
</script>

<template>
  <div class="page">
    <el-card>
      <template #header>
        <div class="header">
          <span>Todo 管理</span>
          <div class="header-actions">
            <el-button @click="loadTodos">刷新</el-button>
            <el-button @click="logout">退出登录</el-button>
          </div>
        </div>
      </template>

      <div class="create-row">
        <el-input
          v-model="createForm.title"
          maxlength="100"
          placeholder="输入待办标题"
          clearable
          @keyup.enter="createTodo"
        />
        <el-checkbox v-model="createForm.isDone">已完成</el-checkbox>
        <el-button type="primary" @click="createTodo">新增</el-button>
      </div>
    </el-card>

    <el-card class="mt-12">
      <el-table :data="todos" v-loading="loading" border stripe>
        <el-table-column prop="id" label="Id" width="80" />
        <el-table-column label="Title">
          <template #default="{ row }">
            <el-input v-model="row.editingTitle" maxlength="100" />
          </template>
        </el-table-column>
        <el-table-column label="IsDone" width="120">
          <template #default="{ row }">
            <el-switch v-model="row.isDone" />
          </template>
        </el-table-column>
        <el-table-column label="CreatedAt" width="200">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="180" fixed="right">
          <template #default="{ row }">
            <div class="actions">
              <el-button type="primary" link @click="saveTodo(row)">保存</el-button>
              <el-button type="danger" link @click="removeTodo(row)">删除</el-button>
            </div>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<style scoped>
.page {
  max-width: 1100px;
  margin: 24px auto;
  padding: 0 16px 24px;
}
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 18px;
  font-weight: 600;
}
.header-actions {
  display: flex;
  gap: 8px;
}
.create-row {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: 12px;
  align-items: center;
}
.mt-12 {
  margin-top: 12px;
}
.actions {
  display: flex;
  gap: 8px;
}
</style>
