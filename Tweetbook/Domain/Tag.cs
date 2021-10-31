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
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedBy { get; set; }
    }
}
