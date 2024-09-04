# Use an official Node runtime as the base image
FROM node:20-alpine AS build

# Set the working directory in the container
WORKDIR /app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the application code
COPY . .

# Build the Angular app
RUN npm run build

# Use Alpine-based Nginx to serve the built Angular app
FROM nginx:alpine

# Copy the build output to replace the default nginx contents
COPY --from=build /app/dist/webapp/* /usr/share/nginx/html/

# Copy the env.js file
COPY src/env.js /usr/share/nginx/html/env.js

# Install envsubst (it's included in the libintl package)
RUN apk add --no-cache libintl

# Copy nginx configuration
COPY default.conf /etc/nginx/conf.d

# Expose port 80
EXPOSE 80

# Start Nginx and replace environment variables in env.js
# CMD ["/bin/sh", "-c", "envsubst < /usr/share/nginx/html/env.js > /usr/share/nginx/html/env.js && nginx -g 'daemon off;'"]
CMD ["/bin/sh", "-c", "cat /usr/share/nginx/html/env.js && echo 'Original content' && envsubst < /usr/share/nginx/html/env.js > /usr/share/nginx/html/env.js.tmp && mv /usr/share/nginx/html/env.js.tmp /usr/share/nginx/html/env.js && cat /usr/share/nginx/html/env.js && echo 'Processed content' && nginx -g 'daemon off;'"]