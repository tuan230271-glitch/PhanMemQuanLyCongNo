namespace PhanMemQuanLyCongNo.Domain.ValueObjects;

public readonly record struct SoTien(decimal Value)
{
    public static SoTien Zero => new(0);

    public static SoTien From(decimal value)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "So tien khong duoc am.");
        }

        return new SoTien(value);
    }

    public override string ToString() => Value.ToString("n0");
}
