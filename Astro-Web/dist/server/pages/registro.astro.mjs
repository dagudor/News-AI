import { c as createComponent, r as renderComponent, a as renderTemplate, m as maybeRenderHead, e as renderScript } from '../chunks/astro/server_MJG1I7oa.mjs';
import 'kleur/colors';
import { $ as $$BaseLayout } from '../chunks/BaseLayout_Bnmyb4hm.mjs';
export { renderers } from '../renderers.mjs';

const prerender = false;
const $$Registro = createComponent(async ($$result, $$props, $$slots) => {
  return renderTemplate`${renderComponent($$result, "BaseLayout", $$BaseLayout, { "title": "Registro - NewsAI" }, { "default": async ($$result2) => renderTemplate` ${maybeRenderHead()}<section class="min-h-screen flex items-center justify-center bg-gray-50 px-6"> <div class="max-w-md w-full"> <!-- Header --> <div class="text-center mb-8"> <h1 class="text-3xl font-bold text-gray-800 mb-2">Únete a NewsAI</h1> <p class="text-gray-600">Crea tu cuenta y empieza a recibir resúmenes personalizados</p> </div> <!-- Formulario de Registro --> <div class="bg-white border border-gray-200 shadow-lg rounded-lg p-8"> <!-- Mensajes de estado --> <div id="mensaje-estado" class="hidden mb-4 p-3 rounded-md"></div> <form id="form-registro" class="space-y-6"> <!-- Campo Nombre --> <div> <label for="nombre" class="block text-sm font-medium text-gray-700 mb-2">
Nombre completo
</label> <input type="text" id="nombre" name="nombre" placeholder="Tu nombre completo" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> </div> <!-- Campo Email --> <div> <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
Correo electrónico
</label> <input type="email" id="email" name="email" placeholder="tu@email.com" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> </div> <!-- Campo Usuario --> <div> <label for="login" class="block text-sm font-medium text-gray-700 mb-2">
Nombre de usuario
</label> <input type="text" id="login" name="login" placeholder="usuario123" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> <p class="text-xs text-gray-500 mt-1">Mínimo 3 caracteres</p> </div> <!-- Campo Password --> <div> <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
Contraseña
</label> <input type="password" id="password" name="password" placeholder="Mínimo 6 caracteres" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> </div> <!-- Campo Confirmar Password --> <div> <label for="password-confirm" class="block text-sm font-medium text-gray-700 mb-2">
Confirmar contraseña
</label> <input type="password" id="password-confirm" name="password-confirm" placeholder="Repite tu contraseña" class="w-full p-3 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" required> </div> <!-- Términos y condiciones --> <div class="flex items-start"> <input type="checkbox" id="terminos" name="terminos" class="w-4 h-4 text-blue-600 bg-white border-gray-300 rounded focus:ring-blue-500 focus:ring-2 mt-1" required> <label for="terminos" class="ml-2 text-sm text-gray-600">
Acepto los <a href="#" class="text-blue-600 hover:text-blue-700">términos y condiciones</a>
y la <a href="#" class="text-blue-600 hover:text-blue-700">política de privacidad</a> </label> </div> <!-- Botón Submit --> <button type="submit" id="btn-registro" class="w-full bg-blue-600 text-white py-3 px-4 rounded-md hover:bg-blue-700 focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors font-medium"> <span id="texto-boton">Crear Cuenta</span> <span id="spinner-boton" class="hidden"> <svg class="animate-spin inline w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24"> <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle> <path class="opacity-75" fill="currentColor" d="m4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path> </svg>
Creando cuenta...
</span> </button> </form> <!-- Link al login --> <div class="mt-6 text-center"> <p class="text-sm text-gray-600">
¿Ya tienes cuenta?
<a href="/login" class="text-blue-600 hover:text-blue-700 font-medium">
Inicia sesión aquí
</a> </p> </div> </div> <!-- Info adicional --> <div class="mt-6 text-center"> <p class="text-xs text-gray-500">
Al registrarte, podrás crear configuraciones personalizadas y recibir resúmenes automáticos
</p> </div> </div> </section> ${renderScript($$result2, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/registro.astro?astro&type=script&index=0&lang.ts")} ` })}`;
}, "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/registro.astro", void 0);

const $$file = "C:/Users/Daniel/Desktop/News AI/Astro-Web/src/pages/registro.astro";
const $$url = "/registro";

const _page = /*#__PURE__*/Object.freeze(/*#__PURE__*/Object.defineProperty({
  __proto__: null,
  default: $$Registro,
  file: $$file,
  prerender,
  url: $$url
}, Symbol.toStringTag, { value: 'Module' }));

const page = () => _page;

export { page };
