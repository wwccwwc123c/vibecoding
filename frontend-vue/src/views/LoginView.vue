<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { api, TOKEN_KEY } from '../api/client'

const router = useRouter()
const route = useRoute()

const userName = ref('')
const password = ref('')
const loading = ref(false)
const isRegister = ref(false)

const wrapEl = ref(null)
const cardEl = ref(null)
const look = ref({ x: 0, y: 0 })
let raf = 0

async function submit() {
  const name = userName.value.trim()
  if (!name || !password.value) {
    ElMessage.warning('请输入用户名和密码')
    return
  }
  if (password.value.length < 6) {
    ElMessage.warning('密码至少 6 位')
    return
  }

  loading.value = true
  try {
    if (isRegister.value) {
      await api.post('/api/auth/register', {
        userName: name,
        password: password.value
      })
      ElMessage.success('注册成功，请登录')
      isRegister.value = false
    } else {
      const res = await api.post('/api/auth/login', {
        userName: name,
        password: password.value
      })
      localStorage.setItem(TOKEN_KEY, res.data.token)
      ElMessage.success('登录成功')
      const redirect = route.query.redirect || '/'
      await router.replace(String(redirect))
    }
  } catch (error) {
    ElMessage.error(error?.response?.data || error?.message || '请求失败')
  } finally {
    loading.value = false
  }
}

function toggleMode() {
  isRegister.value = !isRegister.value
}

function clamp(n, min, max) {
  return Math.max(min, Math.min(max, n))
}

function scheduleLook(clientX, clientY) {
  if (!cardEl.value) return
  if (raf) cancelAnimationFrame(raf)
  raf = requestAnimationFrame(() => {
    const rect = cardEl.value.getBoundingClientRect()
    const cx = rect.left + rect.width / 2
    const cy = rect.top + rect.height / 2
    const dx = (clientX - cx) / (rect.width / 2)
    const dy = (clientY - cy) / (rect.height / 2)

    // 眼球偏移（px）
    look.value = {
      x: clamp(dx * 6, -6, 6),
      y: clamp(dy * 4, -4, 4)
    }
  })
}

function onMouseMove(e) {
  scheduleLook(e.clientX, e.clientY)
}

function onMouseLeave() {
  if (raf) cancelAnimationFrame(raf)
  raf = requestAnimationFrame(() => {
    look.value = { x: 0, y: 0 }
  })
}

onMounted(() => {
  const el = wrapEl.value
  if (!el) return
  el.addEventListener('mousemove', onMouseMove, { passive: true })
  el.addEventListener('mouseleave', onMouseLeave, { passive: true })
})

onUnmounted(() => {
  const el = wrapEl.value
  if (!el) return
  el.removeEventListener('mousemove', onMouseMove)
  el.removeEventListener('mouseleave', onMouseLeave)
  if (raf) cancelAnimationFrame(raf)
})
</script>

<template>
  <div
    ref="wrapEl"
    class="login-wrap"
    :style="{ '--look-x': look.x + 'px', '--look-y': look.y + 'px' }"
  >
    <div class="scene-decor" aria-hidden="true">
      <span class="sparkle s1"></span>
      <span class="sparkle s2"></span>
      <span class="sparkle s3"></span>
      <span class="sparkle s4"></span>
      <span class="paw-outline p1"></span>
      <span class="paw-outline p2"></span>
      <span class="fish fish-top"></span>
      <span class="fish fish-right"></span>
      <span class="yarn yarn-left"></span>
      <span class="yarn yarn-right"></span>
      <span class="tail"></span>
      <span class="cat-cloud"></span>
    </div>
    <el-card ref="cardEl" class="login-card">
      <div class="cat-hero" aria-hidden="true">
        <div class="cat-face">
          <div class="cat-ear left"></div>
          <div class="cat-ear right"></div>
          <div class="cat-cheek left"></div>
          <div class="cat-cheek right"></div>

          <div class="cat-glasses">
            <div class="lens">
              <div class="pupil"></div>
              <div class="shine"></div>
            </div>
            <div class="bridge"></div>
            <div class="lens">
              <div class="pupil"></div>
              <div class="shine"></div>
            </div>
          </div>

          <div class="cat-nose"></div>
          <div class="cat-mouth"></div>
          <div class="whiskers left">
            <span></span><span></span><span></span>
          </div>
          <div class="whiskers right">
            <span></span><span></span><span></span>
          </div>

          <div class="blink"></div>
        </div>
      </div>

      <div class="card-head">
        <h2>{{ isRegister ? '喵星球注册' : '喵星球登录' }}</h2>
        <p>欢迎回家，继续和小猫记录今天的温柔日常</p>
      </div>
      <el-form label-position="top" @submit.prevent="submit">
        <el-form-item label="用户名 / 邮箱">
          <el-input
            v-model="userName"
            maxlength="64"
            autocomplete="username"
            placeholder="输入用户名/邮箱"
          >
            <template #prefix>
              <span class="input-icon paw"></span>
            </template>
            <template #suffix>
              <span class="input-icon kitty"></span>
            </template>
          </el-input>
        </el-form-item>
        <el-form-item label="密码">
          <el-input
            v-model="password"
            type="password"
            show-password
            autocomplete="current-password"
            placeholder="输入密码"
            @keyup.enter="submit"
          >
            <template #prefix>
              <span class="input-icon lock"></span>
            </template>
          </el-input>
        </el-form-item>
        <div class="submit-shell">
          <el-button type="primary" class="full submit-btn" :loading="loading" @click="submit">
            {{ isRegister ? '注册' : '登录' }}
          </el-button>
          <div class="button-cat" aria-hidden="true">
            <div class="button-cat-face">
              <span class="mini-ear left"></span>
              <span class="mini-ear right"></span>
              <span class="mini-eye left"></span>
              <span class="mini-eye right"></span>
              <span class="mini-nose"></span>
              <span class="mini-mouth"></span>
            </div>
          </div>
        </div>
        <div class="action-row">
          <el-button class="switch-btn" link type="primary" @click="toggleMode">
            {{ isRegister ? '已有账号？去登录' : '没有账号？去注册' }}
          </el-button>
          <button class="ghost-link" type="button">忘记密码？</button>
        </div>
      </el-form>
    </el-card>
  </div>
