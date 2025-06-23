let c=[],r=[],a=1;const g=12;let l=null;const x="https://localhost:7298";document.addEventListener("DOMContentLoaded",function(){setTimeout(()=>{if(window.requireAuth()){if(l=window.getCurrentUserId(),!l){console.error("No se pudo obtener el ID del usuario autenticado"),setTimeout(()=>{l=window.getCurrentUserId(),l?(console.log("Usuario autenticado (segundo intento), ID:",l),h()):(console.error("Aún no se puede obtener el ID del usuario, redirigiendo..."),window.location.href="/login")},1e3);return}console.log("Usuario autenticado, ID:",l),console.log("Inicializando página Ver Todos"),h()}},500)});async function h(){try{console.log("Cargando todos los resúmenes...");const t=await(await fetch(`${x}/api/simulador/historial/${l}?limite=1000`)).json();t.success&&t.data&&t.data.length>0?(c=t.data,r=[...c],console.log(`${c.length} resúmenes cargados`),document.getElementById("total-resumenes").textContent=c.length.toString(),m(),I(),v()):y()}catch(e){console.error("Error cargando resúmenes:",e),y()}finally{document.getElementById("loading-resumenes").classList.add("hidden")}}function m(){const e=document.getElementById("grid-resumenes"),t=(a-1)*g,i=t+g,o=r.slice(t,i);if(o.length===0){e.classList.add("hidden"),document.getElementById("empty-state").classList.remove("hidden"),document.getElementById("paginacion").classList.add("hidden");return}document.getElementById("empty-state").classList.add("hidden"),e.classList.remove("hidden"),e.innerHTML="",o.forEach(n=>{const d=document.createElement("div");d.className="bg-white rounded-xl shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 cursor-pointer";const s=n.emailEnviado?"bg-green-100 text-green-800":"bg-yellow-100 text-yellow-800",u=n.emailEnviado?"✓ Enviado":"⏳ Pendiente";d.innerHTML=`
          <div onclick="abrirModalDetalle(${n.id})">
            <!-- Header -->
            <div class="p-4 border-b border-gray-100">
              <div class="flex justify-between items-start mb-3">
                <span class="px-2 py-1 ${s} rounded-full text-xs font-medium">
                  ${u}
                </span>
                <span class="text-xs text-gray-500">${n.fechaRelativa}</span>
              </div>
              
              <h3 class="font-bold text-gray-800 text-base mb-2 line-clamp-2">
                ${n.titulo}
              </h3>
              
              <p class="text-gray-600 text-sm line-clamp-2 leading-relaxed">
                ${n.extracto}
              </p>
            </div>

            <!-- Metadatos -->
            <div class="p-3 bg-gray-50">
              <div class="flex justify-between items-center text-xs text-gray-500 mb-2">
                <span>${n.noticiasProcessadas} noticias</span>
                <span>${n.estadisticas.palabrasResumen} palabras</span>
              </div>
              <div class="text-xs text-gray-600">
                <strong>Tono:</strong> ${n.configuracion.tono}
              </div>
            </div>

            <!-- Hashtags -->
            <div class="p-3">
              <div class="flex flex-wrap gap-1">
                ${n.configuracion.hashtags&&n.configuracion.hashtags!=="Sin hashtags específicos"?n.configuracion.hashtags.split(",").slice(0,3).map(f=>`<span class="px-2 py-1 bg-blue-100 text-blue-800 rounded text-xs">${f.trim()}</span>`).join(""):'<span class="text-xs text-gray-400">Sin hashtags</span>'}
              </div>
            </div>
          </div>
        `,e.appendChild(d)}),w()}function I(){document.getElementById("filtro-periodo").addEventListener("change",p),document.getElementById("filtro-email").addEventListener("change",p)}function p(){const e=document.getElementById("filtro-periodo").value,t=document.getElementById("filtro-email").value;r=c.filter(i=>{let o=!0;if(e!=="todos"){const d=new Date(i.fechaGeneracion),s=new Date;switch(e){case"hoy":o=d.toDateString()===s.toDateString();break;case"semana":const u=new Date(s.setDate(s.getDate()-7));o=d>=u;break;case"mes":const f=new Date(s.getFullYear(),s.getMonth(),1);o=d>=f;break}}let n=!0;return t!=="todos"&&(n=t==="enviados"?i.emailEnviado:!i.emailEnviado),o&&n}),a=1,m()}function v(){document.getElementById("prev-page").addEventListener("click",()=>{a>1&&(a--,m())}),document.getElementById("next-page").addEventListener("click",()=>{const e=Math.ceil(r.length/g);a<e&&(a++,m())})}function w(){const e=Math.ceil(r.length/g);if(e<=1){document.getElementById("paginacion").classList.add("hidden");return}document.getElementById("paginacion").classList.remove("hidden"),document.getElementById("pagina-actual").textContent=a.toString(),document.getElementById("total-paginas").textContent=e.toString(),document.getElementById("prev-page").disabled=a===1,document.getElementById("next-page").disabled=a===e}window.limpiarFiltros=function(){document.getElementById("filtro-periodo").value="todos",document.getElementById("filtro-email").value="todos",p()};function y(){document.getElementById("empty-state").classList.remove("hidden"),document.getElementById("grid-resumenes").classList.add("hidden")}window.abrirModalDetalle=async function(e){try{console.log("Abriendo detalle del resumen:",e),document.getElementById("modal-detalle").classList.remove("hidden"),document.getElementById("modal-contenido").innerHTML="Cargando contenido...";const t=c.find(i=>i.id===e);if(t)E(t);else{const o=await(await fetch(`${x}/api/simulador/resumen/${e}`)).json();o.success&&E(o.data)}}catch(t){console.error("Error cargando detalle:",t),document.getElementById("modal-contenido").innerHTML="Error al cargar el contenido"}};function E(e){document.getElementById("modal-titulo").textContent=`Resumen del ${new Date(e.fechaGeneracion||Date.now()).toLocaleDateString()}`,document.getElementById("modal-fecha").textContent=e.fechaFormateada||new Date(e.fechaGeneracion||Date.now()).toLocaleDateString(),document.getElementById("modal-noticias").textContent=e.noticiasProcessadas||0,document.getElementById("modal-tiempo-lectura").textContent=Math.ceil((e.contenidoCompleto?.split(" ").length||0)/200)+" min",document.getElementById("modal-email-estado").textContent=e.emailEnviado?"":"⏳",document.getElementById("modal-config-hashtags").textContent=e.configuracion?.hashtags||"Sin hashtags",document.getElementById("modal-config-tono").textContent=e.configuracion?.tono||"Estándar",document.getElementById("modal-config-profundidad").textContent=e.configuracion?.profundidad||"Media",document.getElementById("modal-config-accion").textContent=e.configuracion?.accion||"Email",document.getElementById("modal-contenido").innerHTML=(e.contenidoCompleto||e.contenidoResumen||"No disponible").replace(/\n/g,"<br>");const t=document.getElementById("modal-url-origen");t.href=e.urlOrigen||"#",t.textContent=e.urlOrigen||"No disponible"}window.cerrarModalDetalle=function(){document.getElementById("modal-detalle").classList.add("hidden")};document.addEventListener("keydown",function(e){e.key==="Escape"&&window.cerrarModalDetalle()});
