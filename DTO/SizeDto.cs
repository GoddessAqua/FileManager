using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.DTO
{
    internal class SizeDto
    {
        public double Size { get; set; }
        public string Name { get; set; }

        public SizeDto(double size, string name)
        {
            Size = size;
            Name = name;
        }
    }
}