</template>

<style scoped>
.login-wrap {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background:
    radial-gradient(circle at 50% 102%, rgba(255, 245, 224, 0.96) 0 38%, transparent 39%),
    linear-gradient(180deg, #fcd9d2 0%, #ffe6dc 48%, #fff8ef 100%);
  position: relative;
  overflow: hidden;
  padding: 24px;
  --look-x: 0px;
  --look-y: 0px;
}

.login-card {
  position: relative;
  width: 520px;
  border-radius: 42px;
  border: 1px solid rgba(255, 255, 255, 0.75);
  box-shadow:
    0 28px 70px rgba(229, 167, 141, 0.22),
    inset 0 1px 0 rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.78);
  backdrop-filter: blur(8px);
  padding: 28px 34px 30px;
}

:deep(.el-card__body) {
  position: relative;
  z-index: 1;
}

.scene-decor {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.sparkle {
  position: absolute;
  width: 18px;
  height: 18px;
  opacity: 0.8;
}

.sparkle::before,
.sparkle::after {
  content: '';
  position: absolute;
  background: rgba(255, 255, 255, 0.88);
  border-radius: 999px;
}

.sparkle::before {
  inset: 0 7px;
}

.sparkle::after {
  inset: 7px 0;
}

.s1 {
  top: 14%;
  left: 20%;
}

.s2 {
  top: 24%;
  right: 17%;
  transform: scale(0.8);
}

.s3 {
  top: 34%;
  left: 16%;
  transform: scale(0.7);
}

.s4 {
  top: 12%;
  right: 28%;
  transform: scale(0.7);
}

.paw-outline,
.paw-outline::before,
.paw-outline::after {
  position: absolute;
  border: 4px solid rgba(255, 255, 255, 0.45);
}

.paw-outline {
  width: 22px;
  height: 18px;
  border-radius: 50% 50% 45% 45%;
}

.paw-outline::before,
.paw-outline::after {
  content: '';
  width: 10px;
  height: 12px;
  border-radius: 50%;
  top: -12px;
}

.paw-outline::before {
  left: -2px;
}

.paw-outline::after {
  right: -2px;
}

.p1 {
  top: 9%;
  left: 9%;
  transform: rotate(-18deg);
}

.p2 {
  top: 18%;
  right: 8%;
  transform: rotate(14deg) scale(0.9);
}

.cat-cloud {
  position: absolute;
  left: 8%;
  bottom: 18%;
  width: 180px;
  height: 88px;
  border: 5px solid rgba(255, 255, 255, 0.38);
  border-bottom: none;
  border-radius: 80px 80px 0 0;
  opacity: 0.55;
}

.cat-cloud::before,
.cat-cloud::after {
  content: '';
  position: absolute;
  top: -30px;
  width: 52px;
  height: 52px;
  border: 5px solid rgba(255, 255, 255, 0.38);
  border-bottom: none;
  border-left: none;
  background: transparent;
  transform: rotate(-45deg);
}

.cat-cloud::before {
  left: 18px;
}

.cat-cloud::after {
  right: 18px;
}

.fish {
  position: absolute;
  width: 56px;
  height: 28px;
  border-radius: 50% 55% 55% 50%;
  border: 2px solid rgba(230, 153, 102, 0.7);
  background: rgba(255, 201, 126, 0.5);
}

.fish::before {
  content: '';
  position: absolute;
  right: -12px;
  top: 7px;
  width: 14px;
  height: 14px;
  border-top: 2px solid rgba(230, 153, 102, 0.7);
  border-right: 2px solid rgba(230, 153, 102, 0.7);
  transform: rotate(45deg);
}

.fish::after {
  content: '';
  position: absolute;
  left: 12px;
  top: 12px;
  width: 18px;
  height: 2px;
  background: rgba(230, 153, 102, 0.6);
  box-shadow: 10px -5px 0 rgba(230, 153, 102, 0.6), 10px 5px 0 rgba(230, 153, 102, 0.6);
}

.fish-top {
  top: 11%;
  right: 10%;
  transform: rotate(-16deg);
}

.fish-right {
  right: 7%;
  bottom: 25%;
  width: 52px;
  background: rgba(162, 202, 255, 0.45);
  border-color: rgba(103, 147, 210, 0.65);
}

.fish-right::before,
.fish-right::after {
  border-color: rgba(103, 147, 210, 0.65);
  background: rgba(103, 147, 210, 0.65);
}

.yarn {
  position: absolute;
  width: 92px;
  height: 92px;
  border-radius: 50%;
  border: 3px solid rgba(111, 151, 230, 0.5);
}

.yarn::before,
.yarn::after {
  content: '';
  position: absolute;
  inset: 16px;
  border-radius: 50%;
  border: 3px solid rgba(111, 151, 230, 0.45);
}

.yarn::after {
  inset: 28px 12px;
  transform: rotate(35deg);
}

.yarn-left {
  left: 14%;
  bottom: 18%;
}

.yarn-right {
  right: 13%;
  bottom: 14%;
  border-color: rgba(255, 164, 142, 0.55);
}

.yarn-right::before,
.yarn-right::after {
  border-color: rgba(255, 164, 142, 0.5);
}

.tail {
  position: absolute;
  top: 36%;
  right: 20%;
  width: 74px;
  height: 18px;
  border: 3px solid #f2a247;
  border-left: none;
  border-radius: 0 20px 20px 0;
  transform: rotate(-10deg);
  box-shadow:
    -18px 0 0 -14px #fff,
    -18px 0 0 -11px #f2a247,
    -36px 0 0 -14px #fff,
    -36px 0 0 -11px #f2a247;
}

.cat-hero {
  position: absolute;
  left: 50%;
  bottom: 68px;
  transform: translateX(-50%);
  z-index: 3;
  pointer-events: none;
}

.cat-face {
  width: 78px;
  height: 62px;
  background: linear-gradient(180deg, #fffef9, #fff4ee);
  border: 3px solid #ffffff;
  border-radius: 36px;
  position: relative;
  box-shadow: 0 10px 24px rgba(233, 146, 124, 0.2);
  transform-origin: 50% 80%;
  animation: bob 3.2s ease-in-out infinite;
}

.cat-ear {
  position: absolute;
  top: -10px;
  width: 20px;
  height: 20px;
  background: #fff7f1;
  border: 3px solid #ffffff;
  border-radius: 6px;
  transform: rotate(45deg);
}

.cat-ear.left {
  left: 10px;
}

.cat-ear.right {
  right: 10px;
}

@keyframes bob {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-4px);
  }
}

