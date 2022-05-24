using FileManager.DTO;
using System;
using System.IO;
using FileManager;

namespace FileManager.Services
{
    internal static class DirectorySizeCalculationService
    { 
        public static double SizeOfDirectory(string path, ref double commonSize)
        {
            try
            {
                DirectoryInfo currentDir = new(path);
                DirectoryInfo[] subDirs = currentDir.GetDirectories();
                FileInfo[] files = currentDir.GetFiles();

                foreach (var file in files)
                {
                    commonSize += file.Length;
                }

                foreach (var subDir in subDirs)
                {
                    SizeOfDirectory(subDir.FullName, ref commonSize);
                }

                return Math.Round((double)(commonSize), 1);
            }
            catch
            {
                return 0;
            }
        }

        public static SizeDto DefineMeasurement(double size)
        {
            switch (size)
            {
                case var newSize when size >= 1024 && size < Math.Pow(1024, 2):
                default:
                    {
                        newSize = Math.Round(size * 1.0 / 1024, 2, MidpointRounding.AwayFromZero);
                        return new SizeDto(newSize, UnitsOfInformation.KB.ToString());
                    }
                case var newSize when size >= Math.Pow(1024, 2) && size < Math.Pow(1024, 3):
                    {
                        newSize = Math.Round(size * 1.0 / Math.Pow(1024, 2), 2, MidpointRounding.AwayFromZero);
                        return new SizeDto(newSize, UnitsOfInformation.MB.ToString());
                    }
                case var newSize when size >= Math.Pow(1024, 3):
                    {
                        newSize = Math.Round(size * 1.0 / Math.Pow(1024, 3), 2, MidpointRounding.AwayFromZero);
                        return new SizeDto(newSize, UnitsOfInformation.GB.ToString());
                    }
            }
        }

        public static void CastToKB(SizeDto obj)
        {
            if (obj.Name.Equals(UnitsOfInformation.MB.ToString()))
                obj.Size *= 1024;
            else if (obj.Name.Equals(UnitsOfInformation.GB.ToString()))
                obj.Size *= Math.Pow(1024, 2);
        }
    }
}
