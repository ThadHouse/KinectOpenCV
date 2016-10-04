namespace TankTracker.Base.Structs
{
    public struct NetworkedValue<T>
    {
        public T MinimumValue { get; }
        public T MaximumValue { get; }
        public T DefaultValue { get; }

        public NetworkedValue(T minVal, T maxVal, T defaultVal)
        {
            MinimumValue = minVal;
            MaximumValue = maxVal;
            DefaultValue = defaultVal;
        }
    }
}
