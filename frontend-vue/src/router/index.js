import { createRouter, createWebHistory } from 'vue-router'
import { TOKEN_KEY } from '../api/client'

const LoginView = () => import('../views/LoginView.vue')
const TodoView = () => import('../views/TodoView.vue')

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: LoginView, meta: { public: true } },
    { path: '/', name: 'todos', component: TodoView }
  ]
})

router.beforeEach((to) => {
  if (to.meta.public) return true
  const token = localStorage.getItem(TOKEN_KEY)
  if (!token) return { name: 'login', query: { redirect: to.fullPath } }
  return true
})

export default router
