using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MZPO.Data
{
    public class CF
    {
        public int Id { get; set; }
        public int AmoId { get; set; }
        public string Name { get; set; }
        public string EntityName { get; set; }
    }
}
