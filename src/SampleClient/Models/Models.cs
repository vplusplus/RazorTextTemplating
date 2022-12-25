using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient.Models
{
    internal class Cls
    {
        public string? Namespace { get; set; } = "Models";
        public string? Name { get; set; }
        public List<Prop> Properties { get; } = new();

    }

    internal class Prop
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
    }
}
