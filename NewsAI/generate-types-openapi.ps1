Write-Host "Generando tipos TypeScript directamente desde Swagger..."

$swaggerUrl = "http://localhost:5033/swagger/v1/swagger.json"
$outputFile = "..\Astro-Web\src\types\api-types.ts"

try {
    # Verificar que el servidor esté en ejecución
    Write-Host "Verificando conexión con el servidor Swagger..."
    try {
        $response = Invoke-WebRequest -Uri $swaggerUrl -UseBasicParsing -ErrorAction Stop
        Write-Host " Conexión exitosa con el servidor Swagger"
    }
    catch {
        Write-Host " Error: No se puede conectar con el servidor Swagger en $swaggerUrl"
        Write-Host "   Asegúrate de que el backend esté en ejecución antes de ejecutar este script."
        Write-Host "   Error detallado: $($_.Exception.Message)"
        exit 1
    }
    
    $swaggerJson = $response.Content
    
    # Guardar el JSON de Swagger en un archivo temporal
    $tempFile = "temp-swagger.json"
    Write-Host "Guardando JSON de Swagger en archivo temporal..."
    
    # Guardar el JSON directamente sin convertirlo
    try {
        Set-Content -Path $tempFile -Value $swaggerJson -ErrorAction Stop
        Write-Host " JSON de Swagger guardado correctamente"
    }
    catch {
        Write-Host " Error al guardar el JSON de Swagger: $($_.Exception.Message)"
        exit 1
    }
    
    # Crear directorio de destino si no existe
    $outputDir = "..\Astro-Web\src\types"
    if (-not (Test-Path $outputDir)) {
        Write-Host "Creando directorio de destino $outputDir..."
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    }
    
    # Instalar openapi-typescript si no está instalado
    Write-Host "Cambiando al directorio Astro-Web..."
    Push-Location "..\Astro-Web"
    
    # Verificar si npm está instalado
    try {
        npm --version | Out-Null
        Write-Host " npm está instalado"
    } catch {
        Write-Host " Error: npm no está instalado o no está en el PATH"
        exit 1
    }
    
    # Instalar openapi-typescript localmente
    Write-Host "Instalando openapi-typescript en el proyecto Astro..."
    npm install --save-dev openapi-typescript
    
    # Generar los tipos TypeScript usando npx para asegurar que se use la versión local
    Write-Host "Generando tipos TypeScript..."
    $fullTempPath = (Resolve-Path "..\NewsAI\$tempFile").Path
    
    # Ejecutar openapi-typescript con manejo de errores explícito
    Write-Host "Ejecutando: npx openapi-typescript '$fullTempPath' --output 'src\types\api-types.ts'"
    
    # Usar una forma más directa de ejecutar el comando
    $env:NODE_OPTIONS = "--no-warnings"
    $process = Start-Process -FilePath "npx" -ArgumentList "openapi-typescript", "$fullTempPath", "--output", "src\types\api-types.ts" -NoNewWindow -Wait -PassThru -RedirectStandardError "openapi-error.log" -RedirectStandardOutput "openapi-output.log"
    
    if ($process.ExitCode -ne 0) {
        Write-Host " Error al ejecutar openapi-typescript. Código de salida: $($process.ExitCode)"
        if (Test-Path "openapi-error.log") {
            Write-Host "Log de errores:"
            Get-Content "openapi-error.log"
        }
        Pop-Location
        exit 1
    }
    
    # Volver al directorio original
    Pop-Location
    
    # Limpiar
    if (Test-Path $tempFile) {
        Write-Host "Eliminando archivo temporal..."
        Remove-Item $tempFile -Force
    }
    
    # Verificar que el archivo generado es válido
    if (Test-Path "..\Astro-Web\src\types\api-types.ts") {
        $content = Get-Content "..\Astro-Web\src\types\api-types.ts" -Raw
        if ($content -match "\[object Object\]") {
            Write-Host " Error: El archivo generado contiene '[object Object]', lo que indica un problema en la generación"
            exit 1
        }
        Write-Host " Tipos TypeScript generados correctamente en $outputFile"
    }
    else {
        Write-Host " Error: No se pudo generar el archivo de tipos"
        exit 1
    }
}
catch {
    Write-Host " Error inesperado: $($_.Exception.Message)"
    Write-Host "Línea: $($_.InvocationInfo.ScriptLineNumber)"
    Write-Host "Comando: $($_.InvocationInfo.Line)"
    exit 1
}