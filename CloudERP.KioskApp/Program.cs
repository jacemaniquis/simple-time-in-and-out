using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using OpenQA.Selenium;
using System.Diagnostics;
using PCSC;
using PCSC.Iso7816;
using System.Windows.Forms;
using System.Drawing;
using System.Media;

namespace CloudERP.KioskApp
{
    class Program
    {
        static string _lastId;

        static bool _connected;

        static object syncRoot = new Object();

        static bool _deviceConnected;


        static IWebDriver driver;

        static IWebElement serial, body;



        static void Main(string[] args)
        {
            Console.WriteLine("Cloud Kiosk is started.");

            CheckDevice();

            var options = new ChromeOptions();
            options.AddArgument(@"user-data-dir=" + ConfigurationManager.AppSettings["ChromeUserDataPath"]);

            if (ConfigurationManager.AppSettings["KioskMode"] == "true")
            {
                options.AddArgument("--kiosk");
            }

            var driverPath = System.IO.Directory.GetCurrentDirectory();
            driver = new ChromeDriver(driverPath, options);

            driver.Navigate().GoToUrl(ConfigurationManager.AppSettings["KioskWebURL"]);

            while (true)
            {
                System.Threading.Thread.Sleep(500);

                if (ConfigurationManager.AppSettings["EnableMouseLimitation"] == "true")
                {
                    MouseFunction(driver);
                }

                lock (syncRoot)
                {
                    Timer_Elapsed(driver);
                }
            }

        }

        private static void MouseFunction(IWebDriver driver)
        {
            try
            {
                var body = driver.FindElement(By.TagName("Body"));
                var width = body.Size.Width;
                var treshold = body.Size.Width * .20;


                if (Control.MousePosition.X < treshold)
                {
                    Cursor.Position = new Point(Convert.ToInt32(treshold), Cursor.Position.Y);
                }
                if (Control.MousePosition.X > (width - treshold))
                {
                    Cursor.Position = new Point(width - Convert.ToInt32(treshold), Cursor.Position.Y);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void CheckDevice()
        {
            if (ConfigurationManager.AppSettings["EnableDeviceDetection"] == "false")
            {
                _deviceConnected = true;
            }

            while (!_deviceConnected)
            {
                using (var context = new SCardContext())
                {
                    context.Establish(SCardScope.System);
                    var readerNames = context.GetReaders();
                    if (readerNames.Any())
                    {
                        _deviceConnected = true;

                        var readerName = readerNames[0];
                        if (readerName == null)
                        {
                            Console.WriteLine("No NFC Device detected.");
                        }

                        if (ConfigurationManager.AppSettings["DisableDeviceBeep"] == "true")
                        {
                            using (var rfidReader = new SCardReader(context))
                            {
                                var sc = rfidReader.Connect(readerName, SCardShareMode.Direct, SCardProtocol.Unset);

                                var beepApdu = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Unset)
                                {
                                    CLA = 0xFF,
                                    Instruction = 0x00,
                                    P1 = 0x52,
                                    P2 = 0x00,
                                    Le = 0x00 // We don't know the ID tag size
                                };

                                var beepCommand = beepApdu.ToArray();
                                var receiveBuffer = new byte[256];

                                sc = rfidReader.Control((IntPtr)(0x31 << 16 | 3500 << 2), beepCommand, ref receiveBuffer);
                                if (sc != SCardError.Success)
                                {

                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No NFC Device detected.");
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
        }


        private static void Timer_Elapsed(IWebDriver driver)
        {
            try
            {
                if (!_deviceConnected)
                {
                    CheckDevice();
                }

                using (var context = new SCardContext())
                {
                    context.Establish(SCardScope.System);
                    var readerNames = context.GetReaders();
                    if (!readerNames.Any())
                    {
                        _deviceConnected = false;
                        return;
                    }
                    var readerName = readerNames[0];
                    if (readerName == null)
                    {
                        _deviceConnected = false;
                        return;
                    }

                    using (var rfidReader = new SCardReader(context))
                    {

                        var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                        if (sc != SCardError.Success)
                        {
                            Console.WriteLine("{0}: Listening...", DateTime.Now);
                            _connected = false;
                            //Console.WriteLine("Could not connect to reader {0}:\n{1}", readerName, SCardHelper.StringifyError(sc));
                            //Console.ReadKey();
                            return;
                        }

                        var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.ActiveProtocol)
                        {
                            CLA = 0xFF,
                            Instruction = InstructionCode.GetData,
                            P1 = 0x00,
                            P2 = 0x00,
                            Le = 0  // We don't know the ID tag size
                        };

                        sc = rfidReader.BeginTransaction();
                        if (sc != SCardError.Success)
                        {
                            _connected = false;
                            Console.WriteLine("Could not begin transaction." + DateTime.Now);
                            //Console.ReadKey();
                            return;
                        }

                        //Console.WriteLine("Retrieving the UID .... ");

                        var receivePci = new SCardPCI(); // IO returned protocol control information.
                        var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                        var receiveBuffer = new byte[256];
                        var command = apdu.ToArray();

                        sc = rfidReader.Transmit(
                            sendPci,            // Protocol Control Information (T0, T1 or Raw)
                            command,            // command APDU
                            receivePci,         // returning Protocol Control Information
                            ref receiveBuffer); // data buffer

                        if (sc != SCardError.Success)
                        {
                            _connected = false;
                            //Console.WriteLine("Error: " + SCardHelper.StringifyError(sc) + " " + DateTime.Now);
                            return;
                        }

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        /*Console.Write("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                            responseApdu.SW1,
                            responseApdu.SW2,
                            responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");*/

                        //rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        //rfidReader.Disconnect(SCardReaderDisposition.Reset);

                        if (responseApdu.HasData)
                        {
                            if (!_connected)
                            {
                                var id = BitConverter.ToString(responseApdu.GetData());

                                /*if (_lastId == id)
                                {
                                    System.Threading.Thread.Sleep(2000);
                                }*/

                                //Console.WriteLine("UID Retrieved.");
                                //SendKeys.SendWait(id + "{Enter}");

                                serial = driver.FindElement(By.Id("serial"));
                                if (serial != null)
                                {
                                    IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                                    js.ExecuteScript(string.Format("document.getElementById('serial').innerHTML='{0}'", id));
                                    js.ExecuteScript("arguments[0].click();", serial);

                                    if (ConfigurationManager.AppSettings["EnableCustomBeep"] == "true")
                                    {
                                        var simpleSound = new SoundPlayer(System.IO.Directory.GetCurrentDirectory() + "\\beep_success.wav");
                                        simpleSound.Play();
                                    }
                                }

                                _lastId = id;
                                _connected = true;

                                Console.WriteLine("{0}: {1} UID Retrieved.", DateTime.Now, id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + DateTime.Now);
            }
        }


        private static void Test(IWebDriver driver)
        {
            serial = driver.FindElement(By.Id("serial"));
            if (serial != null)
            {
                IJavaScriptExecutor js = driver as IJavaScriptExecutor;
                js.ExecuteScript(string.Format("document.getElementById('serial').innerHTML='{0}'", "JACE"));
                js.ExecuteScript("arguments[0].click();", serial);
            }
        }
    }
}
