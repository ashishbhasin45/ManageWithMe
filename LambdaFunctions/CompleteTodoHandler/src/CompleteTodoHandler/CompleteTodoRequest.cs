using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompleteTodoHandler
{
    public class CompleteTodoRequest
    {
        public string Email { get; set; }
        public int Token { get; set; }
        public List<string> TodoIds { get; set; }
    }
}
