using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Application.Exceptions
{
    public class DuplicateDogNameException : Exception
    {
        public DuplicateDogNameException(string name)
            : base($"Dog with name '{name}' already exists.") { }
    }
}
