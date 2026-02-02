FROM node:16-alpine
WORKDIR /app

RUN addgroup -g 1001 -S nodejs
RUN adduser -S nextjs -u 1001

# Change ownership and permissions for /app directory
RUN chown nextjs:nodejs /app
RUN chmod 755 /app

COPY --chown=nextjs:nodejs . .

USER nextjs

EXPOSE 3000
ENV PORT 3000
ENV NODE_ENV=production
ENV DOCKER 1
ENV NEXT_PUBLIC_WEBHOST="ci.suturehealth.com"

# Update the sed command to use the inline editing (-i) flag with a custom suffix for the backup file
RUN sed -i.bak -e 's/"REPLACE_ME_API_BASE"/process.env.NEXT_PUBLIC_WEBHOST/' server.js && rm server.js.bak
RUN sed -i.bak -e "s|REPLACE_ME_API_BASE|${NEXT_PUBLIC_WEBHOST}|g" next.config.js && rm next.config.js.bak


CMD ["node", "server.js"]
