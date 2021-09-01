using System;
using System.Collections.Generic;

namespace MZPO.ucheba.ru.Models
{
    public class Response
    {
#pragma warning disable IDE1006 // Naming Styles
        public int total { get; set; }
        public List<Lead> items { get; set; }
        public Facets facets { get; set; }

        public class Facets
        {
            public List<Object> activityTypes { get; set; }
            public List<Object> educationLevels { get; set; }
            public List<Object> locations { get; set; }
            public List<Object> leadSourceTypes { get; set; }
            public List<Object> tags { get; set; }
            public List<Object> interestedProgramTypes { get; set; }
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}