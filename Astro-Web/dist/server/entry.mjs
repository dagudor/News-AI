import { renderers } from './renderers.mjs';
import { c as createExports, s as serverEntrypointModule } from './chunks/_@astrojs-ssr-adapter_8fi7DxoI.mjs';
import { manifest } from './manifest_BstCVjgn.mjs';

const serverIslandMap = new Map();;

const _page0 = () => import('./pages/_image.astro.mjs');
const _page1 = () => import('./pages/404.astro.mjs');
const _page2 = () => import('./pages/configuracion.astro.mjs');
const _page3 = () => import('./pages/historial.astro.mjs');
const _page4 = () => import('./pages/login.astro.mjs');
const _page5 = () => import('./pages/posts/_---slug_.astro.mjs');
const _page6 = () => import('./pages/registro.astro.mjs');
const _page7 = () => import('./pages/rss.xml.astro.mjs');
const _page8 = () => import('./pages/simulador.astro.mjs');
const _page9 = () => import('./pages/tags/_tag_.astro.mjs');
const _page10 = () => import('./pages/tags.astro.mjs');
const _page11 = () => import('./pages/index.astro.mjs');
const pageMap = new Map([
    ["node_modules/astro/dist/assets/endpoint/node.js", _page0],
    ["src/pages/404.astro", _page1],
    ["src/pages/configuracion.astro", _page2],
    ["src/pages/historial.astro", _page3],
    ["src/pages/login.astro", _page4],
    ["src/pages/posts/[...slug].astro", _page5],
    ["src/pages/registro.astro", _page6],
    ["src/pages/rss.xml.js", _page7],
    ["src/pages/simulador.astro", _page8],
    ["src/pages/tags/[tag].astro", _page9],
    ["src/pages/tags/index.astro", _page10],
    ["src/pages/index.astro", _page11]
]);

const _manifest = Object.assign(manifest, {
    pageMap,
    serverIslandMap,
    renderers,
    actions: () => import('./_noop-actions.mjs'),
    middleware: () => import('./_noop-middleware.mjs')
});
const _args = {
    "mode": "standalone",
    "client": "file:///C:/Users/Daniel/Desktop/News%20AI/Astro-Web/dist/client/",
    "server": "file:///C:/Users/Daniel/Desktop/News%20AI/Astro-Web/dist/server/",
    "host": false,
    "port": 4321,
    "assets": "_astro"
};
const _exports = createExports(_manifest, _args);
const handler = _exports['handler'];
const startServer = _exports['startServer'];
const options = _exports['options'];
const _start = 'start';
if (_start in serverEntrypointModule) {
	serverEntrypointModule[_start](_manifest, _args);
}

export { handler, options, pageMap, startServer };
