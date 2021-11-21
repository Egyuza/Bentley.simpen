
namespace Shared.Bentley
{
    public class KeyinOptions
    {
        public bool IsLogging { get; }
        public bool IsDebug { get; }

        public KeyinOptions(string unparsed)
        {
            if (unparsed == null)
                return;

            foreach (string par in unparsed.ToUpper().Split(' '))
            {
                switch(par)
                {
                case "DEBUG": IsDebug = true; break;
                case "LOG": IsLogging = true; break;
                }
            }

            applyOptions();
        }

        public void applyOptions()
        {
            Logger.IsActive = IsLogging;

            if (IsDebug)
            {
                Logger.Log.Info("запущено в DEBUG режиме");
            }
        }
    }
}
