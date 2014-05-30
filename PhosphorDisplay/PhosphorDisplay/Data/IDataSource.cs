using System;

namespace PhosphorDisplay.Data
{
    public delegate void DataSourceEvent(DataPacket data);

    public interface IDataSource
    {
        event DataSourceEvent Data;
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