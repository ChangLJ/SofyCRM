# ============================================================
# SofyCRM Frontend — Vite build + nginx（Cloud Run / Docker 通用）
# 固定 Mock Data，不需後端 API
# ============================================================
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install --no-audit --no-fund

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS runtime

# 官方映像會用 envsubst 處理 templates 內的 ${PORT}（Cloud Run 注入 PORT=8080）
ENV PORT=8080
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 8080
