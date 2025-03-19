using WpfAppAITest.Interfaces;
using WpfAppAITest.Views;

namespace WpfAppAITest.Services
{
    public class BusyWindowService : IBusyWindow
    {
        private BusyWindow _busyWindow;
        private Task _task;

        public async Task ShowAsync(string message)
        {
            if(_busyWindow != null) return;

            _busyWindow = new BusyWindow
            {
                MessageBlock =
                {
                    Text = !string.IsNullOrEmpty(message) ? message : "Loading.... Please wait."
                }
            };

            _task = Task.Run(() => { _busyWindow.Dispatcher.Invoke(() => _busyWindow.ShowDialog()); });

            await Task.Delay(100);
        }

        public void Close()
        {
            
            _busyWindow.Dispatcher.Invoke(() => _busyWindow.Close());
            _busyWindow = null;
        }

        public void Dispose()
        {
            Close();
        }
    }
}
