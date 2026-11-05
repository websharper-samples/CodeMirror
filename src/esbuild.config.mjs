import { cpSync, readdirSync, existsSync, renameSync, rmSync } from 'fs'
import { build } from 'esbuild'

// WebSharper writes the client modules to ./build in Release, and straight to
// wwwroot/Scripts in Debug. In Release we copy the build output over first.
const release = process.argv.includes('--release')
if (release) {
  cpSync('./build/', './wwwroot/Scripts/', { recursive: true })
}

const srcDir = release ? './build/' : './wwwroot/Scripts/'

// The page loads <Name>.min.js, so bundle each client entry into that file.
// A temporary file is used so esbuild never reads and writes the same path.
for (const file of readdirSync(srcDir)) {
  if (!file.endsWith('.js')) continue
  if (file.endsWith('.min.js') || file.includes('.head.')) continue
  if (file.startsWith('WebSharper.Core')) continue

  const out = './wwwroot/Scripts/' + file.replace(/\.js$/, '.min.js')
  const tmp = out + '.tmp.js'

  await build({
    entryPoints: [srcDir + file],
    bundle: true,
    minify: true,
    format: 'iife',
    outfile: tmp,
    globalName: 'wsbundle'
  })

  if (existsSync(out)) rmSync(out)
  renameSync(tmp, out)
  console.log('Bundled:', file)
}
