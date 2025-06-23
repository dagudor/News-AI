import { c as createComponent, a as renderTemplate, m as maybeRenderHead, b as createAstro, r as renderComponent, e as renderScript, j as renderHead, f as renderSlot } from './astro/server_MJG1I7oa.mjs';
import 'kleur/colors';
import 'clsx';
/* empty css                          */

var __freeze = Object.freeze;
var __defProp = Object.defineProperty;
var __template = (cooked, raw) => __freeze(__defProp(cooked, "raw", { value: __freeze(cooked.slice()) }));
var _a;
const $$BaseHead = createComponent(($$result, $$props, $$slots) => {
  return renderTemplate(_a || (_a = __template([`<meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"><!-- Basic Meta Tags --><title>News AI</title><meta name="description" content="A brief description of your website content."><meta name="keywords" content="keyword1, keyword2, keyword3"><meta name="author" content="Your Name or Company Name"><!-- Favicons --><link rel="icon" href="/favicon.ico" sizes="any"><link rel="icon" href="/icon.svg" type="image/svg+xml"><link rel="apple-touch-icon" href="/apple-touch-icon.png"><link rel="manifest" href="/manifest.webmanifest"><!-- Favicon for IE --><link rel="shortcut icon" href="/favicon.ico" type="image/x-icon"><!-- Favicons for different sizes --><link rel="icon" type="image/png" sizes="16x16" href="/favicon-16x16.png"><link rel="icon" type="image/png" sizes="32x32" href="/favicon-32x32.png"><link rel="icon" type="image/png" sizes="48x48" href="/favicon-48x48.png"><!-- Open Graph / Facebook --><meta property="og:type" content="website"><meta property="og:url" content="https://www.yourwebsite.com/"><meta property="og:title" content="Your Website Title"><meta property="og:description" content="A brief description of your website content."><meta property="og:image" content="https://www.yourwebsite.com/path/to/image.jpg"><!-- Twitter --><meta property="twitter:card" content="summary_large_image"><meta property="twitter:url" content="https://www.yourwebsite.com/"><meta property="twitter:title" content="Your Website Title"><meta property="twitter:description" content="A brief description of your website content."><meta property="twitter:image" content="https://www.yourwebsite.com/path/to/image.jpg"><!-- Canonical URL --><link rel="canonical" href="https://www.yourwebsite.com/"><!-- Apple Touch Icon (already included in favicons, but keeping for backwards compatibility) --><link rel="apple-touch-icon" sizes="180x180" href="/apple-touch-icon.png"><!-- Theme Color for Mobile Browsers --><meta name="theme-color" content="#ffffff"><!-- For IE --><meta http-equiv="X-UA-Compatible" content="IE=edge"><!-- HTML in your document's head --><link rel="preconnect" href="https://rsms.me/"><link rel="stylesheet" href="https://rsms.me/inter/inter.css"><link href="https://api.fontshare.com/v2/css?f[]=jet-brains-mono@1,2&display=swap" rel="stylesheet"><link rel="preconnect" href="https://fonts.googleapis.com"><link rel="preconnect" href="https://fonts.gstatic.com" crossorigin><link href="https://fonts.googleapis.com/css2?family=Gilda+Display&display=swap" rel="stylesheet"><script defer src="https://unpkg.com/alpinejs@3.x.x/dist/cdn.min.js"><\/script>`])));
}, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/components/BaseHead.astro", void 0);

const $$Footer = createComponent(($$result, $$props, $$slots) => {
  return renderTemplate`${maybeRenderHead()}<footer class="bg-white border-t border-zinc-200"> <div class="px-8 md:px-12 mx-auto max-w-7xl py-6 lg:px-32"> <section class="gap-y-6 gap-x-8" style="text-align: center;"> <div> <p class="text-sm text-zinc-600 lg:col-span-2" x-data="{ year: new Date().getFullYear() }">
© <span x-text="year"></span> Daniel.
</p> </div> </section> </div> </footer>`;
}, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/components/global/Footer.astro", void 0);

const $$Astro = createAstro("https://yourwebsite.com");
const $$BaseLayout = createComponent(async ($$result, $$props, $$slots) => {
  const Astro2 = $$result.createAstro($$Astro, $$props, $$slots);
  Astro2.self = $$BaseLayout;
  const { title } = Astro2.props;
  return renderTemplate`<html lang="es"> <head>${renderComponent($$result, "BaseHead", $$BaseHead, {})}<title>${title}</title><!-- Sistema de Autenticación -->${renderScript($$result, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/layouts/BaseLayout.astro?astro&type=script&index=0&lang.ts")}${renderHead()}</head> <body class="bg-white flex flex-col min-h-screen"> <!-- Navegación mejorada --> <nav class="bg-white border-b border-gray-200 shadow-sm"> <div class="max-w-7xl mx-auto px-4 py-4 flex items-center justify-between"> <!-- Logo --> <a href="/" class="text-xl font-bold text-blue-600">
NewsAI
</a> <!-- Menú de navegación --> <div class="hidden md:flex items-center space-x-6"> <!-- Enlaces que solo se muestran cuando está logueado --> <div id="nav-authenticated" class="hidden items-center space-x-6"> <a href="/" class="text-gray-600 hover:text-blue-600 font-medium">
Inicio
</a> <a href="/simulador" class="text-gray-600 hover:text-blue-600 font-medium">
Simulador
</a> <a href="/historial" class="text-gray-600 hover:text-blue-600 font-medium">
Historial
</a> <a href="/configuracion" class="text-gray-600 hover:text-blue-600 font-medium">
Configuración
</a> <div class="w-px h-6 bg-gray-300"></div> </div> <!-- Info del usuario (incluye botón login si no está autenticado) --> <div id="user-info"> <!-- Info del usuario se renderiza aquí automáticamente --> </div> </div> <!-- Menú móvil (hamburguesa) --> <div class="md:hidden"> <button id="mobile-menu-button" class="text-gray-600 hover:text-blue-600"> <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path> </svg> </button> </div> </div> <!-- Menú móvil expandido --> <div id="mobile-menu" class="hidden md:hidden"> <div class="px-4 py-4 border-t border-gray-200 space-y-3"> <!-- Enlaces móviles que solo se muestran cuando está logueado --> <div id="nav-authenticated-mobile" class="hidden space-y-3"> <a href="/" class="block text-gray-600 hover:text-blue-600 font-medium">
Inicio
</a> <a href="/simulador" class="block text-gray-600 hover:text-blue-600 font-medium">
Simulador
</a> <a href="/historial" class="block text-gray-600 hover:text-blue-600 font-medium">
Historial
</a> <a href="/configuracion" class="block text-gray-600 hover:text-blue-600 font-medium">
Configuración
</a> <div class="border-t border-gray-200 pt-3"></div> </div> <!-- Info del usuario móvil --> <div id="user-info-mobile"> <!-- Info del usuario móvil se renderiza aquí --> </div> </div> </div> </nav> <!-- Contenido principal --> <main class="flex-grow"> ${renderSlot($$result, $$slots["default"])} </main> <!-- Footer --> ${renderComponent($$result, "Footer", $$Footer, {})} <!-- Scripts adicionales --> ${renderScript($$result, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/layouts/BaseLayout.astro?astro&type=script&index=1&lang.ts")} <!-- Slot para scripts adicionales --> ${renderSlot($$result, $$slots["scripts"])} </body></html>`;
}, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/layouts/BaseLayout.astro", void 0);

export { $$BaseLayout as $ };
