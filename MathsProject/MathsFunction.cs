using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathsProject
{
    /*
Macro class for MathsProject
Copyright (C) 2024 Jordan Ferrazza

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
    public class MathsFunction
    {
        public string Name { get; set; }
        public string[] Arguments { get; set; }
        public string[] Commands { get; set; }
        public string Run(Maths from, params string[] args)
        {
            if (args.Length != Arguments.Length) throw new ArgumentException("Incorrect number of arguments.");

            for (int i = 0; i < args.Length; i++)
            {
                from.Variables[Arguments[i]] = args[i];
            }

            return Commands.Select(x => from.Run(x)).Last();
        }

        public MathsFunction(string name, string[] arguments, string[] commands)
        {
            Name = name;
            Arguments = arguments;
            Commands = commands;
        }
    }
}