.cat-cheek {
  position: absolute;
  top: 33px;
  width: 12px;
  height: 8px;
  background: rgba(255, 157, 122, 0.2);
  border-radius: 999px;
}

.cat-cheek.left {
  left: 8px;
}

.cat-cheek.right {
  right: 8px;
}

.cat-glasses {
  position: absolute;
  top: 14px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 6px;
}

.lens {
  width: 24px;
  height: 20px;
  border-radius: 12px;
  border: 2px solid rgba(224, 131, 111, 0.45);
  background: rgba(255, 255, 255, 0.72);
  position: relative;
  overflow: hidden;
  box-shadow: 0 4px 8px rgba(125, 72, 57, 0.06);
}

.bridge {
  width: 8px;
  height: 4px;
  border-top: 2px solid rgba(224, 131, 111, 0.45);
  border-radius: 10px;
}

.pupil {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 8px;
  height: 8px;
  background: #2f1f1a;
  border-radius: 50%;
  transform: translate(calc(-50% + var(--look-x) * 0.5), calc(-50% + var(--look-y) * 0.5));
  transition: transform 60ms linear;
}

.pupil::after {
  content: '';
  position: absolute;
  top: 1px;
  left: 2px;
  width: 3px;
  height: 3px;
  background: rgba(255, 255, 255, 0.8);
  border-radius: 50%;
}

