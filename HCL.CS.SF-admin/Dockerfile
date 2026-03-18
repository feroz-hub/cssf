FROM node:20-alpine AS builder
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN NEXTAUTH_URL=https://admin.example.com \
    NEXTAUTH_SECRET=build-time-nextauth-secret \
    HCL.CS.SF_ISSUER=https://identity.example.com \
    HCL.CS.SF_CLIENT_ID=build-time-client-id \
    HCL.CS.SF_CLIENT_SECRET=build-time-client-secret \
    HCL.CS.SF_API_BASE_URL=https://identity.example.com \
    HCL.CS.SF_INSTALLER_BASE_URL=https://installer.example.com \
    HCL.CS.SF_DEMO_SERVER_BASE_URL=https://identity.example.com \
    NEXT_TELEMETRY_DISABLED=1 \
    npm run build

FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV HOSTNAME=0.0.0.0
ENV PORT=3000

RUN addgroup -S nextjs && adduser -S nextjs -G nextjs

COPY --from=builder --chown=nextjs:nextjs /app/public ./public
COPY --from=builder --chown=nextjs:nextjs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nextjs /app/.next/static ./.next/static
RUN mkdir -p /app/.next/cache && chown -R nextjs:nextjs /app

USER nextjs

EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
  CMD sh -c 'wget -qO- "http://127.0.0.1:${PORT:-3000}/api/health" >/dev/null || exit 1'

CMD ["node", "server.js"]
