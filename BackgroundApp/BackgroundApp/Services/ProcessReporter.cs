using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundApp.Services
{
    public class ProcessReporter : IProcessReporter
    {
        //робимо екземпляр, через який даємо такі методи як: шлях та запис у файл з перезаписом

        private readonly string _filePath;

        public ProcessReporter(string filePath)
        {
            _filePath = filePath;
        }

        public void SaveReport(List<string> lines)
        {
            lines.Sort();
            File.WriteAllLines(_filePath, lines);
        }
    }
}
