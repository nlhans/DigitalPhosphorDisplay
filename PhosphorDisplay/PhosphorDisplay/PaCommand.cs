namespace PhosphorDisplay
{
    public enum PaCommand : ushort
    {
        GET_CAPABILITIES = 0x0000,
        GET_CALIBATION = 0x0010,
        SET_CALIBRATION = 0x8010,
        GET_CALIBRATOIN_PAGES = 0x0011,
        GET_STREAM_SETTINGS = 0x0020,
        SET_STREAM_SETTINGS = 0x8020,
        SET_STREAM_START = 0x8021,
        SET_STREAM_STOP = 0x8022,
        SET_STREAM_RESTART = 0x8023,
        STREAM_DATA = 0x0030,
    }
}