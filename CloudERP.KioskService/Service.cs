using PCSC;
using PCSC.Iso7816;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CloudERP.KioskService
{
    public partial class Service : ServiceBase
    {
        bool _isRunning;

        Task _task;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _task = new Task(() => TaskFunction());
            _task.Start();
        }

        private void TaskFunction()
        {
            _isRunning = true;
            while (_isRunning)
            {
                Timer_Elapsed();
            }
        }

        private void Timer_Elapsed()
        {
            try
            {
                Process[] pname = Process.GetProcessesByName("chrome");
                if (pname.Length == 0)
                    return;

                using (var context = new SCardContext())
                {
                    context.Establish(SCardScope.System);
                    var readerNames = context.GetReaders();
                    var readerName = readerNames[0];
                    if (readerName == null)
                    {
                        return;
                    }

                    using (var rfidReader = new SCardReader(context))
                    {

                        var sc = rfidReader.Connect(readerName, SCardShareMode.Shared, SCardProtocol.Any);
                        if (sc != SCardError.Success)
                        {
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
                            //Console.WriteLine("Could not begin transaction.");
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

                        /*if (sc != SCardError.Success)
                        {
                            Console.WriteLine("Error: " + SCardHelper.StringifyError(sc));
                        }*/

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        /*Console.Write("SW1: {0:X2}, SW2: {1:X2}\nUid: {2}",
                            responseApdu.SW1,
                            responseApdu.SW2,
                            responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received");*/

                        rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        rfidReader.Disconnect(SCardReaderDisposition.Reset);

                        if(responseApdu.HasData)
                        {
                            SendKeys.SendWait(BitConverter.ToString(responseApdu.GetData()));
                            System.Threading.Thread.Sleep(2000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnStop()
        {
            _isRunning = false;
            _task.Dispose();
        }
    }
}
