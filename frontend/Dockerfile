# ---- build stage ----
    FROM node:22-alpine AS build
    WORKDIR /app
    COPY package*.json ./
    RUN npm ci
    COPY . .
    RUN npm run build
    # ---- runtime: nginx serwuje statyki ----
    FROM nginx:1.27-alpine
    # Angular 21 buduje do dist/<project>/browser — dostosuj nazwę jeśli inna
    COPY --from=build /app/dist/ /usr/share/nginx/html/
    # prosty config dla SPA fallback
    RUN printf 'server { listen 80; root /usr/share/nginx/html; index index.html; location / { try_files $uri $uri/ /index.html; } }' > /etc/nginx/conf.d/default.conf
    EXPOSE 80