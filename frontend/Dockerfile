# =========================================
# Stage 1: Build the Angular Application
# =========================================
ARG NODE_VERSION=24.12.0-alpine
ARG NGINX_VERSION=alpine3.22

FROM node:${NODE_VERSION} AS builder

WORKDIR /app

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci

COPY . .

RUN npm run build

# =========================================
# Stage 2: Serve static files with Nginx (non-root)
# =========================================
FROM nginxinc/nginx-unprivileged:${NGINX_VERSION} AS runner

# Custom nginx config (listens on 8080, exposes /healthz)
COPY --chown=nginx:nginx nginx.conf /etc/nginx/nginx.conf

# Angular 17+/21 with @angular/build:application outputs to dist/<project>/browser
COPY --chown=nginx:nginx --from=builder /app/dist/frontend/browser/ /usr/share/nginx/html/

USER nginx

EXPOSE 8080

# nginx defines enteryPoint in the image
