import { c as createComponent, r as renderComponent, a as renderTemplate, m as maybeRenderHead, e as renderScript } from '../chunks/astro/server_MJG1I7oa.mjs';
import 'kleur/colors';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Bnmyb4hm.mjs';
/* empty css                                 */
export { renderers } from '../renderers.mjs';

const prerender = false;
const $$Index = createComponent(async ($$result, $$props, $$slots) => {
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "Noticias IA - Inicio", "data-astro-cid-j7pv25f6": true }, { "default": async ($$result2) => renderTemplate` ${maybeRenderHead()}<section class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100" data-astro-cid-j7pv25f6> <!-- Hero Section --> <div class="container mx-auto px-6 py-12" data-astro-cid-j7pv25f6> <div class="text-center mb-12" data-astro-cid-j7pv25f6> <h1 class="text-4xl md:text-6xl font-bold text-gray-800 mb-4" data-astro-cid-j7pv25f6>
Bienvenido a <span class="text-blue-600" data-astro-cid-j7pv25f6>NewsAI</span> </h1> <p class="text-xl text-gray-600 max-w-2xl mx-auto" data-astro-cid-j7pv25f6>
Tu asistente inteligente para mantenerte informado con resúmenes
          personalizados de noticias
</p> </div> <!-- Sección de Resúmenes Recientes --> <div class="mb-16" data-astro-cid-j7pv25f6> <div class="flex justify-between items-center mb-8" data-astro-cid-j7pv25f6> <h2 class="text-3xl font-bold text-gray-800" data-astro-cid-j7pv25f6>
📰 Tus Resúmenes Recientes
</h2> <a href="/historial" class="text-blue-600 hover:text-blue-800 font-medium transition-colors" data-astro-cid-j7pv25f6>
Ver todos →
</a> </div> <!-- Carrusel Container --> <div id="carrusel-container" class="relative" data-astro-cid-j7pv25f6> <!-- Loading State --> <div id="loading-carrusel" class="flex justify-center items-center h-64" data-astro-cid-j7pv25f6> <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600" data-astro-cid-j7pv25f6></div> <span class="ml-3 text-gray-600" data-astro-cid-j7pv25f6>Cargando resúmenes...</span> </div> <!-- Empty State --> <div id="empty-carrusel" class="hidden text-center py-16" data-astro-cid-j7pv25f6> <div class="text-6xl mb-4" data-astro-cid-j7pv25f6>📝</div> <h3 class="text-xl font-semibold text-gray-700 mb-2" data-astro-cid-j7pv25f6>
¡Aún no tienes resúmenes!
</h3> <p class="text-gray-600 mb-6" data-astro-cid-j7pv25f6>
Crea tu primera configuración y prueba el simulador
</p> <a href="/simulador" class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-3 rounded-lg font-medium transition" data-astro-cid-j7pv25f6>
Probar Simulador
</a> </div> <!-- Carrusel de Cards --> <div id="carrusel-wrapper" class="hidden" data-astro-cid-j7pv25f6> <!-- Navegación Izquierda --> <button id="prev-btn" class="absolute left-0 top-1/2 transform -translate-y-1/2 z-10 bg-white hover:bg-gray-50 rounded-full shadow-lg p-3 transition-all hover:scale-110" data-astro-cid-j7pv25f6> <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" data-astro-cid-j7pv25f6> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" data-astro-cid-j7pv25f6></path> </svg> </button> <!-- Container de Cards --> <div id="carrusel-track" class="flex transition-transform duration-500 ease-in-out mx-12" data-astro-cid-j7pv25f6> <!-- Las cards se generarán dinámicamente aquí --> </div> <!-- Navegación Derecha --> <button id="next-btn" class="absolute right-0 top-1/2 transform -translate-y-1/2 z-10 bg-white hover:bg-gray-50 rounded-full shadow-lg p-3 transition-all hover:scale-110" data-astro-cid-j7pv25f6> <svg class="w-6 h-6 text-gray-600" fill="none" stroke="currentColor" viewBox="0 0 24 24" data-astro-cid-j7pv25f6> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" data-astro-cid-j7pv25f6></path> </svg> </button> <!-- Indicadores (dots) --> <div id="carrusel-dots" class="flex justify-center mt-6 space-x-2" data-astro-cid-j7pv25f6> <!-- Los dots se generarán dinámicamente --> </div> </div> </div> </div> <!-- Sección de Acciones Rápidas --> <div class="grid md:grid-cols-3 gap-8 mb-16" data-astro-cid-j7pv25f6> <!-- Card Simulador --> <div class="bg-white rounded-xl shadow-lg p-8 hover:shadow-xl transition-shadow" data-astro-cid-j7pv25f6> <div class="text-4xl mb-4" data-astro-cid-j7pv25f6>🚀</div> <h3 class="text-xl font-bold text-gray-800 mb-2" data-astro-cid-j7pv25f6>Probar Simulador</h3> <p class="text-gray-600 mb-4" data-astro-cid-j7pv25f6>
Genera un resumen personalizado desde cualquier URL de noticias
</p> <a href="/simulador" class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg font-medium transition block text-center" data-astro-cid-j7pv25f6>
Comenzar
</a> </div> <!-- Card Configuración --> <div class="bg-white rounded-xl shadow-lg p-8 hover:shadow-xl transition-shadow" data-astro-cid-j7pv25f6> <div class="text-4xl mb-4" data-astro-cid-j7pv25f6>⚙️</div> <h3 class="text-xl font-bold text-gray-800 mb-2" data-astro-cid-j7pv25f6>
Mis Configuraciones
</h3> <p class="text-gray-600 mb-4" data-astro-cid-j7pv25f6>
Crea y gestiona tus configuraciones de resumen personalizadas
</p> <a href="/configuracion?tab=configuracion" class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg font-medium transition block text-center" data-astro-cid-j7pv25f6>
Gestionar
</a> </div> <!-- Card URLs --> <div class="bg-white rounded-xl shadow-lg p-8 hover:shadow-xl transition-shadow" data-astro-cid-j7pv25f6> <div class="text-4xl mb-4" data-astro-cid-j7pv25f6>🔗</div> <h3 class="text-xl font-bold text-gray-800 mb-2" data-astro-cid-j7pv25f6>URLs Confiables</h3> <p class="text-gray-600 mb-4" data-astro-cid-j7pv25f6>
Administra tus fuentes de noticias de confianza
</p> <a href="/configuracion?tab=urls" class="bg-purple-600 hover:bg-purple-700 text-white px-4 py-2 rounded-lg font-medium transition block text-center" data-astro-cid-j7pv25f6>
Ver URLs
</a> </div> </div> </div> <!-- Modal para Detalle de Resumen --> <div id="modal-detalle" class="fixed inset-0 bg-black bg-opacity-50 hidden z-50" onclick="cerrarModalDetalle()" data-astro-cid-j7pv25f6> <div class="flex items-center justify-center min-h-screen p-4" data-astro-cid-j7pv25f6> <div class="bg-white rounded-xl shadow-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto" onclick="event.stopPropagation()" data-astro-cid-j7pv25f6> <!-- Header del Modal --> <div class="sticky top-0 bg-white border-b border-gray-200 p-6 rounded-t-xl" data-astro-cid-j7pv25f6> <div class="flex justify-between items-start" data-astro-cid-j7pv25f6> <div data-astro-cid-j7pv25f6> <h2 id="modal-titulo" class="text-2xl font-bold text-gray-800" data-astro-cid-j7pv25f6>
Resumen Detallado
</h2> <p id="modal-fecha" class="text-gray-600 mt-1" data-astro-cid-j7pv25f6>Cargando...</p> </div> <button onclick="cerrarModalDetalle()" class="text-gray-500 hover:text-gray-700 p-2" data-astro-cid-j7pv25f6> <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" data-astro-cid-j7pv25f6> <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" data-astro-cid-j7pv25f6></path> </svg> </button> </div> </div> <!-- Contenido del Modal --> <div class="p-6" data-astro-cid-j7pv25f6> <!-- Metadatos --> <div class="grid md:grid-cols-3 gap-4 mb-6 p-4 bg-gray-50 rounded-lg" data-astro-cid-j7pv25f6> <div class="text-center" data-astro-cid-j7pv25f6> <div id="modal-noticias" class="text-2xl font-bold text-blue-600" data-astro-cid-j7pv25f6>
-
</div> <div class="text-sm text-gray-600" data-astro-cid-j7pv25f6>Noticias Procesadas</div> </div> <div class="text-center" data-astro-cid-j7pv25f6> <div id="modal-tiempo-lectura" class="text-2xl font-bold text-green-600" data-astro-cid-j7pv25f6>
-
</div> <div class="text-sm text-gray-600" data-astro-cid-j7pv25f6>Tiempo de Lectura</div> </div> <div class="text-center" data-astro-cid-j7pv25f6> <div id="modal-email-estado" class="text-2xl" data-astro-cid-j7pv25f6>📧</div> <div class="text-sm text-gray-600" data-astro-cid-j7pv25f6>Email Enviado</div> </div> </div> <!-- Configuración Usada --> <div class="mb-6 p-4 border border-gray-200 rounded-lg" data-astro-cid-j7pv25f6> <h4 class="font-semibold text-gray-700 mb-3" data-astro-cid-j7pv25f6>
Configuración Utilizada
</h4> <div class="grid md:grid-cols-2 gap-3 text-sm" data-astro-cid-j7pv25f6> <div data-astro-cid-j7pv25f6> <span class="text-gray-600" data-astro-cid-j7pv25f6>Hashtags:</span> <span id="modal-config-hashtags" class="ml-2 font-medium" data-astro-cid-j7pv25f6>-</span> </div> <div data-astro-cid-j7pv25f6> <span class="text-gray-600" data-astro-cid-j7pv25f6>Tono:</span> <span id="modal-config-tono" class="ml-2 font-medium" data-astro-cid-j7pv25f6>-</span> </div> <div data-astro-cid-j7pv25f6> <span class="text-gray-600" data-astro-cid-j7pv25f6>Profundidad:</span> <span id="modal-config-profundidad" class="ml-2 font-medium" data-astro-cid-j7pv25f6>-</span> </div> <div data-astro-cid-j7pv25f6> <span class="text-gray-600" data-astro-cid-j7pv25f6>Acción:</span> <span id="modal-config-accion" class="ml-2 font-medium" data-astro-cid-j7pv25f6>-</span> </div> </div> </div> <!-- Resumen Completo --> <div class="mb-6" data-astro-cid-j7pv25f6> <h4 class="font-semibold text-gray-700 mb-3" data-astro-cid-j7pv25f6>Resumen Completo</h4> <div id="modal-contenido" class="prose prose-gray max-w-none bg-gray-50 p-6 rounded-lg leading-relaxed" data-astro-cid-j7pv25f6>
Cargando contenido...
</div> </div> <!-- URL Original --> <div class="border-t border-gray-200 pt-4" data-astro-cid-j7pv25f6> <h4 class="font-semibold text-gray-700 mb-2" data-astro-cid-j7pv25f6>Fuente Original</h4> <a id="modal-url-origen" href="#" target="_blank" class="text-blue-600 hover:text-blue-800 break-all text-sm" data-astro-cid-j7pv25f6>
Cargando URL...
</a> </div> </div> </div> </div> </div> </section>  ${renderScript($$result2, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/index.astro?astro&type=script&index=0&lang.ts")}   ` })}`;
}, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/index.astro", void 0);

const $$file = "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/index.astro";
const $$url = "";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Index,
  file: $$file,
  prerender,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
