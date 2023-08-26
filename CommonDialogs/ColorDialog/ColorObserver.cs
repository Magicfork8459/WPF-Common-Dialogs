using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyshines
{
    public class ColorObserver : IObserver<Color>
    {
        private Color Color = new Color();
        public ColorChangedEventHandler? ColorChanged;

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Color value)
        {
            ColorChanged?.Invoke(this, new ColorChangedEventArgs() { NewColor = value });
            Color = value;
        }
    }
}
