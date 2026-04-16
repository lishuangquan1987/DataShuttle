using DataShuttle.Core.Helper;
using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataShuttle.Transports.SerialPort
{
    public class SerialPortTransport : ITransport
    {
        private System.IO.Ports.SerialPort _serialPort;
        private SerialPortTransportOptions _options;
        private CancellationTokenSource _startTokenSource;
        private CancellationTokenSource _readTokenSource = new CancellationTokenSource();
        private bool _isConnected;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<TransportErrorArgs> OnError;

        public bool IsConnected => _isConnected;
        public string Name => "串口";

        public SerialPortTransport() { }
        private SerialPortTransport(SerialPortTransportOptions options) { _options = options; }

        public static SerialPortTransport Create(SerialPortTransportOptions options) => new SerialPortTransport(options);

        public void Dispose() => Stop();

        public async Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            if (_startTokenSource == null) return OperationResult<byte[]>.NG("Transport is not started.");

            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _readTokenSource.Token, _startTokenSource.Token).Token;
            await MethodHelper.Delay(linkedToken);

            if (token.IsCancellationRequested) return OperationResult<byte[]>.NG("Read operation was cancelled.");
            if (_startTokenSource.IsCancellationRequested) return OperationResult<byte[]>.NG("Transport has been stopped.");

            try
            {
                var bytes = new byte[_serialPort.BytesToRead];
                _serialPort.Read(bytes, 0, bytes.Length);
                return OperationResult<byte[]>.OK(bytes);
            }
            catch (Exception e)
            {
                return OperationResult<byte[]>.NG(e);
            }
            finally
            {
                _readTokenSource = new CancellationTokenSource();
            }
        }

        public async Task Run()
        {
            await Stop();

            _startTokenSource = new CancellationTokenSource();
            _ = Task.Factory.StartNew(async () =>
            {
                var token = _startTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (_serialPort != null && _serialPort.IsOpen)
                        {
                            await MethodHelper.Delay(TimeSpan.FromSeconds(1), token);
                            continue;
                        }

                        SetConnectionStatus(false);

                        if (_serialPort != null)
                        {
                            _serialPort.DataReceived -= SerialPort_DataReceived;
                            _serialPort.Dispose();
                        }

                        _serialPort = new System.IO.Ports.SerialPort()
                        {
                            PortName = _options.PortName,
                            BaudRate = _options.BauRate,
                            Parity = _options.Parity,
                            DataBits = _options.DataBits,
                            StopBits = _options.StopBits,
                            ReadTimeout = 1000,
                            WriteTimeout = 1000,
                            ReceivedBytesThreshold = 1
                        };

                        _serialPort.DataReceived += SerialPort_DataReceived;
                        _serialPort.Open();
                        SetConnectionStatus(true);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(TransportErrorArgs.Create("Connect", ex.Message, ex));
                        await MethodHelper.Delay(TimeSpan.FromSeconds(1), token);
                    }
                }
            });
        }

        private void SetConnectionStatus(bool isConnected)
        {
            if (_isConnected == isConnected) return;
            _isConnected = isConnected;
            OnConnectionStatusChanged?.Invoke(_isConnected);
        }

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            _readTokenSource.Cancel();
        }

        public async Task Stop()
        {
            _startTokenSource?.Cancel();
            if (_serialPort != null)
            {
                try
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                }
                catch { }
            }
        }

        public async Task<OperationResult> Write(byte[] data, CancellationToken token)
        {
            try
            {
                if (!IsConnected) return OperationResult.NG("Serial port is not connected.");
                _serialPort.Write(data, 0, data.Length);
                return OperationResult.OK();
            }
            catch (Exception e)
            {
                OnError?.Invoke(TransportErrorArgs.Create("Write", e.Message, e));
                return OperationResult.NG(e);
            }
        }
    }
}
