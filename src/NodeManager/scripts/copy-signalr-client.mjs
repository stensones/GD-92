import { cp, mkdir } from "node:fs/promises";
import { dirname, resolve } from "node:path";

const source = resolve(
    "node_modules",
    "@microsoft",
    "signalr",
    "dist",
    "browser",
    "signalr.min.js");
const destination = resolve("wwwroot", "lib", "signalr", "signalr.min.js");

await mkdir(dirname(destination), { recursive: true });
await cp(source, destination);
