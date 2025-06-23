import { defineConfig } from 'astro/config';
import tailwindcss from '@tailwindcss/vite';
import sitemap from "@astrojs/sitemap";
import mdx from "@astrojs/mdx";
import node from '@astrojs/node'; // 👈 nuevo

export default defineConfig({
  adapter: node({ mode: 'standalone' }), // 👈 activar modo SSR
  vite: {
    plugins: [tailwindcss()],
  },
  markdown: {
    drafts: true,
    shikiConfig: {
      theme: "css-variables"
    }
  },
  site: 'https://yourwebsite.com',
  integrations: [sitemap(), mdx()]
});
