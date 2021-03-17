using System;

namespace Integration1C
{
    public class Diploma
    {
#pragma warning disable IDE1006 // Naming Styles
        public string discipline { get; set; }
        public string qualification { get; set; }
        public int hours { get; set; } 
        public string educationForm { get; set; }
        public string educationType { get; set; }
        public string diplomaNumber { get; set; }
        public DateTime dateOfIssue { get; set; }
        public Guid client_Id_1C { get; set; }
#pragma warning restore IDE1006 // Naming Styles    
    }
}