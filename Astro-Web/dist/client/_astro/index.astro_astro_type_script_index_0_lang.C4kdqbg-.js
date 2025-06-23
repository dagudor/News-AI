let a=[],o=0;const r=3;let l=null;const p="https://localhost:7298";document.addEventListener("DOMContentLoaded",function(){setTimeout(()=>{if(window.requireAuth()){if(l=window.getCurrentUserId(),!l){console.error(" No se pudo obtener el ID del usuario autenticado"),setTimeout(()=>{l=window.getCurrentUserId(),l?(console.log(" Usuario autenticado (segundo intento), ID:",l),u()):(console.error(" Aún no se puede obtener el ID del usuario, redirigiendo..."),window.location.href="/login")},1e3);return}console.log(" Usuario autenticado, ID:",l),console.log("🚀 Inicializando carrusel de resúmenes"),u()}},500)});async function u(){try{console.log("🔍 Cargando resúmenes recientes...");const t=await(await fetch(`${p}/api/simulador/historial/${l}?limite=30`)).json();t.success&&t.data&&t.data.length>0?(a=t.data,console.log(`${a.length} resúmenes cargados`),a.length<=3?L():D()):(console.log("ℹ️ No hay resúmenes disponibles"),g())}catch(e){console.error(" Error cargando resúmenes:",e),g()}finally{document.getElementById("loading-carrusel").classList.add("hidden")}}function L(){console.log("📱 Mostrando carrusel estático"),document.getElementById("carrusel-wrapper").classList.remove("hidden"),document.getElementById("prev-btn").style.display="none",document.getElementById("next-btn").style.display="none",document.getElementById("carrusel-dots").style.display="none",x(a);const e=document.getElementById("carrusel-track");e.style.display="grid",e.style.gridTemplateColumns=`repeat(${a.length}, 1fr)`,e.style.gap="1.5rem",e.style.transform="none"}function D(){console.log("🎠 Mostrando carrusel deslizante"),document.getElementById("carrusel-wrapper").classList.remove("hidden"),document.getElementById("prev-btn").style.display="block",document.getElementById("next-btn").style.display="block",document.getElementById("carrusel-dots").style.display="flex",M(),o=0,i(),P()}function i(){const e=a.slice(o,o+r);x(e),k(),z(),console.log(`Mostrando cards ${o+1}-${o+e.length} de ${a.length}`)}function M(){document.getElementById("prev-btn").onclick=()=>f(),document.getElementById("next-btn").onclick=()=>h()}function f(){o>0&&(o--,i(),m("left"))}function h(){const e=a.length-r;o<e&&(o++,i(),m("right"))}function m(e){const t=document.getElementById("carrusel-track");t.classList.add("transitioning"),e==="right"?t.style.transform="translateX(-10px)":t.style.transform="translateX(10px)",setTimeout(()=>{t.style.transform="translateX(0)",t.classList.remove("transitioning")},150)}function k(){const e=document.getElementById("prev-btn"),t=document.getElementById("next-btn");o===0?(e.style.opacity="0.4",e.style.cursor="not-allowed"):(e.style.opacity="1",e.style.cursor="pointer");const n=a.length-r;o>=n?(t.style.opacity="0.4",t.style.cursor="not-allowed"):(t.style.opacity="1",t.style.cursor="pointer")}function P(){const e=document.getElementById("carrusel-dots");e.innerHTML="";const t=Math.max(0,a.length-r+1);for(let n=0;n<t;n++){const s=document.createElement("button");s.className=`w-2 h-2 rounded-full transition-all mx-1 ${n===o?"bg-blue-600 w-6":"bg-gray-300 hover:bg-gray-400"}`,s.onclick=()=>T(n),e.appendChild(s)}}function T(e){const t=a.length-r;o=Math.max(0,Math.min(e,t)),i(),m("right")}function z(){document.querySelectorAll("#carrusel-dots button").forEach((t,n)=>{n===o?t.className="w-6 h-2 rounded-full transition-all mx-1 bg-blue-600":t.className="w-2 h-2 rounded-full transition-all mx-1 bg-gray-300 hover:bg-gray-400"})}function x(e){const t=document.getElementById("carrusel-track");t.innerHTML="",t.style.display="grid",t.style.gridTemplateColumns="repeat(3, 1fr)",t.style.gap="1.5rem",t.style.transform="none",e.forEach((n,s)=>{const d=document.createElement("div");d.className="card-slide-in relative";const b=n.emailEnviado?"bg-green-100 text-green-800":"bg-yellow-100 text-yellow-800",E=n.emailEnviado?"✓ Enviado":"⏳ Pendiente",I=n.titulo||"Resumen sin título",w=n.extracto||n.contenidoResumen?.substring(0,150)+"..."||"Sin contenido disponible",v=n.noticiasProcessadas||0,B=n.estadisticas?.palabrasResumen||0,C=n.configuracion?.tono||"Por determinar",c=n.configuracion?.hashtags||"";d.innerHTML=`
        <div class="bg-white rounded-xl shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 cursor-pointer h-full"
             onclick="abrirModalDetalle(${n.id})">
          
          <!-- Indicador de posición -->
          <div class="absolute top-2 left-2 bg-blue-600 text-white text-xs px-2 py-1 rounded-full z-10">
            ${o+s+1}/${a.length}
          </div>
          
          <!-- Header de la Card -->
          <div class="p-6 border-b border-gray-100">
            <div class="flex justify-between items-start mb-3">
              <span class="px-3 py-1 ${b} rounded-full text-xs font-medium">
                ${E}
              </span>
              <span class="text-xs text-gray-500">${n.fechaRelativa}</span>
            </div>
            
            <h3 class="font-bold text-gray-800 text-lg mb-2 line-clamp-2">
              ${I}
            </h3>
            
            <p class="text-gray-600 text-sm line-clamp-3 leading-relaxed">
              ${w}
            </p>
          </div>

          <!-- Metadatos -->
          <div class="p-4 bg-gray-50">
            <div class="flex justify-between items-center text-xs text-gray-500">
              <span>${v} noticias</span>
              <span>${B} palabras</span>
              <span>${C}</span>
            </div>
          </div>

          <!-- Footer con hashtags -->
          <div class="p-4">
            <div class="flex flex-wrap gap-1">
              ${c&&c!=="Por determinar"?c.split(",").slice(0,3).map($=>`<span class="px-2 py-1 bg-blue-100 text-blue-800 rounded text-xs">${$.trim()}</span>`).join(""):'<span class="text-xs text-gray-400">Sin hashtags</span>'}
            </div>
          </div>
        </div>
      `,t.appendChild(d)})}function g(){document.getElementById("empty-carrusel").classList.remove("hidden")}window.abrirModalDetalle=async function(e){try{console.log("🔍 Abriendo detalle del resumen:",e),document.getElementById("modal-detalle").classList.remove("hidden"),document.getElementById("modal-contenido").innerHTML="Cargando contenido...";const t=a.find(n=>n.id===e);if(t)y(t);else{const s=await(await fetch(`${p}/api/simulador/resumen/${e}`)).json();if(s.success)y(s.data);else throw new Error("No se pudo cargar el resumen")}}catch(t){console.error(" Error cargando detalle:",t),document.getElementById("modal-contenido").innerHTML="Error al cargar el contenido"}};function y(e){document.getElementById("modal-titulo").textContent=`Resumen del ${new Date(e.fechaGeneracion||Date.now()).toLocaleDateString()}`,document.getElementById("modal-fecha").textContent=e.fechaFormateada||new Date(e.fechaGeneracion||Date.now()).toLocaleDateString(),document.getElementById("modal-noticias").textContent=e.noticiasProcessadas||0,document.getElementById("modal-tiempo-lectura").textContent=Math.ceil((e.contenidoCompleto?.split(" ").length||0)/200)+" min",document.getElementById("modal-email-estado").textContent=e.emailEnviado?"":"⏳",document.getElementById("modal-config-hashtags").textContent=e.configuracion?.hashtags||"Por determinar",document.getElementById("modal-config-tono").textContent=e.configuracion?.tono||"Por determinar",document.getElementById("modal-config-profundidad").textContent=e.configuracion?.profundidad||"Por determinar",document.getElementById("modal-config-accion").textContent=e.configuracion?.accion||"Por determinar",document.getElementById("modal-contenido").innerHTML=(e.contenidoCompleto||e.contenidoResumen||"No hay contenido disponible").replace(/\n/g,"<br>");const t=document.getElementById("modal-url-origen");t.href=e.urlOrigen||"#",t.textContent=e.urlOrigen||"No disponible"}window.cerrarModalDetalle=function(){document.getElementById("modal-detalle").classList.add("hidden")};document.addEventListener("keydown",function(e){a.length>3&&(e.key==="ArrowLeft"?f():e.key==="ArrowRight"&&h()),e.key==="Escape"&&window.cerrarModalDetalle()});
