using System.ComponentModel.DataAnnotations;

namespace NewsAI.Dominio.Entidades;

public class Configuracion
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Hashtags { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string GradoDesarrolloResumen { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Lenguaje { get; set; } = string.Empty;

    // ⭐ CAMPOS EXISTENTES
    public bool Email { get; set; }
    public bool Audio { get; set; } = false;

    // ⭐ NUEVOS CAMPOS PARA SCHEDULING AUTOMÁTICO
    public string Frecuencia { get; set; } = "diaria"; // diaria, semanal, personalizada, desactivada
    public TimeSpan HoraEnvio { get; set; } = new TimeSpan(8, 0, 0); // 08:00:00 por defecto
    public string DiasPersonalizados { get; set; } = ""; // "1,3,5" para Lun,Mie,Vie
    public DateTime? ProximaEjecucion { get; set; } // Calculado automáticamente
    public DateTime? UltimaEjecucion { get; set; }
    public bool SchedulingActivo { get; set; } = true;

    // ⭐ CAMPOS EXISTENTES  
    public bool Activa { get; set; } = true;
    public int UsuarioId { get; set; }
    public virtual Usuario? Usuario { get; set; }
    public virtual ICollection<ConfiguracionUrl>? ConfiguracionUrls { get; set; }

    // ⭐ MÉTODO HELPER PARA CALCULAR PRÓXIMA EJECUCIÓN
    public DateTime? CalcularProximaEjecucion()
{
    //  USAR HORA LOCAL ESPAÑOLA - NO UTC
    var ahora = DateTime.Now; // ← CAMBIO PRINCIPAL
    var hoy = ahora.Date;
    var horaHoy = hoy.Add(HoraEnvio);

    switch (Frecuencia.ToLower())
    {
        case "diaria":
            return ahora >= horaHoy ? horaHoy.AddDays(1) : horaHoy;

        case "semanal":
            var proximoLunes = hoy.AddDays(((int)DayOfWeek.Monday - (int)hoy.DayOfWeek + 7) % 7);
            var horaProximoLunes = proximoLunes.Add(HoraEnvio);
            return ahora >= horaProximoLunes ? horaProximoLunes.AddDays(7) : horaProximoLunes;

        case "personalizada":
            if (string.IsNullOrEmpty(DiasPersonalizados))
                return DateTime.Now.AddDays(1).Add(HoraEnvio);

            var diasSeleccionados = DiasPersonalizados.Split(',')
                .Select(d => int.Parse(d.Trim())).ToList();

            for (int i = 0; i < 7; i++)
            {
                var fechaCandidata = hoy.AddDays(i);
                var diaNumero = (int)fechaCandidata.DayOfWeek;
                if (diaNumero == 0) diaNumero = 7;

                if (diasSeleccionados.Contains(diaNumero))
                {
                    var horaCandidata = fechaCandidata.Add(HoraEnvio);
                    if (ahora < horaCandidata)
                        return horaCandidata;
                }
            }
            return hoy.AddDays(7).Add(HoraEnvio);

        case "pausada":
            return null;

        default:
            return DateTime.Now.Date.AddDays(1).Add(HoraEnvio);
    }
}
}

