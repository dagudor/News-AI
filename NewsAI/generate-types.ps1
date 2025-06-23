Write-Host "Generando tipos TypeScript desde la API..."

# Verificar que el servidor esté en ejecución
$swaggerUrl = "https://localhost:7256/swagger/v1/swagger.json"
try {
    $response = Invoke-WebRequest -Uri $swaggerUrl -UseBasicParsing -ErrorAction Stop
    Write-Host " API en ejecución, Swagger disponible."
}
catch {
    Write-Host " Error: No se puede acceder a $swaggerUrl. Asegúrate de que el backend esté en ejecución."
    Write-Host "Error: $($_.Exception.Message)"
    exit 1
}

# Instalar NSwag.CLI si no está instalado
if (-not (Get-Command nswag -ErrorAction SilentlyContinue)) {
    Write-Host "Instalando NSwag.CLI..."
    dotnet tool install -g NSwag.CLI
}

# Crear el directorio de destino si no existe
$outputDir = "..\Astro-Web\src\types"
if (-not (Test-Path $outputDir)) {
    Write-Host "Creando directorio $outputDir..."
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Generar la documentación Swagger y los tipos TypeScript
try {
    nswag run nswag-config.json
    
    if (Test-Path "..\Astro-Web\src\types\api-client.ts") {
        Write-Host " Tipos TypeScript generados correctamente en ..\Astro-Web\src\types\api-client.ts"
    } else {
        Write-Host " Error: No se pudo generar el archivo api-client.ts"
        exit 1
    }
}
catch {
    Write-Host " Error ejecutando NSwag: $($_.Exception.Message)"
    exit 1
}