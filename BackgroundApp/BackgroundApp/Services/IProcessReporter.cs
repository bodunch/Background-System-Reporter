using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundApp.Services
{
    public interface IProcessReporter
    {
        //приймаємо список рядків які треба записати, Це інтерфейс рядків у які будуть записані логи
        void SaveReport(List<string> lines);
    }
}
