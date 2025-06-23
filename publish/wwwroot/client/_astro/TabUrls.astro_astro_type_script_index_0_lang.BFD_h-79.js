let c=null;document.addEventListener("DOMContentLoaded",()=>{console.log("Inicializando TabUrls...");function n(o=5,e=1e3){return new Promise((t,i)=>{let a=0;function r(){if(a++,console.log(`🔍 Intento ${a} de obtener usuario autenticado...`),typeof window.getCurrentUserId!="function")if(console.log("⏳ Funciones de auth aún no disponibles..."),a<o){setTimeout(r,e);return}else{i(new Error("Funciones de autenticación no disponibles"));return}c=window.getCurrentUserId(),c?(console.log(` Usuario autenticado obtenido: ${c}`),t(c)):(console.log("⏳ Usuario aún no disponible..."),a<o?setTimeout(r,e):i(new Error("No se pudo obtener el ID del usuario")))}r()})}n().then(o=>{c=o,console.log("🚀 Inicializando URLs con usuario:",c),new URLSearchParams(window.location.search).get("tab")==="urls"&&(g(),l(),p(),h())}).catch(o=>{console.error(" Error inicializando URLs:",o),window.location.href="/login"})});async function g(){try{const o=await(await fetch(`https://localhost:7298/api/configuracion/obtener/${c}`)).json();if(o.success&&o.data){const e=Array.isArray(o.data)?o.data:[o.data],t=document.getElementById("select-configuracion");t&&(t.innerHTML='<option value="">Seleccionar configuración...</option>',e.forEach(i=>{const a=document.createElement("option");a.value=i.id,a.textContent=`${i.hashtags||"Sin hashtags"} (${i.frecuencia||"diaria"})`,a.dataset.config=JSON.stringify(i),t.appendChild(a)}))}}catch(n){console.error("Error cargando configuraciones:",n)}}async function l(){try{const o=await(await fetch(`https://localhost:7298/api/urlsconfiables/usuario/${c}`)).json();o.success&&o.data?m(o.data):document.getElementById("lista-urls").innerHTML='<div class="text-center text-gray-500">No hay URLs configuradas</div>'}catch(n){console.error("Error cargando URLs:",n),document.getElementById("lista-urls").innerHTML='<div class="text-center text-red-500">Error al cargar URLs</div>'}}function m(n){const o=document.getElementById("lista-urls");if(!o||n.length===0){o.innerHTML='<div class="text-center text-gray-500">No hay URLs configuradas</div>';return}o.innerHTML=n.map(e=>`
      <div class="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <div class="flex justify-between items-start mb-2">
          <div class="flex-1">
            <h3 class="font-medium text-blue-600">${e.nombre||"URL sin nombre"}</h3>
            <p class="text-sm text-gray-600 break-all">${e.url}</p>
            <p class="text-xs text-gray-500 mt-1">
              ${e.tipoFuente||"RSS"} • ${e.activa?"Activa":"Inactiva"}
            </p>
          </div>
          <div class="flex space-x-2">
            <button 
              onclick="editarUrl(${e.id})" 
              class="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded-full border-2 border-blue-300 ml-2"
            >
              Editar
            </button>
            <button 
              onclick="eliminarUrl(${e.id})" 
              class="px-3 py-1 text-sm bg-red-100 text-red-700 rounded-full border-2 border-red-300"
            >
              Eliminar
            </button>
          </div>
        </div>
        
        ${e.configuraciones&&e.configuraciones.length>0?`
          <div class="mt-3 pt-3 border-t border-gray-200">
            <p class="text-xs font-medium text-gray-700 mb-2">Configuraciones asociadas:</p>
            <div class="flex flex-wrap gap-2">
              ${e.configuraciones.map(t=>`
                <span class="inline-flex items-center px-2 py-1 text-xs bg-blue-100 text-blue-800 rounded">
                  ${t.hashtags||"Sin hashtags"}
                  <button 
                    onclick="eliminarAsociacion(${e.id}, ${t.id})"
                    class="ml-1 text-blue-600 hover:text-blue-800"
                    title="Eliminar esta asociación"
                  >
                    ×
                  </button>
                </span>
              `).join("")}
            </div>
          </div>
        `:'<p class="text-xs text-gray-500 mt-2">Sin configuraciones asociadas</p>'}
      </div>
    `).join("")}function p(){const n=document.getElementById("select-configuracion");n&&n.addEventListener("change",function(){const o=this.options[this.selectedIndex];if(o.value&&o.dataset.config)try{const e=JSON.parse(o.dataset.config);document.getElementById("config-hashtags").textContent=e.hashtags||"No especificados",document.getElementById("config-tono").textContent=e.tonoResumen||"No especificado",document.getElementById("config-profundidad").textContent=e.profundidadResumen||"No especificada",document.getElementById("config-accion").textContent=e.accionResumen||"No especificada"}catch(e){console.error("Error parseando configuración:",e)}else document.getElementById("config-hashtags").textContent="No especificados",document.getElementById("config-tono").textContent="No especificado",document.getElementById("config-profundidad").textContent="No especificada",document.getElementById("config-accion").textContent="No especificada"})}function h(){const n=document.getElementById("form-urls-confiables");n&&n.addEventListener("submit",async o=>{o.preventDefault();const e=new FormData(o.target),t=e.get("nueva_url"),i=e.get("configuracion_url");if(!t||t.trim()===""){alert("Por favor, ingresa una URL válida");return}if(!i||i===""){alert("Por favor, selecciona una configuración");return}try{new URL(t)}catch{alert("Por favor, ingresa una URL válida (debe incluir http:// o https://)");return}const a={usuarioId:c,url:t.trim(),configuracionId:parseInt(i)};try{const s=await(await fetch("https://localhost:7298/api/urlsconfiables",{method:"POST",headers:{"Content-Type":"application/json",Accept:"application/json"},body:JSON.stringify(a)})).json();s.success?(alert(" URL añadida correctamente"),o.target.reset(),document.getElementById("config-hashtags").textContent="No especificados",document.getElementById("config-tono").textContent="No especificado",document.getElementById("config-profundidad").textContent="No especificada",document.getElementById("config-accion").textContent="No especificada",l()):alert(" Error: "+(s.message||"Error desconocido"))}catch(r){console.error("Error al guardar URL:",r),alert("Error de conexión. Verifica que el backend esté ejecutándose.")}})}window.eliminarUrl=async n=>{try{const o=await fetch(`https://localhost:7298/api/urlsconfiables/${n}`,{method:"DELETE"}),e=await o.json();if(o.ok)alert(" URL eliminada correctamente"),l();else if(e.tipo==="multiples_configuraciones"){const t=e.configuraciones?.length||0,i=e.configuraciones?.map(r=>`• ${r.hashtags||"Sin hashtags"}`).join(`
`)||"",a=`Esta URL está asociada a ${t} configuración(es):

${i}

Si continúas, se eliminarán TODAS las asociaciones y la URL.

¿Continuar?`;if(confirm(a)){if(e.configuraciones)for(const s of e.configuraciones)try{await fetch(`https://localhost:7298/api/urlsconfiables/${n}/configuracion/${s.id}`,{method:"DELETE"})}catch(f){console.warn(`Error eliminando asociación ${s.id}:`,f)}(await fetch(`https://localhost:7298/api/urlsconfiables/${n}`,{method:"DELETE"})).ok?(alert(" URL y todas sus asociaciones eliminadas correctamente"),l()):alert(" Error al eliminar la URL después de eliminar asociaciones")}}else alert(" Error: "+(e.message||"Error desconocido"))}catch(o){alert("Error de conexión: "+o.message)}};window.editarUrl=async n=>{try{const e=await(await fetch(`https://localhost:7298/api/urlsconfiables/${n}`)).json();if(e.success&&e.data){const t=e.data;window.currentUrlId=n,window.configuracionesActuales=[],document.getElementById("url-id-edit").value=t.id,document.getElementById("url-nombre-edit").value=t.nombre||"",document.getElementById("url-url-edit").value=t.url||"",document.getElementById("url-descripcion-edit").value=t.descripcion||"",document.getElementById("url-activa-edit").checked=t.activa,await w(),await E(n),document.getElementById("modal-editar-url").classList.remove("hidden")}}catch{alert("Error al cargar los datos de la URL")}};async function w(){try{const o=await(await fetch(`https://localhost:7298/api/configuracion/obtener/${c}`)).json();if(o.success&&o.data){const e=Array.isArray(o.data)?o.data:[o.data];window.todasLasConfiguraciones=e,d()}}catch(n){console.error("Error cargando configuraciones para modal:",n)}}function d(){const n=document.getElementById("select-nueva-configuracion");if(!n||!window.todasLasConfiguraciones)return;n.innerHTML='<option value="">Seleccionar configuración para añadir...</option>',window.todasLasConfiguraciones.filter(e=>!window.configuracionesActuales.some(t=>t.id===e.id)).forEach(e=>{const t=document.createElement("option");t.value=e.id,t.textContent=`${e.hashtags||"Sin hashtags"} (${e.frecuencia||"diaria"})`,n.appendChild(t)})}async function E(n){try{const e=await(await fetch(`https://localhost:7298/api/urlsconfiables/usuario/${c}`)).json();if(e.success&&e.data){const t=e.data.find(i=>i.id===n);t&&t.configuraciones?window.configuracionesActuales=t.configuraciones:window.configuracionesActuales=[],u(),d()}}catch(o){console.error("Error cargando configuraciones asociadas:",o),window.configuracionesActuales=[]}}function u(){const n=document.getElementById("configuraciones-tags");if(n){if(window.configuracionesActuales.length===0){n.innerHTML='<span class="text-sm text-gray-500">No hay configuraciones asociadas</span>';return}n.innerHTML=window.configuracionesActuales.map(o=>`
      <span class="inline-flex items-center px-3 py-1 text-sm bg-blue-100 text-blue-800 rounded-full border border-blue-300">
        #${o.hashtags||"Sin hashtags"}
        <button 
          type="button"
          onclick="eliminarConfiguracionTag(${o.id})"
          class="ml-2 text-blue-600 hover:text-blue-800 font-bold"
          title="Eliminar configuración"
        >
          ×
        </button>
      </span>
    `).join("")}}window.añadirConfiguracion=()=>{const n=document.getElementById("select-nueva-configuracion"),o=parseInt(n.value);if(!o){alert("Selecciona una configuración");return}const e=window.todasLasConfiguraciones.find(t=>t.id===o);e&&(window.configuracionesActuales.push(e),u(),d())};window.eliminarConfiguracionTag=n=>{window.configuracionesActuales=window.configuracionesActuales.filter(o=>o.id!==n),u(),d()};window.cerrarModalUrl=()=>{document.getElementById("modal-editar-url").classList.add("hidden")};document.addEventListener("DOMContentLoaded",()=>{const n=document.getElementById("form-editar-url");n&&n.addEventListener("submit",async o=>{o.preventDefault();const e=new FormData(o.target),t=e.get("urlId"),i={nombre:e.get("nombre"),url:e.get("url"),descripcion:e.get("descripcion"),tipoFuente:"RSS",activa:e.has("activa")};try{console.log("Actualizando URL:",t,i);const a=await fetch(`https://localhost:7298/api/urlsconfiables/${t}`,{method:"PUT",headers:{"Content-Type":"application/json",Accept:"application/json"},body:JSON.stringify(i)});if(console.log("Respuesta del servidor:",a.status),!a.ok){const s=await a.text();throw console.error("Error del servidor:",s),new Error(`Error ${a.status}: ${s}`)}const r=await a.json();console.log("Resultado:",r),r.success?(console.log("Gestionando configuraciones asociadas..."),await y(t),alert(" URL actualizada correctamente"),window.cerrarModalUrl(),l()):alert(" Error: "+r.message)}catch(a){console.error("Error completo:",a),alert("Error de conexión: "+a.message+`

Verifica que el backend esté ejecutándose en puerto 7298.`)}})});async function y(n){try{console.log("Iniciando gestión de configuraciones para URL:",n);const e=await(await fetch(`https://localhost:7298/api/urlsconfiables/usuario/${c}`)).json();if(e.success&&e.data){const i=e.data.find(a=>a.id==n)?.configuraciones||[];console.log("Configuraciones en servidor:",i.length),console.log("Configuraciones en UI:",window.configuracionesActuales.length);for(const a of i)if(!window.configuracionesActuales.some(r=>r.id===a.id)){console.log("Eliminando configuración:",a.id);try{await fetch(`https://localhost:7298/api/urlsconfiables/${n}/configuracion/${a.id}`,{method:"DELETE"})}catch(r){console.warn("Error eliminando configuración:",r)}}for(const a of window.configuracionesActuales)if(!i.some(r=>r.id===a.id)){console.log("Añadiendo configuración:",a.id);try{await fetch("https://localhost:7298/api/urlsconfiables",{method:"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify({usuarioId:c,url:document.getElementById("url-url-edit").value,configuracionId:a.id})})}catch(r){console.warn("Error añadiendo configuración:",r)}}console.log("Gestión de configuraciones completada")}}catch(o){console.error("Error gestionando configuraciones asociadas:",o)}}window.eliminarAsociacion=async(n,o)=>{if(confirm("¿Eliminar la asociación con esta configuración?"))try{(await fetch(`https://localhost:7298/api/urlsconfiables/${n}/configuracion/${o}`,{method:"DELETE"})).ok?(alert("Asociación eliminada correctamente"),l()):alert("Error al eliminar asociación")}catch(e){alert("Error de conexión: "+e.message)}};
