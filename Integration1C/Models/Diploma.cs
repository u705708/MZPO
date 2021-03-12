using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration1C
{
    public class Diploma
    {
        public string Discipline { get; set; }
        public string Qualification { get; set; }
        public int Hours { get; set; } 
        public string EducationForm { get; set; }
        public string EducationType { get; set; }
        public string DiplomaNumber { get; set; }
        public DateTime DateOfIssue { get; set; }
        public Guid Client_Id_1C { get; set; }
    }
}
