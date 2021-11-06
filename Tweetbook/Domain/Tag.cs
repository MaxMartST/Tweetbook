using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tweetbook.Domain
{
    public class Tag
    {
        [Key]
        public string Name { get; set; }
        public DateTime CreatedBy { get; set; }
    }
}
