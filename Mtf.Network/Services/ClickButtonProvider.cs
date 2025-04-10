namespace Mtf.Network.Services
{
    public static class ClickButtonProvider
    {
        public static int Get(string clickType)
        {
            if (clickType.StartsWith("down "))
            {
                return 0;
            }
            else if (clickType.StartsWith("up "))
            {
                return 1;
            }
            else if (clickType.StartsWith("click "))
            {
                return 2;
            }
            else if (clickType.StartsWith("doubleclick "))
            {
                return 3;
            }
            return -1;
        }
    }
}
