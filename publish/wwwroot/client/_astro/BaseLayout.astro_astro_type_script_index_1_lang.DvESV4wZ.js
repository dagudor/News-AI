class i{constructor(){this.currentUser=null,this.sessionKey="newsai_session",this.userKey="newsai_user",this.init()}init(){this.loadUserFromStorage(),this.setupGlobalAuth()}loadUserFromStorage(){try{const e=localStorage.getItem(this.sessionKey),t=localStorage.getItem(this.userKey);e==="active"&&t&&(this.currentUser=JSON.parse(t),window.currentUser=this.currentUser,console.log("Sesión cargada:",this.currentUser.nombre))}catch(e){console.error("Error cargando sesión:",e),this.clearSession()}}setupGlobalAuth(){window.authManager=this,window.isAuthenticated=()=>this.isAuthenticated(),window.getCurrentUser=()=>this.getCurrentUser(),window.getCurrentUserId=()=>this.getCurrentUserId(),window.logout=()=>this.logout(),window.requireAuth=()=>this.requireAuth(),window.redirectIfAuthenticated=()=>this.redirectIfAuthenticated()}isAuthenticated(){return this.currentUser!==null&&localStorage.getItem(this.sessionKey)==="active"}getCurrentUser(){return this.currentUser}getCurrentUserId(){return this.currentUser?.id||null}saveSession(e){try{this.currentUser=e,window.currentUser=e,localStorage.setItem(this.sessionKey,"active"),localStorage.setItem(this.userKey,JSON.stringify(e)),console.log("Sesión guardada para:",e.nombre)}catch(t){console.error("Error guardando sesión:",t)}}clearSession(){this.currentUser=null,window.currentUser=null,localStorage.removeItem(this.sessionKey),localStorage.removeItem(this.userKey),console.log("Sesión limpiada")}async logout(){try{await fetch("https://localhost:7298/api/auth/logout",{method:"POST",headers:{"Content-Type":"application/json"}})}catch(e){console.error("Error en logout del servidor:",e)}this.clearSession(),window.location.pathname!=="/login"&&(window.location.href="/login")}requireAuth(){return this.isAuthenticated()?!0:(console.log("Acceso denegado - redirigiendo al login"),window.location.href="/login",!1)}redirectIfAuthenticated(){return this.isAuthenticated()?(console.log("Usuario ya autenticado - redirigiendo al home"),window.location.href="/",!0):!1}renderUserInfo(e="user-info"){const t=document.getElementById(e);if(!t)return;const r=document.getElementById("nav-authenticated"),n=document.getElementById("nav-authenticated-mobile");this.isAuthenticated()?(t.innerHTML=`
              <div class="flex items-center space-x-3">
                <div class="flex items-center space-x-2">
                  <div class="w-8 h-8 bg-blue-600 text-white rounded-full flex items-center justify-center text-sm font-medium">
                    ${this.currentUser.nombre.charAt(0).toUpperCase()}
                  </div>
                  <span class="text-sm font-medium text-gray-700">
                    ${this.currentUser.nombre}
                  </span>
                </div>
                <button 
                  onclick="window.authManager.logout()" 
                  class="text-sm text-red-600 hover:text-red-700 font-medium px-2 py-1 rounded hover:bg-red-50"
                >
                  Salir
                </button>
              </div>
            `,r&&(r.classList.remove("hidden"),r.classList.add("flex")),n&&n.classList.remove("hidden")):(t.innerHTML=`
              <a href="/login" class="text-sm text-blue-600 hover:text-blue-700 font-medium px-3 py-2 bg-blue-50 rounded-lg hover:bg-blue-100">
                Iniciar Sesión
              </a>
            `,r&&(r.classList.add("hidden"),r.classList.remove("flex")),n&&n.classList.add("hidden"))}}document.addEventListener("DOMContentLoaded",()=>{console.log("Inicializando AuthManager..."),window.authManager||(window.authManager=new i),window.isAuthenticated=()=>window.authManager.isAuthenticated(),window.getCurrentUser=()=>window.authManager.getCurrentUser(),window.getCurrentUserId=()=>window.authManager.getCurrentUserId(),window.logout=()=>window.authManager.logout(),window.requireAuth=()=>window.authManager.requireAuth(),window.redirectIfAuthenticated=()=>window.authManager.redirectIfAuthenticated(),console.log(" AuthManager inicializado, funciones globales disponibles"),setTimeout(()=>{window.authManager?.renderUserInfo()},100)});window.getCurrentUserId=function(){return window.authManager?.getCurrentUserId()||null};