.shine {
  position: absolute;
  inset: -8px -8px auto auto;
  width: 24px;
  height: 16px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.65), rgba(255, 255, 255, 0));
  transform: rotate(12deg);
}

.cat-nose {
  position: absolute;
  top: 34px;
  left: 50%;
  width: 6px;
  height: 5px;
  transform: translateX(-50%);
  background: #ff9d7a;
  border-radius: 3px 3px 6px 6px;
}

.cat-mouth {
  position: absolute;
  top: 41px;
  left: 50%;
  width: 12px;
  height: 6px;
  transform: translateX(-50%);
}

.cat-mouth::before,
.cat-mouth::after {
  content: '';
  position: absolute;
  top: 0;
  width: 7px;
  height: 5px;
  border-bottom: 2px solid rgba(125, 72, 57, 0.48);
  border-radius: 0 0 12px 12px;
}

.cat-mouth::before {
  left: 0;
  transform: rotate(8deg);
}

.cat-mouth::after {
  right: 0;
  transform: rotate(-8deg);
}

.whiskers {
  position: absolute;
  top: 38px;
  display: grid;
  gap: 3px;
}

.whiskers span {
  width: 16px;
  height: 1.5px;
  background: rgba(125, 72, 57, 0.35);
  border-radius: 2px;
}

.whiskers.left {
  left: -5px;
}

.whiskers.left span {
  transform-origin: left center;
}

.whiskers.left span:nth-child(1) {
  transform: rotate(10deg);
}

.whiskers.left span:nth-child(2) {
  transform: rotate(0deg);
}

.whiskers.left span:nth-child(3) {
  transform: rotate(-10deg);
}

.whiskers.right {
  right: -5px;
}

.whiskers.right span {
  transform-origin: right center;
}

.whiskers.right span:nth-child(1) {
  transform: rotate(-10deg);
}

.whiskers.right span:nth-child(2) {
  transform: rotate(0deg);
}

.whiskers.right span:nth-child(3) {
  transform: rotate(10deg);
}

/* 眨眼遮罩（定时动画） */
.blink {
  position: absolute;
  left: 50%;
  top: 14px;
  width: 64px;
  height: 20px;
  transform: translateX(-50%);
  pointer-events: none;
}

