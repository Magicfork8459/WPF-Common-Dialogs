using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monkeyshines
{
    

    public class ObservableColor : IObservable<Color>
    {
        public Color Color { get; private set; }

        private class Unsubscriber : IDisposable
        {
            private List<IObserver<Color>> observers;
            private IObserver<Color> observer;

            public Unsubscriber(IObserver<Color> observer, List<IObserver<Color>> observers)
            {
                this.observer = observer;
                this.observers = observers;
            }

            public void Dispose()
            {
                if(observers.Contains(observer))
                {
                    observers.Remove(observer);
                }
            }
        }

        private List<IObserver<Color>> observers;

        public ObservableColor(Color color)
        {
            Color = color;
            observers = new List<IObserver<Color>>();
        }

        public IDisposable Subscribe(IObserver<Color> observer)
        {
            
            observers.Add(observer);
            return new Unsubscriber(observer, observers);
        }

        public void UpdateColor(Color color)
        {
            foreach(var observer in observers)
            {
                observer.OnNext(color);
            }

            Color = color;
        }
    }
}
