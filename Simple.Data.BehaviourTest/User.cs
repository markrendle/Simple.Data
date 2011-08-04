using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.IntegrationTest
{
    class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
    }
    class RogueUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
        public int RogueProperty { get; set; }
    }
}
