
namespace WpfAppAITest.Interfaces
{
    public interface IBusyWindow : IDisposable
    {
        Task ShowAsync(string message);
        void Close();
    }
}
