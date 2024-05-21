using MathsProject;

namespace TestMathsProject
{

    /*
    CLI of MathsProject
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

    internal class Program
    {

        static Maths math = new Maths();
        static string _com = "";
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Console.WriteLine();
                    Console.Write("> ");
                    var com = Console.ReadLine();
                    if (com == "") com = _com;
                    Console.WriteLine(math.Run(com));
                    _com = com;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}