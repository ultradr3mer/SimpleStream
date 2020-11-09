using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStream.Services
{
  public class ConsoleWraper
  {
    public event EventHandler<string> ConsoleWritten;
    
    internal void WriteLine(string line)
    {
      ConsoleWritten?.Invoke(this, line + Environment.NewLine);
    }

    internal void WriteLine()
    {
      ConsoleWritten?.Invoke(this, Environment.NewLine);
    }
  }
}
