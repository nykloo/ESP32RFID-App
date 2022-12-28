using ESP32RFID.Services;
using ESP32RFID.ViewModels;
using Moq;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var client = new Mock<IESP32RfidClient>();
            var vm = new RawRfidViewModel(client.Object);
            //assert rfid daata length
            //test full scan
            //test word object seperatly
            //

        }
    }
}