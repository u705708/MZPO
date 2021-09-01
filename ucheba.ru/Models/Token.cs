using System;

namespace MZPO.ucheba.ru.Models
{
    public class Token
    {
#pragma warning disable IDE1006 // Naming Styles
        public string token { get; set; }
        public DateTime expiresAt { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}