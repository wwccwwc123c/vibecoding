# vibecoding

一个简单的全栈示例项目：

- `backend/`：ASP.NET Core Web API（JWT + MySQL）
- `frontend-vue/`：Vue 3 + Vite（使用 Axios 调后端）
- `frontend/`：静态页面示例（可选）

## 本地开发环境

- Node.js（建议 18+）
- .NET SDK（建议 8.x）
- MySQL（本地或容器均可）

## 后端启动（ASP.NET Core）

后端项目路径：`backend/Vibecoding.Api/`

### 1) 准备配置（非常重要）

本仓库的 `appsettings.json` **不应放真实密码/密钥**。请在本地创建你的开发配置文件：

1. 复制示例文件：
   - 从 `backend/Vibecoding.Api/appsettings.Development.json.example`
   - 复制为 `backend/Vibecoding.Api/appsettings.Development.json`
2. 修改其中的：
   - MySQL 连接串（用户名/密码/库名/端口）
   - `Jwt:Key`（长度至少 32 字符）

> 提示：`appsettings.Development.json` 已被加入忽略规则，不会被提交到 GitHub。

### 2) 运行数据库迁移（如果需要）

在 `backend/Vibecoding.Api/` 目录：

```bash
dotnet tool restore
dotnet ef database update
```

如果你没有安装 EF 工具，也可以：

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### 3) 启动 API

```bash
cd backend/Vibecoding.Api
dotnet run
```

开发环境默认地址（见 `launchSettings.json`）：

- `http://localhost:5231`
- `https://localhost:7201`

Swagger（Development 才会启用）：
- `http://localhost:5231/swagger`

## 前端启动（Vue 3 + Vite）

前端项目路径：`frontend-vue/`

```bash
cd frontend-vue
npm install
npm run dev
```

默认会运行在：
- `http://localhost:5173`

### API 地址

前端 API baseURL 配在：
- `frontend-vue/src/api/client.js`

默认是：
- `http://localhost:5231`

## 常见问题

### 1) CORS 报错

后端已允许这些来源（见 `Program.cs`）：

- `http://localhost:5173` / `http://127.0.0.1:5173`（Vite）
- `http://localhost:5500` / `http://127.0.0.1:5500`（静态页面）

如果你换了端口，需要把新地址加到后端的 CORS origins 里。

### 2) GitHub 上不小心提交了密码/密钥怎么办？

- 立刻更换数据库密码 / JWT Key
- 再提交一次，把仓库里的明文配置改成模板（本项目已按这个原则处理）

