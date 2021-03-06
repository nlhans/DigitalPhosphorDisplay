using System;

namespace PhosphorDisplay.Data
{
    public delegate void DataSourceEvent(DataPacket data);
    public delegate void HighresEvent(float voltage);

    public interface IDataSource
    {
        float SampleRate { get; }

        event DataSourceEvent Data;
        event HighresEvent HighresVoltage;
        event EventHandler Connected;
        event EventHandler Disconnected;

        // Start connect
        void Connect(object target);
        void Disconnect();

        // Will re-start scope
        void Configure(object configuration);

        // Start/stop
        void Start();
        void Stop();
    }
}