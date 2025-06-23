import { c as createComponent, r as renderComponent, a as renderTemplate, m as maybeRenderHead, e as renderScript } from '../chunks/astro/server_MJG1I7oa.mjs';
import 'kleur/colors';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Bnmyb4hm.mjs';
export { renderers } from '../renderers.mjs';

const prerender = false;
const $$Login = createComponent(async ($$result, $$props, $$slots) => {
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "Login - NewsAI" }, { "default": async ($$result2) => renderTemplate` ${maybeRenderHead()}<section class="min-h-screen flex items-center justify-center bg-gray-50 px-6"> <div class="max-w-md w-full"> <!-- Header --> <div class="text-center mb-8"> <h1 class="text-3xl font-bold text-gray-800 mb-2">NewsAI</h1> <p class="text-gray-600">Inicia sesión para continuar</p> </div> <!-- Formulario de Login --> <div class="bg-white border border-gray-200 shadow-lg rounded-lg p-8"> <!-- Mensajes de estado --> <div id="mensaje-estado" class="hidden mb-4 p-3 rounded-md"></div> <form id="form-login" class="space-y-6"> <!-- Campo Login --> <div> <label for="login" class="block text-sm font-medium text-gray-700 mb-2">
Usuario
</label> <input type="text" id="login" name="login" placeholder="Introduce tu usuario" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> </div> <!-- Campo Password --> <div> <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
Contraseña
</label> <input type="password" id="password" name="password" placeholder="Introduce tu contraseña" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> </div> <!-- Botón Submit --> <button type="submit" id="btn-login" class="w-full bg-blue-600 text-white py-3 px-4 rounded-md hover:bg-blue-700 focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors font-medium"> <span id="texto-boton">Iniciar Sesión</span> <span id="spinner-boton" class="hidden"> <svg class="animate-spin inline w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24"> <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle> <path class="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path> </svg>
Iniciando...
</span> </button> </form> <!-- Links adicionales --> <div class="mt-6 text-center"> <p class="text-sm text-gray-600">
¿No tienes cuenta?
<a href="/registro" class="text-blue-600 hover:text-blue-700 font-medium">
Regístrate aquí
</a> </p> </div> </div> <!-- Info adicional --> <div class="mt-6 text-center"> <p class="text-xs text-gray-500">
NewsAI
</p> </div> </div> </section> ${renderScript($$result2, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/login.astro?astro&type=script&index=0&lang.ts")} ` })}`;
}, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/login.astro", void 0);

const $$file = "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/login.astro";
const $$url = "/login";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Login,
  file: $$file,
  prerender,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
