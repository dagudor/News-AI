export {};

declare global {
  interface Window {
    requireAuth: () => boolean;
    getCurrentUserId: () => string | number;
    updateCurrentUserId: (id: string | number) => void;
    toggleConfiguracion: (id: string | number) => void;
    abrirModalEditar: (id: string | number) => Promise<void>;;
    getCurrentUserIdForLabs: () => string | number;
    getCurrentUserIdForTabs: () => string | number;
    abrirModalDetalle: (resumenId: string) => Promise<void>;
    eliminarUrl: (id: string | number) =>  void;
    editarUrl: (id: string | number) =>  void;
    cerrarModalDetalle: () => void;
    cerrarModalConfig: () => void;
    eliminarConfiguracionTag: (id: string | number) => void;
    limpiarFiltros: () => void;
    cerrarModalUrl: () => void;
    eliminarAsociacion: (urlId: string | number, configId: string | number) => void;
    manejarCambioFrecuenciaModal: (frecuencia: string) => void;
    authManager: {
      redirectIfAuthenticated: () => boolean;
      // Puedes añadir más métodos si existen
    };
    currentUser: any;
    mostrarConfiguracionSeleccionada: (id: string | number) => void;
    reiniciarSimulacion: () => void;
  }
}