.blink::before,
.blink::after {
  content: '';
  position: absolute;
  top: 0;
  width: 24px;
  height: 20px;
  background: linear-gradient(180deg, #fff6f0, #fff6f0);
  border-radius: 12px;
  transform: scaleY(0);
  transform-origin: 50% 50%;
  animation: blink 5.2s infinite;
}

.blink::before {
  left: 4px;
}

.blink::after {
  right: 4px;
}

@keyframes blink {
  0%,
  92%,
  100% {
    transform: scaleY(0);
  }
  94% {
    transform: scaleY(1);
  }
  96% {
    transform: scaleY(0);
  }
}

.card-head {
  margin-bottom: 22px;
  text-align: center;
}

.card-head p {
  margin: 8px 0 0;
  color: #b08373;
  font-size: 14px;
}

:deep(.el-form-item__label) {
  display: none;
}

:deep(.el-input__wrapper) {
  border-radius: 0;
  background: transparent;
  box-shadow: none;
  border-bottom: 1.5px solid #dfc7bb;
  padding: 10px 0 12px;
}

:deep(.el-input__wrapper.is-focus) {
  box-shadow: none;
  border-bottom-color: #ef9b8a;
}

:deep(.el-input__inner) {
  color: #7f6255;
  font-size: 16px;
}

:deep(.el-input__inner::placeholder) {
  color: #a8887c;
}

h2 {
  margin: 0;
  color: #fffdf7;
  font-size: 30px;
  letter-spacing: 2px;
  text-shadow:
    -2px -2px 0 #8e4f42,
    2px -2px 0 #8e4f42,
    -2px 2px 0 #8e4f42,
    2px 2px 0 #8e4f42,
    0 4px 10px rgba(255, 255, 255, 0.55);
}

.full {
  width: 100%;
}

.input-icon {
  position: relative;
  display: inline-block;
  width: 18px;
  height: 18px;
}

.input-icon.paw::before,
.input-icon.paw::after {
  content: '';
  position: absolute;
  background: #f2a18c;
  border-radius: 50%;
}

.input-icon.paw::before {
  inset: 8px 2px 0;
  border-radius: 8px 8px 10px 10px;
}

.input-icon.paw::after {
  top: 1px;
  left: 2px;
  width: 4px;
  height: 4px;
  box-shadow: 5px 0 0 #f2a18c, 10px 0 0 #f2a18c, 3px 4px 0 #f2a18c;
}

.input-icon.kitty::before,
.input-icon.kitty::after {
  content: '';
  position: absolute;
  background: transparent;
}

.input-icon.kitty {
  border: 1.8px solid #f2a18c;
  border-radius: 50% 50% 46% 46%;
  width: 14px;
  height: 12px;
}

.input-icon.kitty::before,
.input-icon.kitty::after {
  top: -3px;
  width: 5px;
  height: 5px;
  border-top: 1.8px solid #f2a18c;
  border-left: 1.8px solid #f2a18c;
  transform: rotate(45deg);
}

.input-icon.kitty::before {
  left: 1px;
}

.input-icon.kitty::after {
  right: 1px;
}

.input-icon.lock {
  width: 14px;
  height: 14px;
  border: 1.8px solid #f2a18c;
  border-radius: 2px;
  margin-left: 2px;
}

.input-icon.lock::before {
  content: '';
  position: absolute;
  left: 2px;
  top: -7px;
  width: 8px;
  height: 8px;
  border: 1.8px solid #f2a18c;
  border-bottom: none;
  border-radius: 8px 8px 0 0;
}

.submit-shell {
  position: relative;
  margin-top: 24px;
}

.submit-btn {
  height: 54px;
  border: none;
  border-radius: 999px;
  background: linear-gradient(90deg, #ffb39f, #ffa695);
  font-size: 24px;
  font-weight: 700;
  letter-spacing: 2px;
  color: #ffffff;
  box-shadow: 0 14px 28px rgba(255, 172, 152, 0.28);
}

.submit-btn:hover {
  opacity: 0.96;
}

.button-cat {
  position: absolute;
  left: 50%;
  top: 50%;
  width: 66px;
  height: 66px;
  transform: translate(-50%, -50%);
  border-radius: 50%;
  background: #ffffff;
  box-shadow: 0 10px 26px rgba(239, 155, 138, 0.28);
  display: grid;
  place-items: center;
}

.button-cat-face {
  position: relative;
  width: 40px;
  height: 30px;
  background: #fffaf6;
  border: 2px solid #f1a28f;
  border-radius: 18px;
}

.mini-ear {
  position: absolute;
  top: -7px;
  width: 11px;
  height: 11px;
  background: #fffaf6;
  border: 2px solid #f1a28f;
  transform: rotate(45deg);
}

.mini-ear.left {
  left: 5px;
}

.mini-ear.right {
  right: 5px;
}

.mini-eye {
  position: absolute;
  top: 11px;
  width: 4px;
  height: 4px;
  background: #7d4839;
  border-radius: 50%;
}

.mini-eye.left {
  left: 11px;
}

.mini-eye.right {
  right: 11px;
}

.mini-nose {
  position: absolute;
  left: 50%;
  top: 16px;
  width: 5px;
  height: 4px;
  background: #f39f89;
  border-radius: 4px;
  transform: translateX(-50%);
}

.mini-mouth {
  position: absolute;
  left: 50%;
  top: 20px;
  width: 10px;
  height: 5px;
  transform: translateX(-50%);
}

.mini-mouth::before,
.mini-mouth::after {
  content: '';
  position: absolute;
  top: 0;
  width: 5px;
  height: 4px;
  border-bottom: 1.6px solid #7d4839;
  border-radius: 0 0 8px 8px;
}

.mini-mouth::before {
  left: 0;
}

.mini-mouth::after {
  right: 0;
}

.action-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 18px;
}

.switch-btn,
.ghost-link {
  padding: 0;
  border: none;
  background: transparent;
  color: #916255;
  font-size: 16px;
  cursor: pointer;
}

.ghost-link:hover,
.switch-btn:hover {
  color: #e28774;
}

@media (max-width: 720px) {
  .login-wrap {
    padding: 16px;
  }

  .login-card {
    width: min(100%, 420px);
    padding: 24px 22px 26px;
    border-radius: 30px;
  }

  h2 {
    font-size: 24px;
  }

  .submit-btn {
    height: 50px;
    font-size: 20px;
  }

  .button-cat {
    width: 58px;
    height: 58px;
  }

  .cat-cloud,
  .yarn-right,
  .fish-right {
    display: none;
  }
}
</style>
