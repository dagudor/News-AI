Write-Host "🚀 Iniciando backend C#..."
$backend = Start-Process "powershell" -ArgumentList "-NoExit", "-Command", "dotnet run --project .\NewsAI.csproj" -WindowStyle Normal -PassThru

Write-Host "⏳ Esperando a que el backend esté listo..."
Write-Host "⚠️ Una vez que veas que el servidor está listo, ejecuta manualmente .\generate-types-openapi.ps1"

Write-Host "🌐 Iniciando frontend Astro..."
$frontend = Start-Process "cmd.exe" -ArgumentList "/c npm run dev" -WorkingDirectory "..\Astro-Web" -WindowStyle Normal -PassThru

Write-Host "`n Ambos procesos están corriendo. Cierra la ventana del backend para detener todo."

# Esperar a que el backend termine
$backend.WaitForExit()

Write-Host "`n🛑 Backend detenido. Cerrando frontend Astro..."
Stop-Process -Id $frontend.Id -Force

Write-Host " Todo apagado correctamente."
